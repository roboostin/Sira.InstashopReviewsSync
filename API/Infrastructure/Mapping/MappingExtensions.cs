using Microsoft.Extensions.DependencyInjection;

namespace API.Infrastructure.Mapping;

/// <summary>
/// Extension methods for the mapping service
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Adds the mapping service to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMappingService(this IServiceCollection services)
    {
        services.AddSingleton<IMappingService, MappingService>();
        return services;
    }

    /// <summary>
    /// Maps a source object to a destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <param name="mappingService">The mapping service</param>
    /// <returns>Mapped destination object</returns>
    public static TDestination MapTo<TSource, TDestination>(this TSource source, IMappingService mappingService)
    {
        return mappingService.Map<TSource, TDestination>(source);
    }

    /// <summary>
    /// Maps a collection of source objects to a collection of destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="mappingService">The mapping service</param>
    /// <returns>Mapped destination collection</returns>
    public static IEnumerable<TDestination> MapTo<TSource, TDestination>(this IEnumerable<TSource> source, IMappingService mappingService)
    {
        return mappingService.Map<TSource, TDestination>(source);
    }

    /// <summary>
    /// Maps a queryable source to a queryable destination
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source queryable</param>
    /// <param name="mappingService">The mapping service</param>
    /// <returns>Mapped destination queryable</returns>
    public static IQueryable<TDestination> MapTo<TSource, TDestination>(this IQueryable<TSource> source, IMappingService mappingService)
    {
        return mappingService.Map<TSource, TDestination>(source);
    }
} 