using API.Application.Features.Common.Locations.Commands;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using MediatR;

namespace API.Infrastructure.MessageBroker.Consumers
{
    public class ReviewReceivedConsumer : IBaseConsumer<ReviewReceivedMessage>
    {
        private readonly IMediator _mediator;

        public ReviewReceivedConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ReviewReceivedMessage message)
        {
            await _mediator.Send(new UpdateReviewReceivedTimeCommand(
                message.ID,
                message.MessageCreatedDate
            ));
        }
    }
}

