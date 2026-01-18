using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Queries
{
    public record GetTalabatLocationReviewCountQuery(long locationID):IRequest<RequestResult<int>>;
    public class GetTalabatLocationReviewCountQueryHandler : IRequestHandler<GetTalabatLocationReviewCountQuery, RequestResult<int>>
    {
        private readonly IRepository<Domain.Entities.Client.Review> _repository;
        public GetTalabatLocationReviewCountQueryHandler(IRepository<Domain.Entities.Client.Review> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<int>> Handle(GetTalabatLocationReviewCountQuery request, CancellationToken cancellationToken)
        {
            int result = await _repository.CountAsync(r => r.LocationID == request.locationID && r.Source == SourceType.Talabat, cancellationToken);

            return RequestResult<int>.Success(result);
        }
    }
}
