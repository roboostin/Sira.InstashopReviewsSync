using API.Domain.Entities.Client;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Commands
{
    public record MarkReviewsAsProcessedCommand(List<long> ReviewIds) : IRequest<RequestResult<bool>>;

    public class MarkReviewsAsProcessedCommandHandler : IRequestHandler<MarkReviewsAsProcessedCommand, RequestResult<bool>>
    {
        private readonly IRepository<Review> _repository;

        public MarkReviewsAsProcessedCommandHandler(
            IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<bool>> Handle(MarkReviewsAsProcessedCommand request, CancellationToken cancellationToken)
        {
            foreach (var reviewID in request.ReviewIds)
            {
                _repository.SaveIncluded(new Review()
                {
                    ID = reviewID,
                    IsProcessed = true,
                    UpdatedAt = DateTime.UtcNow
                }, nameof(Review.IsProcessed), nameof(Review.UpdatedAt));
            }

            return RequestResult<bool>.Success(true);
        }
    }
}

