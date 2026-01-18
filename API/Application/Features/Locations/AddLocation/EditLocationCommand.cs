using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.AddLocation
{
    public record EditLocationCommand(long LocationID, string LocationName, DateTime UpdatedDate, long UpdatedBy) : IRequest<RequestResult<Unit>>;

    public class EditLocationCommandHandler : IRequestHandler<EditLocationCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> repository;

        public EditLocationCommandHandler(IRepository<Location> repository)
        {
            this.repository = repository;
        }

        public async Task<RequestResult<Unit>> Handle(EditLocationCommand request, CancellationToken cancellationToken)
        {
            repository.SaveIncluded(new Location
            {
                ID = request.LocationID,
                Name = request.LocationName,
                UpdatedAt = request.UpdatedDate,
                UpdatedBy = request.UpdatedBy
            }, nameof(Location.Name));

            return RequestResult<Unit>.Success();
        }
    }

}
