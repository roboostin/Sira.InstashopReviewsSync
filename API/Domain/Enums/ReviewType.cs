using API.Helpers;

namespace API.Domain.Enums
{
    public enum ReviewType
    {
        [DescriptionAnnotation("إيجابي","Positive")]
        Positive = 1,

        [DescriptionAnnotation("سلبي", "Complaint")]
        Complaint = 2,
    }
}
