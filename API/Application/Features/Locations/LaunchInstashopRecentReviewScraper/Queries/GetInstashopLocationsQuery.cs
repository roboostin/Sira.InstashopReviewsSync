using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Queries
{
    public record GetInstashopLocationsQuery : IRequest<RequestResult<List<InstashopLocationDto>>>;

    public class GetInstashopLocationsQueryHandler : IRequestHandler<GetInstashopLocationsQuery, RequestResult<List<InstashopLocationDto>>>
    {
        private readonly IRepository<Location> _repository;
        private readonly ILogger<GetInstashopLocationsQueryHandler> _logger;

        public GetInstashopLocationsQueryHandler(
            IRepository<Location> repository,
            ILogger<GetInstashopLocationsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RequestResult<List<InstashopLocationDto>>> Handle(GetInstashopLocationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var predicate = PredicateBuilder.New<Location>();
                predicate = predicate.And(x => !string.IsNullOrEmpty(x.InstashopClientId));
                predicate = predicate.And(x => x.IsActive);

                var locations = await _repository.Get(predicate)
                    .Select(x => new InstashopLocationDto
                    {
                        LocationId = x.ID,
                        LocationName = x.Name,
                        InstashopClientId = x.InstashopClientId,
                        CompanyID = x.CompanyID
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (locations == null || locations.Count == 0)
                {
                    return RequestResult<List<InstashopLocationDto>>.Failure(Domain.Enums.ErrorCode.NoLocationsFound);
                }

                return RequestResult<List<InstashopLocationDto>>.Success(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Instashop locations");
                return RequestResult<List<InstashopLocationDto>>.Failure(Domain.Enums.ErrorCode.None); ;
            }
        }
    }

    public class InstashopLocationDto
    {
        public long LocationId { get; set; }
        public string LocationName { get; set; }
        public string InstashopClientId { get; set; }
        public long CompanyID { get; set; }
    }
}
