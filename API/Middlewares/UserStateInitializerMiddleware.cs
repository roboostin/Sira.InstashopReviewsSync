using API.Shared.Helpers;
using API.Shared.Models;
using HttpRequestHelper = API.Helpers.HttpRequestHelper;

namespace API.Middlewares;

public class UserStateInitializerMiddleware(UserState userState, ILogger<UserStateInitializerMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            var accessToken = HttpRequestHelper.GetHeaderValue("token");
            var payload = SecurityHelper.GetPayload(accessToken);
            if (payload != null)
            {
                userState.SetUserData(payload.UserID, payload.RoleID, payload.CompanyID, payload.UserName);
            }

            await next(context);
        }
        catch (Exception ex)
        {   
            logger.LogError(ex, "UserStateInitializerMiddleware: Error initializing user state");
        }
    }
}