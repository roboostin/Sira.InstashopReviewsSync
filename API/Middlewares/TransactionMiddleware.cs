using System.Data;
using API.Infrastructure.Persistence.DbContexts;
using DotNetCore.CAP;

namespace API.Middlewares;

public class TransactionMiddleware(
    ApplicationDbContext dbContext, 
    ICapPublisher capPublisher,
    ILogger<TransactionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsTransactionRequired(context))
        {
            try
            {
                await next(context);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        else
        {

            await next(context);
        }

      
    }

    private static bool IsTransactionRequired(HttpContext context)
    {
        if (context == null)
            return false;

        var endpoint = context.GetEndpoint();
        
        // Check for explicit opt-in
        if (endpoint?.Metadata?.GetMetadata<RequiredDBTransactionAttribute>() != null)
            return true;
        
        // Check for explicit opt-out
        if (endpoint?.Metadata?.GetMetadata<SkipDBTransactionAttribute>() != null)
            return false;

        // Default: Skip transactions for GET requests
        var method = context.Request.Method;
        return !string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase);
    }
}

// Attributes for explicit control
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiredDBTransactionAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SkipDBTransactionAttribute : Attribute { }