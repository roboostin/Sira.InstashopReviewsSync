using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Queries
{
    public record GetTalabatLocationsQuery : IRequest<RequestResult<List<TalabatLocationDto>>>;

    public class GetTalabatLocationsQueryHandler : IRequestHandler<GetTalabatLocationsQuery, RequestResult<List<TalabatLocationDto>>>
    {
        private readonly IRepository<Location> _repository;
        private readonly ILogger<GetTalabatLocationsQueryHandler> _logger;

        public GetTalabatLocationsQueryHandler(
            IRepository<Location> repository,
            ILogger<GetTalabatLocationsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RequestResult<List<TalabatLocationDto>>> Handle(GetTalabatLocationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var predicate = PredicateBuilder.New<Location>();
                predicate = predicate.And(x => x.TalabatLocationIDs != null && x.TalabatLocationIDs.Count > 0);
                predicate = predicate.And(x => x.IsActive);

                var locations = await _repository.Get(predicate)
                    .Select(x => new TalabatLocationDto
                    {
                        LocationId = x.ID,
                        LocationName = x.Name,
                        TalabatLocationIDs = x.TalabatLocationIDs,
                        CompanyID = x.CompanyID
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (locations == null || locations.Count == 0)
                {
                    return RequestResult<List<TalabatLocationDto>>.Failure(Domain.Enums.ErrorCode.NoLocationsFound);
                }

                return RequestResult<List<TalabatLocationDto>>.Success(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Talabat locations");
                return RequestResult<List<TalabatLocationDto>>.Failure(Domain.Enums.ErrorCode.None); ;
            }
        }
    }

    public class TalabatLocationDto
    {
        public long LocationId { get; set; }
        public string LocationName { get; set; }
        public List<int> TalabatLocationIDs { get; set; }
        public long CompanyID { get; set; }
    }
}
