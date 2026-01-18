using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.AddLocation
{
    public record DeleteLocationCommand(long LocationID, DateTime UpdatedDate, long UpdatedBy) : IRequest<RequestResult<Unit>>;

    public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> repository;

        public DeleteLocationCommandHandler(IRepository<Location> repository)
        {
            this.repository = repository;
        }

        public async Task<RequestResult<Unit>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
           
            repository.SaveIncluded(new Location
            {
                ID = request.LocationID,
                UpdatedBy = request.UpdatedBy,
                UpdatedAt = request.UpdatedDate
            }, nameof(Location.UpdatedBy)
            , nameof(Location.UpdatedAt));

            return RequestResult<Unit>.Success();
        }
    }
}

