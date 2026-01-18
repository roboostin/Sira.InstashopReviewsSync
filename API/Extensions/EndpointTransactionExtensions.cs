using API.Middlewares;

namespace API.Extensions;

/// <summary>
/// Extension methods for controlling transaction behavior on endpoints
/// </summary>
public static class EndpointTransactionExtensions
{
    /// <summary>
    /// Explicitly requires a database transaction for this endpoint.
    /// Use for GET endpoints that need transactions (rare) or for documentation purposes.
    /// </summary>
    public static RouteHandlerBuilder RequiresTransaction(this RouteHandlerBuilder builder)
    {
        return builder.WithMetadata(new RequiredDBTransactionAttribute());
    }

    /// <summary>
    /// Skips database transaction for this endpoint.
    /// Use for POST/PUT/DELETE endpoints that don't modify the database (logging, analytics, etc.)
    /// </summary>
    public static RouteHandlerBuilder SkipsTransaction(this RouteHandlerBuilder builder)
    {
        return builder.WithMetadata(new SkipDBTransactionAttribute());
    }
}






