using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Queries
{
    public record GetInstashopLocationReviewCountQuery(long locationID):IRequest<RequestResult<int>>;
    public class GetInstashopLocationReviewCountQueryHandler : IRequestHandler<GetInstashopLocationReviewCountQuery, RequestResult<int>>
    {
        private readonly IRepository<Domain.Entities.Client.Review> _repository;
        public GetInstashopLocationReviewCountQueryHandler(IRepository<Domain.Entities.Client.Review> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<int>> Handle(GetInstashopLocationReviewCountQuery request, CancellationToken cancellationToken)
        {
            int result = await _repository.CountAsync(r => r.LocationID == request.locationID && r.Source == SourceType.Instashop, cancellationToken);

            return RequestResult<int>.Success(result);
        }
    }
}
