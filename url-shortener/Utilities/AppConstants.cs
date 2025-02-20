namespace url_shortener.Utilities
{
    public class AppConstants
    {
        public static readonly string DEFAULT_ERROR_MESSAGE = "Something went wrong, please try again.";
        public static readonly string INVALID_URL_ERROR_MESSAGE = "Invalid url.";
        
        public static readonly string URLS_REPOSITORY = "UrlsRepository";

        public static readonly string BASECHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static readonly int MIN_SHORTURL_LENGTH = 6;
        public static readonly int MAX_SHORTURL_LENGTH = 8;
        public static readonly string BASE_URL = "short.url/";
    }
}
