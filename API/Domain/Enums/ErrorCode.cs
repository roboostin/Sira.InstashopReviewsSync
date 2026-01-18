using API.Helpers;

namespace API.Domain.Enums
{
    public enum ErrorCode
    {
        [DescriptionAnnotation("none", "none")]
        None = 0,

        [DescriptionAnnotation("غير موجود", "Not found")]
        NotFound =1,

        [DescriptionAnnotation("No Summaries Found", "No Summaries Found")]
        NoSummariesFound = 2,

        [DescriptionAnnotation("No Locations Found", "No Locations Found")]
        NoLocationsFound = 3,

        [DescriptionAnnotation("Save Reviews Failed", "Failed to save reviews")]
        SaveReviewsFailed = 4,

        [DescriptionAnnotation("Process Location Reviews Failed", "Failed to process location reviews")]
        ProcessLocationReviewsFailed = 5,

        [DescriptionAnnotation("Update Source Review Summary Failed", "Failed to update source review summary")]
        UpdateSourceReviewSummaryFailed = 6,

        [DescriptionAnnotation("Add Source Review Summary Failed", "Failed to add source review summary")]
        AddSourceReviewSummaryFailed = 7,

        [DescriptionAnnotation("Upsert Source Review Summary Failed", "Failed to upsert source review summary")]
        UpsertSourceReviewSummaryFailed = 8,

        [DescriptionAnnotation("Get Non Processed Reviews Failed", "Failed to get non-processed reviews")]
        GetNonProcessedReviewsFailed = 9,

        [DescriptionAnnotation("Mark Reviews As Processed Failed", "Failed to mark reviews as processed")]
        MarkReviewsAsProcessedFailed = 10,

        [DescriptionAnnotation("Get Source Review Summary Existence Failed", "Failed to get source review summary existence")]
        GetSourceReviewSummaryExistenceFailed = 11,

        [DescriptionAnnotation("Get Location Review Count Failed", "Failed to get location review count")]
        GetLocationReviewCountFailed = 12,

        [DescriptionAnnotation("No reviews found", "No reviews found")]
        NoReviewsFound = 13,
    }
}
