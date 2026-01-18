using API.Helpers;

namespace API.Domain.Enums
{
    public enum ReviewRate
    {
        [DescriptionAnnotation("نجمة واحدة","One Star")]
        OneStar = 1,
        [DescriptionAnnotation("نجمتان","Two Stars")]
        TwoStars = 2,
        [DescriptionAnnotation("ثلاث نجوم","Three Stars")]
        ThreeStars = 3,
        [DescriptionAnnotation("أربع نجوم","Four Stars")]
        FourStars = 4,
        [DescriptionAnnotation("خمس نجوم","Five Stars")]
        FiveStars = 5
    }
}
