using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpdateLocationFromTalabatCommand(
        long LocationID,
        TalabatServiceLocationDTO LocationDto) : IRequest<RequestResult<Unit>>;

    public class UpdateLocationFromTalabatCommandHandler : IRequestHandler<UpdateLocationFromTalabatCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> _repository;

        public UpdateLocationFromTalabatCommandHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public Task<RequestResult<Unit>> Handle(UpdateLocationFromTalabatCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            
            _repository.SaveIncluded(new Location
            {
                ID = request.LocationID,
                Name = request.LocationDto.LocationName,
                TalabatLocationIDs = request.LocationDto.TalabatLocationsIDs ?? new List<int>(),
                IsActive = request.LocationDto.IsActive,
                CompanyID = request.LocationDto.CompanyID,
                UpdatedAt = now,
            }, 
            nameof(Location.Name),
            nameof(Location.TalabatLocationIDs),
            nameof(Location.IsActive),
            nameof(Location.CompanyID),
            nameof(Location.UpdatedAt));

            return Task.FromResult(RequestResult<Unit>.Success());
        }
    }
}

