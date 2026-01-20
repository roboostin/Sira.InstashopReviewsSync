using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries;

public record UpSertInstashopSourceReviewSummaryExistenceQuery(long LocationID) : IRequest<RequestResult<long>>;

public class UpSertInstashopSourceReviewSummaryExistenceQueryHandler
    : IRequestHandler<UpSertInstashopSourceReviewSummaryExistenceQuery, RequestResult<long>>
{
    private readonly IRepository<Domain.Entities.Review.SourceReviewSummary> _repository;

    public UpSertInstashopSourceReviewSummaryExistenceQueryHandler(
        IRepository<Domain.Entities.Review.SourceReviewSummary> repository)
    {
        _repository = repository;
    }

    public async Task<RequestResult<long>> Handle(
        UpSertInstashopSourceReviewSummaryExistenceQuery request,
        CancellationToken cancellationToken)
    {
        var id = await _repository.Get(element =>
                element.Date == DateOnly.FromDateTime(DateTime.UtcNow)
                && element.LocationID == request.LocationID)
            .Select(element => element.ID)
            .FirstOrDefaultAsync(cancellationToken);

        return RequestResult<long>.Success(id);
    }
}
