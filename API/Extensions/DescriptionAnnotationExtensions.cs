using API.Domain.Enums;
using API.Helpers;

namespace API.Extensions;

public static class DescriptionAnnotationExtensions
{
    public static string GetDescription(this object obj, Language language = Language.Arabic)
    {
        return DescriptionAnnotation.GetDescription(obj, language);
    }
}