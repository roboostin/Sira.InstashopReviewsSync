using API.Domain.Entities.Client;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpdateReviewReceivedTimeCommand(
        long ReviewID,
        DateTime MessageCreatedDate
    ) : IRequest<RequestResult<bool>>;

    public class UpdateReviewReceivedTimeCommandHandler : IRequestHandler<UpdateReviewReceivedTimeCommand, RequestResult<bool>>
    {
        private readonly IRepository<Review> _repository;

        public UpdateReviewReceivedTimeCommandHandler(
            IRepository<Review> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<bool>> Handle(UpdateReviewReceivedTimeCommand request, CancellationToken cancellationToken)
        {
            _repository.SaveIncluded(new Review()
            {
                ID = request.ReviewID,
                TTL = request.MessageCreatedDate.AddDays(100),
                UpdatedAt = DateTime.UtcNow
            }, nameof(Review.TTL), nameof(Review.UpdatedAt));

            return RequestResult<bool>.Success(true);
        }
    }
}

