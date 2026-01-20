using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetCompaniesWithActiveInstashopLocationsQuery : IRequest<RequestResult<List<long>>>;

    public class GetCompaniesWithActiveInstashopLocationsQueryHandler : IRequestHandler<GetCompaniesWithActiveInstashopLocationsQuery, RequestResult<List<long>>>
    {
        private readonly IRepository<Location> _repository;
        private readonly ILogger<GetCompaniesWithActiveInstashopLocationsQueryHandler> _logger;

        public GetCompaniesWithActiveInstashopLocationsQueryHandler(
            IRepository<Location> repository,
            ILogger<GetCompaniesWithActiveInstashopLocationsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RequestResult<List<long>>> Handle(GetCompaniesWithActiveInstashopLocationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var predicate = PredicateBuilder.New<Location>();
                predicate = predicate.And(x => !string.IsNullOrEmpty(x.InstashopClientId));
                predicate = predicate.And(x => x.IsActive);

                var companyIDs = await _repository.Get(predicate)
                    .Select(x => x.CompanyID)
                    .Distinct()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (companyIDs == null || companyIDs.Count == 0)
                {
                    return RequestResult<List<long>>.Failure(Domain.Enums.ErrorCode.NoLocationsFound);
                }

                return RequestResult<List<long>>.Success(companyIDs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching companies with active Instashop locations");
                return RequestResult<List<long>>.Failure(Domain.Enums.ErrorCode.None);
            }
        }
    }
}
