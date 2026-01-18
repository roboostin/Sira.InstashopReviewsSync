using FluentValidation;

namespace API.Middlewares;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator == null) return await next(context);

        var argument = context.Arguments.FirstOrDefault(arg => arg is T) as T;
        if (argument == null) return Results.BadRequest("Invalid request model.");

        var validationResult = await validator.ValidateAsync(argument);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }

        return await next(context);
    }
}