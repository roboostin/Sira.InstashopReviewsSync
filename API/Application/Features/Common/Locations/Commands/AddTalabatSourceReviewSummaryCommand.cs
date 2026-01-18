using API.Domain.Entities.Review;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Commands;

public record AddTalabatSourceReviewSummaryCommand(
    long LocationID,
    long CompanyID,
    double? TalabatAvgRating, 
    int? TalabatTotalResponses
) : IRequest<RequestResult<Unit>>;

public class AddTalabatSourceReviewSummaryCommandHandler : IRequestHandler<AddTalabatSourceReviewSummaryCommand, RequestResult<Unit>>
{
    private readonly IRepository<SourceReviewSummary> _repository;

    public AddTalabatSourceReviewSummaryCommandHandler(
        IRepository<SourceReviewSummary> repository)
    {
        _repository = repository;
    }

    public async Task<RequestResult<Unit>> Handle(AddTalabatSourceReviewSummaryCommand request, CancellationToken cancellationToken)
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
            TalabatAvgRating = request.TalabatAvgRating,
            TalabatTotalResponses = request.TalabatTotalResponses,
            Date = today
        });

        await _repository.SaveChangesAsync(cancellationToken);

        return RequestResult<Unit>.Success(Unit.Value);
    }
}




