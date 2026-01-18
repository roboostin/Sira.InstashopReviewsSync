using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands;
public record UpdateTalabatSourceReviewSummaryCommand(
    long ID,
    long CompanyID,
    long LocationID,
    double TalabatAvgRating,
    int TalabatTotalResponses
) : IRequest<RequestResult<Unit>>;

public class UpdateTalabatSourceReviewSummaryCommandHandler
    : IRequestHandler<UpdateTalabatSourceReviewSummaryCommand, RequestResult<Unit>>
{
    private readonly IRepository<Domain.Entities.Review.SourceReviewSummary> _repository;

    public UpdateTalabatSourceReviewSummaryCommandHandler(
        IRepository<Domain.Entities.Review.SourceReviewSummary> repository)
    {
        _repository = repository;
    }

    public async Task<RequestResult<Unit>> Handle(UpdateTalabatSourceReviewSummaryCommand request, CancellationToken cancellationToken)
    {
        _repository.SaveIncluded(
            new Domain.Entities.Review.SourceReviewSummary
            {
                ID = request.ID,
                CompanyID = request.CompanyID,
                LocationID = request.LocationID,
                AvgRate = request.TalabatAvgRating,
                TotalResponses = request.TalabatTotalResponses
            },
            nameof(Domain.Entities.Review.SourceReviewSummary.CompanyID),
            nameof(Domain.Entities.Review.SourceReviewSummary.AvgRate),
            nameof(Domain.Entities.Review.SourceReviewSummary.TotalResponses)
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return RequestResult<Unit>.Success(Unit.Value);
    }
}