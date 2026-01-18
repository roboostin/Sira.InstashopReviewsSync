using API.Infrastructure.MessageBroker.Core;
using MediatR;

namespace API.Shared.Models;

public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected readonly IMediator _mediator;
    protected readonly UserState _userState;
    protected readonly IEventPublisher _eventPublisher;

    public RequestHandlerBase(RequestHandlerBaseParameters parameters)
    {
        _mediator = parameters.Mediator;
        _userState = parameters.UserState;
        _eventPublisher = parameters.EventPublisher;
    }

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
