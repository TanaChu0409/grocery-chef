﻿using System.Security.Cryptography;
using System.Text;
using GroceryChef.Api.Services;

namespace GroceryChef.Api.Middleware;

public sealed class ETagMiddleware(RequestDelegate next)
{
    private static readonly string[] ConcurrencyCheckMethods =
    [
        HttpMethods.Put,
        HttpMethods.Patch
    ];
    public async Task InvokeAsync(HttpContext context, InMemoryETagStore eTagStore)
    {
        if (CanSkipETag(context))
        {
            await next(context);

            return;
        }

        string resourceUri = context.Request.Path.Value!;
        string? ifNoneMatch = context.Request.Headers.IfNoneMatch.FirstOrDefault()?.Replace("\"", "");
        string? ifMatch = context.Request.Headers.IfMatch.FirstOrDefault()?.Replace("\"", "");

        if (ConcurrencyCheckMethods.Contains(context.Request.Method) && !string.IsNullOrWhiteSpace(ifMatch))
        {
            string currentETag = eTagStore.GetETag(resourceUri);

            if (!string.IsNullOrWhiteSpace(currentETag) && ifMatch != currentETag)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                context.Response.ContentLength = 0;
                return;
            }
        }

        Stream originalStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        if (IsETaggableResponse(context))
        {
            memoryStream.Position = 0;
            byte[] responseBody = await GetResponseBody(memoryStream);
            string etag = GenerateETag(responseBody);

            eTagStore.SetETag(resourceUri, etag);
            context.Response.Headers.ETag = $"\"{etag}\"";
            context.Response.Body = originalStream;

            if (context.Request.Method == HttpMethods.Get && ifNoneMatch == etag)
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                context.Response.ContentLength = 0;
                return;
            }
        }

        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalStream);
    }

    private static bool IsETaggableResponse(HttpContext context) =>
        context.Response.StatusCode == StatusCodes.Status200OK &&
            (context.Response.Headers.ContentType
                .FirstOrDefault()?
                .Contains("json", StringComparison.OrdinalIgnoreCase) ?? false);

    private static async Task<byte[]> GetResponseBody(MemoryStream memoryStream)
    {
        using var reader = new StreamReader(memoryStream, leaveOpen: true);
        memoryStream.Position = 0;
        string content = await reader.ReadToEndAsync();
        return Encoding.UTF8.GetBytes(content);
    }

    private static string GenerateETag(byte[] content)
    {
        byte[] hash = SHA512.HashData(content);

        return Convert.ToHexString(hash);
    }

    private static bool CanSkipETag(HttpContext context) =>
        context.Request.Method == HttpMethods.Post ||
        context.Request.Method == HttpMethods.Delete;
}
