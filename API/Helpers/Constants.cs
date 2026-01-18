namespace API.Helpers;

public class Constants
{
    public static string TOKEN_PREFIX = "Rb$T";
    public static int DBCommandTimeout { get; set; } = 300;
    public static readonly string _TEMP_UPLOAD_PATH = "temp";
    public static readonly string Media_Service_API = "https://media-api.roboost.app";
    public static readonly string Review_API = "https://sira-feedback.roboost.app/qrcode/";
    public const string GoogleMapsReviews = "compass~google-maps-reviews-scraper";
    public static readonly string _ShortUrlClientName = "ShortUrlClient";
    public static readonly string GoogleMapsReviewsURLPrefix = "https://www.google.com/maps/reviews";
    public static readonly string AIReviewReplyAPI  = "https://auto-reply-lnfb.onrender.com";
    

}