using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpdateLocationFromInstashopCommand(
        long LocationID,
        InstashopServiceLocationDTO LocationDto) : IRequest<RequestResult<Unit>>;

    public class UpdateLocationFromInstashopCommandHandler : IRequestHandler<UpdateLocationFromInstashopCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> _repository;

        public UpdateLocationFromInstashopCommandHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public Task<RequestResult<Unit>> Handle(UpdateLocationFromInstashopCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            
            _repository.SaveIncluded(new Location
            {
                ID = request.LocationID,
                Name = request.LocationDto.LocationName,
                InstashopClientId = request.LocationDto.InstashopClientId,
                IsActive = request.LocationDto.IsActive,
                CompanyID = request.LocationDto.CompanyID,
                UpdatedAt = now,
            }, 
            nameof(Location.Name),
            nameof(Location.InstashopClientId),
            nameof(Location.IsActive),
            nameof(Location.CompanyID),
            nameof(Location.UpdatedAt));

            return Task.FromResult(RequestResult<Unit>.Success());
        }
    }
}
