using API.Application.Features.Common.Locations.Queries;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands;

public record UpSertInstashopSourceReviewSummaryOrchestrator(
    long LocationID,
    long CompanyID,
    double InstashopAvgRating,
    int InstashopTotalResponses
) : IRequest<RequestResult<Unit>>;

public class UpSertInstashopSourceReviewSummaryOrchestratorHandler
    : IRequestHandler<UpSertInstashopSourceReviewSummaryOrchestrator, RequestResult<Unit>>
{
    private readonly IMediator _mediator;

    public UpSertInstashopSourceReviewSummaryOrchestratorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<RequestResult<Unit>> Handle(
        UpSertInstashopSourceReviewSummaryOrchestrator request,
        CancellationToken cancellationToken)
    {
        var sourceReviewsIdResult = await _mediator
            .Send(new UpSertInstashopSourceReviewSummaryExistenceQuery(LocationID: request.LocationID), cancellationToken)
            .ConfigureAwait(false);

        if (!sourceReviewsIdResult.IsSuccess)
        {
            return RequestResult<Unit>.Failure(sourceReviewsIdResult.ErrorCode, sourceReviewsIdResult.Message);
        }

        var sourceReviewsId = sourceReviewsIdResult.Data;

        if (sourceReviewsId != 0)
        {
            var updateResult = await _mediator.Send(
                new UpdateInstashopSourceReviewSummaryCommand(
                    ID: sourceReviewsId,
                    CompanyID: request.CompanyID,
                    LocationID: request.LocationID,
                    InstashopAvgRating: request.InstashopAvgRating,
                    InstashopTotalResponses: request.InstashopTotalResponses),
                cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsSuccess)
            {
                return RequestResult<Unit>.Failure(updateResult.ErrorCode, updateResult.Message);
            }
        }
        else
        {
            var addResult = await _mediator.Send(
                new AddInstashopSourceReviewSummaryCommand(
                    LocationID: request.LocationID,
                    CompanyID: request.CompanyID,
                    InstashopAvgRating: request.InstashopAvgRating,
                    InstashopTotalResponses: request.InstashopTotalResponses),
                cancellationToken).ConfigureAwait(false);

            if (!addResult.IsSuccess)
            {
                return RequestResult<Unit>.Failure(addResult.ErrorCode, addResult.Message);
            }
        }

        return RequestResult<Unit>.Success(Unit.Value);
    }
}
