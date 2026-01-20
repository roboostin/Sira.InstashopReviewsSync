using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record AddLocationFromInstashopCommand(
        InstashopServiceLocationDTO LocationDto) : IRequest<RequestResult<Unit>>;

    public class AddLocationFromInstashopCommandHandler : IRequestHandler<AddLocationFromInstashopCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> _repository;

        public AddLocationFromInstashopCommandHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public Task<RequestResult<Unit>> Handle(AddLocationFromInstashopCommand request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var newLocation = new Location
            {
                ID = request.LocationDto.LocationID,
                Name = request.LocationDto.LocationName,
                InstashopClientId = request.LocationDto.InstashopClientId,
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
