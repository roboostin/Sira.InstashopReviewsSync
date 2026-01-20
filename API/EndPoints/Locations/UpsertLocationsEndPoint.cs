using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Shared.Models;
using MediatR;

namespace API.EndPoints.Locations
{
    public class UpsertLocationsEndPoint : EndpointDefinition
    {
        public override void RegisterEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/Location/AddLocationsFromInstashop",
                async (List<InstashopServiceLocationDTO> locations, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    if (locations == null || locations.Count == 0)
                    {
                        return Response(RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None, "No locations provided"));
                    }

                    var result = await mediator.Send(
                        new UpsertLocationsFromInstashopCommand(locations),
                        cancellationToken);

                    return Response(result);
                })
            .WithTags("Location")
            .WithName("AddLocationsFromInstashop")
            .Accepts<List<InstashopServiceLocationDTO>>("application/json")
            .Produces<EndPointResponse<bool>>();
        }
    }
}

