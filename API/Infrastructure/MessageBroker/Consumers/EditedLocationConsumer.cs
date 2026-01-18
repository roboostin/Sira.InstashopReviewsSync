using API.Application.Features.Locations;
using API.Application.Features.Locations.AddLocation;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using MediatR;

namespace API.Infrastructure.MessageBroker.Consumers
{
    public class EditedLocationConsumer : IBaseConsumer<EditedLocationMessage>
    {
        IMediator mediator;

        public EditedLocationConsumer(IMediator _mediator)
        {
            mediator = _mediator;
        }
        public async Task Consume(EditedLocationMessage message)
        {
            await mediator.Send(new EditLocationCommand(
                message.LocationID,
                message.LocationName,
                message.MessageCreatedDate,
                message.UserID));
        }
    }
}
