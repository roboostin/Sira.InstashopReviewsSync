using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetLocationByLocationIdAndCompanyIdQuery(
        long LocationID,
        long CompanyID) : IRequest<RequestResult<Location>>;

    public class GetLocationByLocationIdAndCompanyIdQueryHandler : IRequestHandler<GetLocationByLocationIdAndCompanyIdQuery, RequestResult<Location>>
    {
        private readonly IRepository<Location> _repository;

        public GetLocationByLocationIdAndCompanyIdQueryHandler(IRepository<Location> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<Location>> Handle(GetLocationByLocationIdAndCompanyIdQuery request, CancellationToken cancellationToken)
        {
            var location = await _repository.FirstOrDefaultAsync(
                l => l.ID == request.LocationID 
                    && l.CompanyID == request.CompanyID 
                    && !l.IsDeleted,
                cancellationToken);

            // Return success with null data if not found (this is a valid case for upsert operations)
            return RequestResult<Location>.Success(location);
        }
    }
}

