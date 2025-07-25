﻿namespace GroceryChef.Api.Services;

public static class CustomMediaTypeNames
{
    public static class Application
    {
        public const string JsonV1 = "application/json;v=1";
        public const string HateoasJson = "application/vnd.grocerychef.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.grocerychef.hateoas.1+json";
        public const string HateoasSubType = "hateoas";
    }
}
