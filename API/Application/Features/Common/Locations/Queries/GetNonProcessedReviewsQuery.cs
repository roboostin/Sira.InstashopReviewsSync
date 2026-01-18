using API.Domain.Entities.Client;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Queries
{
    public record GetNonProcessedReviewsQuery(int Limit = 101) : IRequest<RequestResult<List<Review>>>;

    public class GetNonProcessedReviewsQueryHandler : IRequestHandler<GetNonProcessedReviewsQuery, RequestResult<List<Review>>>
    {
        private readonly IRepository<Review> _repository;

        public GetNonProcessedReviewsQueryHandler(IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<List<Review>>> Handle(GetNonProcessedReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _repository
                .Get(r => !r.IsProcessed)
                .Take(request.Limit)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return RequestResult<List<Review>>.Success(reviews);
        }
    }


}

