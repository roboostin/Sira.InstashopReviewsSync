using System.Linq.Expressions;

namespace API.Infrastructure.Mapping;

/// <summary>
/// Generic mapping service for converting between different types
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// Maps a source object to a destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object</param>
    /// <returns>Mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a collection of source objects to a collection of destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source collection</param>
    /// <returns>Mapped destination collection</returns>
    IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source);

    /// <summary>
    /// Maps a queryable source to a queryable destination
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source queryable</param>
    /// <returns>Mapped destination queryable</returns>
    IQueryable<TDestination> Map<TSource, TDestination>(IQueryable<TSource> source);

    /// <summary>
    /// Creates a mapping configuration for a specific source and destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mappingExpression">Expression that defines how to map from source to destination</param>
    void Configure<TSource, TDestination>(Expression<Func<TSource, TDestination>> mappingExpression);
} 