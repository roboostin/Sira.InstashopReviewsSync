using API.Infrastructure.MessageBroker.Core;
using MediatR;

namespace API.Shared.Models;

public class RequestHandlerBaseParameters
{
    public IMediator Mediator => _mediator;
    public UserState UserState => _userState;
    public IEventPublisher EventPublisher { get; init; }

    private UserState _userState;
    private readonly IMediator _mediator;

    public RequestHandlerBaseParameters(IMediator mediator, UserState userState, IEventPublisher eventPublisher)
    {
        _mediator = mediator;
        _userState = userState;
        EventPublisher = eventPublisher;
    }
}
