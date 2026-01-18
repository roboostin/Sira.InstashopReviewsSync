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
            app.MapPost("/Location/AddLocationsFromTalabat",
                async (List<TalabatServiceLocationDTO> locations, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    if (locations == null || locations.Count == 0)
                    {
                        return Response(RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None, "No locations provided"));
                    }

                    var result = await mediator.Send(
                        new UpsertLocationsFromTalabatCommand(locations),
                        cancellationToken);

                    return Response(result);
                })
            .WithTags("Location")
            .WithName("AddLocationsFromTalabat")
            .Accepts<List<TalabatServiceLocationDTO>>("application/json")
            .Produces<EndPointResponse<bool>>();
        }
    }
}

