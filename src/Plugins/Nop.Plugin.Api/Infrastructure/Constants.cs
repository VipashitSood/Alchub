using Nop.Core.Caching;

namespace Nop.Plugin.Api.Infrastructure
{
    public static class Constants
    {
        public static class Roles
        {
            public const string API_ROLE_SYSTEM_NAME = "ApiUserRole";

            public const string API_ROLE_NAME = "Api Users";
        }

        public static class ViewNames
        {
            public const string ADMIN_LAYOUT = "_AdminLayout";
        }

        public static class Configurations
        {
            public const int DEFAULT_ACCESS_TOKEN_EXPIRATION_IN_DAYS = 365; // 1 year

            // time is in seconds (10 years = 315360000 seconds) and should not exceed 2038 year
            // https://stackoverflow.com/questions/43593074/jwt-validation-fails/43605820
            //public const int DefaultAccessTokenExpiration = 315360000;
            //public const int DefaultRefreshTokenExpiration = int.MaxValue;
            public const int DEFAULT_LIMIT = 50;
            public const int DEFAULT_PAGE_VALUE = 1;
            public const int DEFAULT_SINCE_ID = 0;
            public const int DEFAULT_CUSTOMER_ID = 0;
            public const string DEFAULT_ORDER = "Id";
            public const int MAX_LIMIT = 250;

            public const int MIN_LIMIT = 1;

            //public const string PublishedStatus = "published";
            //public const string UnpublishedStatus = "unpublished";
            //public const string AnyStatus = "any";
            public static CacheKey JsonTypeMapsPattern => new CacheKey("json.maps");

            public static CacheKey NEWSLETTER_SUBSCRIBERS_KEY = new CacheKey("Nop.api.newslettersubscribers");
        }
    }
}
