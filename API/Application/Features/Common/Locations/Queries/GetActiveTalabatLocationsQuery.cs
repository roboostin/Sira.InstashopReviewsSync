using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetActiveTalabatLocationsQuery : IRequest<RequestResult<List<ActiveTalabatLocationDto>>>;

    public class GetActiveTalabatLocationsQueryHandler : IRequestHandler<GetActiveTalabatLocationsQuery, RequestResult<List<ActiveTalabatLocationDto>>>
    {
        private readonly IRepository<Location> _repository;

        public GetActiveTalabatLocationsQueryHandler(
            IRepository<Location> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<List<ActiveTalabatLocationDto>>> Handle(GetActiveTalabatLocationsQuery request, CancellationToken cancellationToken)
        {
            
            var predicate = PredicateBuilder.New<Location>();
            predicate = predicate.And(x => x.TalabatLocationIDs != null && x.TalabatLocationIDs.Count > 0);
            predicate = predicate.And(x => x.IsActive);

            var locations = await _repository.Get(predicate)
                .Select(x => new ActiveTalabatLocationDto
                {
                    LocationID = x.ID,
                    LocationName = x.Name,
                    CompanyID = x.CompanyID,
                    LastSuccessfulScrapeTime = x.LastSuccessfulScrapeTime
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (locations == null || locations.Count == 0)
            {
                return RequestResult<List<ActiveTalabatLocationDto>>.Failure(Domain.Enums.ErrorCode.NoLocationsFound);
            }

            return RequestResult<List<ActiveTalabatLocationDto>>.Success(locations);
            
        }
    }
}

