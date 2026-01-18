using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities.Review;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetLocationsWithReviewSummaryQuery() : IRequest<RequestResult<List<LocationReviewSummaryDto>>>;

    public class GetLocationsWithReviewSummaryQueryHandler : IRequestHandler<GetLocationsWithReviewSummaryQuery, RequestResult<List<LocationReviewSummaryDto>>>
    {
        private readonly IRepository<SourceReviewSummary> _summaryRepository;

        public GetLocationsWithReviewSummaryQueryHandler(IRepository<SourceReviewSummary> summaryRepository)
        {
            _summaryRepository = summaryRepository;
        }

        private readonly ILogger<GetLocationsWithReviewSummaryQueryHandler> _logger;

        public async Task<RequestResult<List<LocationReviewSummaryDto>>> Handle(GetLocationsWithReviewSummaryQuery request, CancellationToken cancellationToken)
        {
            var lastTwoDays = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

            var latestSummaries = _summaryRepository
                .Get(s => s.Date >= lastTwoDays)
                .Select(summary => new LocationReviewSummaryDto
                {
                    LocationID = summary.LocationID,
                    CompanyID = summary.CompanyID,
                    Rating = summary.AvgRate,
                    TotalResponseCount = (int)summary.TotalResponses
                }).ToList();

            if (latestSummaries.Any())
            {
                return await Task.FromResult(RequestResult<List<LocationReviewSummaryDto>>.Success(latestSummaries));
            }

            return await Task.FromResult(RequestResult<List<LocationReviewSummaryDto>>.Failure(ErrorCode.NoSummariesFound));
        }
    }
}

