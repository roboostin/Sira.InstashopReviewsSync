namespace API.Helpers;

public class LanguageHelper
{
    private const string DEFAULT_LANG = "ar";
    private const string LANGUAGE_HEADER_NAME = "lang";

    public static bool IsArabic()
    {
        return
            string.IsNullOrEmpty(HttpRequestHelper.GetHeaderValue(LANGUAGE_HEADER_NAME))
            || HttpRequestHelper.GetHeaderValue(LANGUAGE_HEADER_NAME).ToLower() == DEFAULT_LANG;
    }
}