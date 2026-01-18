using API.Application.Features.Locations.AddLocation;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using MediatR;

namespace API.Infrastructure.MessageBroker.Consumers
{
    public class CreatedLocationConsumer : IBaseConsumer<CreatedLocationMessage>
    {
        IMediator mediator;

        public CreatedLocationConsumer(IMediator _mediator)
        {
            mediator = _mediator;
        }
        public async Task Consume(CreatedLocationMessage message)
        {
            await mediator.Send(new AddLocationOrchestrator(message.LocationID, message.LocationName, message.TalabatLocationsIDs
                , message.CompanyID, message.MessageCreatedDate, message.UserID));
        }
    }
}
