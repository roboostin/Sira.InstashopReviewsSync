using API.Application.Features.Common.Locations.Queries;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands;

public record UpSertTalabatSourceReviewSummaryOrchestrator(
    long LocationID,
    long CompanyID,
    double TalabatAvgRating,
    int TalabatTotalResponses
) : IRequest<RequestResult<Unit>>;

public class UpSertTalabatSourceReviewSummaryOrchestratorHandler
    : IRequestHandler<UpSertTalabatSourceReviewSummaryOrchestrator, RequestResult<Unit>>
{
    private readonly IMediator _mediator;

    public UpSertTalabatSourceReviewSummaryOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<RequestResult<Unit>> Handle(
        UpSertTalabatSourceReviewSummaryOrchestrator request,
        CancellationToken cancellationToken)
    {
        var sourceReviewsIdResult = await _mediator
            .Send(new UpSertTalabatSourceReviewSummaryExistenceQuery(LocationID: request.LocationID), cancellationToken)
            .ConfigureAwait(false);

        if (!sourceReviewsIdResult.IsSuccess)
        {
            return RequestResult<Unit>.Failure(sourceReviewsIdResult.ErrorCode, sourceReviewsIdResult.Message);
        }

        var sourceReviewsId = sourceReviewsIdResult.Data;

        if (sourceReviewsId != 0)
        {
            var updateResult = await _mediator.Send(
                new UpdateTalabatSourceReviewSummaryCommand(
                    ID: sourceReviewsId,
                    CompanyID: request.CompanyID,
                    LocationID: request.LocationID,
                    TalabatAvgRating: request.TalabatAvgRating,
                    TalabatTotalResponses: request.TalabatTotalResponses),
                cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsSuccess)
            {
                return RequestResult<Unit>.Failure(updateResult.ErrorCode, updateResult.Message);
            }
        }
        else
        {
            var addResult = await _mediator.Send(
                new AddTalabatSourceReviewSummaryCommand(
                    LocationID: request.LocationID,
                    CompanyID: request.CompanyID,
                    TalabatAvgRating: request.TalabatAvgRating,
                    TalabatTotalResponses: request.TalabatTotalResponses),
                cancellationToken).ConfigureAwait(false);

            if (!addResult.IsSuccess)
            {
                return RequestResult<Unit>.Failure(addResult.ErrorCode, addResult.Message);
            }
        }

        return RequestResult<Unit>.Success(Unit.Value);
    }
}



