using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetActiveInstashopLocationsQuery : IRequest<RequestResult<List<ActiveInstashopLocationDto>>>;

    public class GetActiveInstashopLocationsQueryHandler : IRequestHandler<GetActiveInstashopLocationsQuery, RequestResult<List<ActiveInstashopLocationDto>>>
    {
        private readonly IRepository<Location> _repository;

        public GetActiveInstashopLocationsQueryHandler(
            IRepository<Location> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<List<ActiveInstashopLocationDto>>> Handle(GetActiveInstashopLocationsQuery request, CancellationToken cancellationToken)
        {
            
            var predicate = PredicateBuilder.New<Location>();
            predicate = predicate.And(x => !string.IsNullOrEmpty(x.InstashopClientId));
            predicate = predicate.And(x => x.IsActive);

            var locations = await _repository.Get(predicate)
                .Select(x => new ActiveInstashopLocationDto
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
                return RequestResult<List<ActiveInstashopLocationDto>>.Failure(Domain.Enums.ErrorCode.NoLocationsFound);
            }

            return RequestResult<List<ActiveInstashopLocationDto>>.Success(locations);
            
        }
    }
}
