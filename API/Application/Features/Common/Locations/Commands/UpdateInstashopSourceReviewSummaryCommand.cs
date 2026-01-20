using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands;
public record UpdateInstashopSourceReviewSummaryCommand(
    long ID,
    long CompanyID,
    long LocationID,
    double InstashopAvgRating,
    int InstashopTotalResponses
) : IRequest<RequestResult<Unit>>;

public class UpdateInstashopSourceReviewSummaryCommandHandler
    : IRequestHandler<UpdateInstashopSourceReviewSummaryCommand, RequestResult<Unit>>
{
    private readonly IRepository<Domain.Entities.Review.SourceReviewSummary> _repository;

    public UpdateInstashopSourceReviewSummaryCommandHandler(
        IRepository<Domain.Entities.Review.SourceReviewSummary> repository)
    {
        _repository = repository;
    }

    public async Task<RequestResult<Unit>> Handle(UpdateInstashopSourceReviewSummaryCommand request, CancellationToken cancellationToken)
    {
        _repository.SaveIncluded(
            new Domain.Entities.Review.SourceReviewSummary
            {
                ID = request.ID,
                CompanyID = request.CompanyID,
                LocationID = request.LocationID,
                AvgRate = request.InstashopAvgRating,
                TotalResponses = request.InstashopTotalResponses
            },
            nameof(Domain.Entities.Review.SourceReviewSummary.CompanyID),
            nameof(Domain.Entities.Review.SourceReviewSummary.AvgRate),
            nameof(Domain.Entities.Review.SourceReviewSummary.TotalResponses)
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return RequestResult<Unit>.Success(Unit.Value);
    }
}
