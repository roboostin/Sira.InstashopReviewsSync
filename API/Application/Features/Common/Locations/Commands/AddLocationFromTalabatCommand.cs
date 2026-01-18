using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record AddLocationFromTalabatCommand(
        TalabatServiceLocationDTO LocationDto) : IRequest<RequestResult<Unit>>;

    public class AddLocationFromTalabatCommandHandler : IRequestHandler<AddLocationFromTalabatCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> _repository;

        public AddLocationFromTalabatCommandHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public Task<RequestResult<Unit>> Handle(AddLocationFromTalabatCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var newLocation = new Location
            {
                ID = request.LocationDto.LocationID,
                Name = request.LocationDto.LocationName,
                TalabatLocationIDs = request.LocationDto.TalabatLocationsIDs ?? new List<int>(),
                IsActive = request.LocationDto.IsActive,
                CompanyID = request.LocationDto.CompanyID,
                CreatedAt = now,
                UpdatedAt = now,
            };

            _repository.Add(newLocation);

            return Task.FromResult(RequestResult<Unit>.Success());
        }
    }
}

