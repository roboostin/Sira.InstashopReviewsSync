using API.Helpers;

namespace API.Domain.Enums
{
    public enum DayOfWeek
    {
        [DescriptionAnnotation("الأحد", "Sunday")]
        Sunday = 1,

        [DescriptionAnnotation("الاثنين", "Monday")]
        Monday = 2,

        [DescriptionAnnotation("الثلاثاء", "Tuesday")]
        Tuesday = 3,

        [DescriptionAnnotation("الأربعاء", "Wednesday")]
        Wednesday = 4,

        [DescriptionAnnotation("الخميس", "Thursday")]
        Thursday = 5,

        [DescriptionAnnotation("الجمعة", "Friday")]
        Friday = 6,

        [DescriptionAnnotation("السبت", "Saturday")]
        Saturday = 7
    }
}
