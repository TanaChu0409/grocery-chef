using System.Buffers.Text;
using System.Security.Claims;
using System.Text.Json;

namespace GroceryChefClient.UI.Extensions;

public static class JwtParser
{
    public static List<Claim> ParseClaimsFromJwt(string jwtToken)
    {
        List<Claim> claims = [];
        string payload = jwtToken.Split('.')[1];

        byte[] jsonBytes = ParseBase64WithoutPadding(payload);

        Dictionary<string, object>? keyValuePairs = JsonSerializer
            .Deserialize<Dictionary<string, object>>(jsonBytes);

        claims.AddRange(keyValuePairs?.
            Select(kvp => 
                new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty)) ?? []);

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string payload)
    {
        switch (payload.Length % 4)
        {
            case 2:
                payload += "==";
                break;
            case 3:
                payload += "=";
                break;
        }

        return Convert.FromBase64String(payload);
    }
}
