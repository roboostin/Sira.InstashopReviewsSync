using API.Application.Features.Locations.AddLocation;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using MediatR;

namespace API.Infrastructure.MessageBroker.Consumers
{
    public class DeleteLocationConsumer : IBaseConsumer<DeleteLocationMessage>
    {
        IMediator mediator;

        public DeleteLocationConsumer(IMediator _mediator)
        {
            mediator = _mediator;
        }

        public async Task Consume(DeleteLocationMessage message)
        {
            await mediator.Send(new DeleteLocationCommand(
                message.LocationID,
                message.MessageCreatedDate,
                message.UserID));
        }
    }
}

