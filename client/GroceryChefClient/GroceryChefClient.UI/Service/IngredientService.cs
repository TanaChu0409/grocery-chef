﻿using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Web;
using GroceryChefClient.UI.Dtos.Common;
using GroceryChefClient.UI.Dtos.Ingredients;
using Microsoft.AspNetCore.Components.Authorization;

namespace GroceryChefClient.UI.Service;

public sealed class IngredientService(
    HttpClient httpClient,
    InMemoryTokenStore inMemoryTokenStore,
    AuthenticationStateProvider authenticationStateProvider)
{
    private const string IngredientUri = "ingredients";

    public async Task<List<IngredientDto>> GetIngredients(IngredientQueryRequest queryRequest)
    {
        try
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (PropertyInfo prop in queryRequest.GetType().GetProperties())
            {
                object? value = prop.GetValue(queryRequest);
                if (value is not null)
                {
                    query[prop.Name] = value.ToString();
                }
            }

            string uri = $"{IngredientUri}?{query}";
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            PaginationResult<IngredientDto>? ingredientWithPagination =
                await response.Content.ReadFromJsonAsync<PaginationResult<IngredientDto>>();

            return ingredientWithPagination?.Items ?? [];
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            return [];
        }
        catch
        {
            throw;
        }
    }

    public async Task<IngredientDto> GetIngredientAsync(string id)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
           new AuthenticationHeaderValue(
               "Bearer",
               await inMemoryTokenStore.GetTokenAsync());
            HttpResponseMessage response = await httpClient.GetAsync($"{IngredientUri}/{id}");

            response.EnsureSuccessStatusCode();

            IngredientDto? ingredient = await response.Content.ReadFromJsonAsync<IngredientDto>();

            if (ingredient is null)
            {
                throw new Exception("Ingredient not found");
            }

            return ingredient;
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();

            return default;
        }
        catch
        {
            throw;
        }
    }

    public async Task AddIngredient(CreateIngredientDto createIngredient)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"{IngredientUri}",
                createIngredient);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }

    public async Task UpdateIngredient(string id, UpdateIngredientDto updateIngredientDto)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                await inMemoryTokenStore.GetTokenAsync());

            HttpResponseMessage response = await httpClient.PutAsJsonAsync(
                $"{IngredientUri}/{id}",
                updateIngredientDto);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException httpEx)
            when (httpEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            ((AuthProvider)authenticationStateProvider).NotifyUserLogout();
        }
        catch
        {
            throw;
        }
    }
}
