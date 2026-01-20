using API.Domain.Entities.Review;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Commands;

public record AddInstashopSourceReviewSummaryCommand(
    long LocationID,
    long CompanyID,
    double? InstashopAvgRating, 
    int? InstashopTotalResponses
) : IRequest<RequestResult<Unit>>;

public class AddInstashopSourceReviewSummaryCommandHandler : IRequestHandler<AddInstashopSourceReviewSummaryCommand, RequestResult<Unit>>
{
    private readonly IRepository<SourceReviewSummary> _repository;

    public AddInstashopSourceReviewSummaryCommandHandler(
        IRepository<SourceReviewSummary> repository)
    {
        _repository = repository;
    }

    public async Task<RequestResult<Unit>> Handle(AddInstashopSourceReviewSummaryCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var exists = await _repository
            .Get(x => x.LocationID == request.LocationID && x.Date == today)
            .AnyAsync(cancellationToken);

        if (exists)
        {
            return RequestResult<Unit>.Success(Unit.Value);
        }

        _repository.Add(new SourceReviewSummary
        {
            LocationID = request.LocationID,
            CompanyID = request.CompanyID,
            InstashopAvgRating = request.InstashopAvgRating,
            InstashopTotalResponses = request.InstashopTotalResponses,
            Date = today
        });

        await _repository.SaveChangesAsync(cancellationToken);

        return RequestResult<Unit>.Success(Unit.Value);
    }
}
