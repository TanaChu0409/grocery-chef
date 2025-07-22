using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Web;
using GroceryChefClient.UI.Dtos.Common;

namespace GroceryChefClient.UI.Extensions;

public static class HttpExtensions
{
    public static NameValueCollection? GetQueryParameter<T> ([NotNull]T request)
    {
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
        foreach (PropertyInfo prop in request!.GetType().GetProperties())
        {
            if (prop.Name == nameof(BaseQueryRequest.Filters))
            {
                continue;
            }

            object? value = prop.GetValue(request);
            if (value is not null && value is not System.Collections.IEnumerable enumerable)
            {
                query[prop.Name] = value.ToString();
            }
        }

        return query;
    }
}
