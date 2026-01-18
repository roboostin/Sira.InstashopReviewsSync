using API.Domain.Enums;
using API.Infrastructure.MessageBroker.Core;
using API.Shared.Models;
using DotNetCore.CAP.Filter;

namespace API.Infrastructure.MessageBroker.Interceptors;

public class UserStateInitializerEventBusInterceptor(UserState userState) : IEventBusInterceptor
{
    public double ExecutionOrder => 3;

    public Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        InitializeData(context);
        
        return Task.CompletedTask;
    }
    
    public Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        return Task.CompletedTask;
    }
    
    private void InitializeData(ExecutingContext context)
    {
        var username = context.DeliverMessage.Headers.TryGetValue("Username", out var usernameValue)
            ? usernameValue
            : null;
        
        var userUID = context.DeliverMessage.Headers.TryGetValue("UserUID", out var userUIDValue) &&
                      long.TryParse(userUIDValue, out var parsedUserUID)
            ? parsedUserUID
            : (long?)null;
        
        var roleID = context.DeliverMessage.Headers.TryGetValue("RoleID", out var roleIDValue) &&
                     Enum.TryParse<ApplicationRole>(roleIDValue, out var parsedRoleID)
            ? parsedRoleID
            : default;
        
        var companyUID = context.DeliverMessage.Headers.TryGetValue("CompanyID", out var companyUIDValue) &&
                         long.TryParse(companyUIDValue, out var parsedCompanyUID)
            ? parsedCompanyUID
            : (long?)null;
        
        userState.SetUserData(userUID, roleID, companyUID, username);
    }
}