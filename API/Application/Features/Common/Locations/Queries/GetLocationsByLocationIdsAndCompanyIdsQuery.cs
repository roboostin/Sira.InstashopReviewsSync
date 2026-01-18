using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries
{
    public record LocationKey(long LocationID, long CompanyID);

    public record GetLocationsByLocationIdsAndCompanyIdsQuery(
        List<LocationKey> LocationKeys) : IRequest<RequestResult<List<LocationExistenceDto>>>;

    public class GetLocationsByLocationIdsAndCompanyIdsQueryHandler : IRequestHandler<GetLocationsByLocationIdsAndCompanyIdsQuery, RequestResult<List<LocationExistenceDto>>>
    {
        private readonly IRepository<Location> _repository;

        public GetLocationsByLocationIdsAndCompanyIdsQueryHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<List<LocationExistenceDto>>> Handle(GetLocationsByLocationIdsAndCompanyIdsQuery request, CancellationToken cancellationToken)
        {
            if (request.LocationKeys == null || request.LocationKeys.Count == 0)
            {
                return RequestResult<List<LocationExistenceDto>>.Success(new List<LocationExistenceDto>());
            }

            // Build predicate to match any of the location ID + company ID combinations
            var predicate = PredicateBuilder.New<Location>(false);
            
            foreach (var key in request.LocationKeys)
            {
                var keyPredicate = PredicateBuilder.New<Location>();
                keyPredicate = keyPredicate.And(l => l.ID == key.LocationID && l.CompanyID == key.CompanyID && !l.IsDeleted);
                predicate = predicate.Or(keyPredicate);
            }

            var locations = await _repository.Get(predicate)
                .Select(l => new LocationExistenceDto
                {
                    LocationID = l.ID,
                    CompanyID = l.CompanyID
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return RequestResult<List<LocationExistenceDto>>.Success(locations ?? new List<LocationExistenceDto>());
        }
    }
}

