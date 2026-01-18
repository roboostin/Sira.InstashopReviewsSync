using System.Linq.Expressions;
using System.Reflection;

namespace API.Infrastructure.Mapping;

/// <summary>
/// Implementation of the mapping service that uses expression trees for efficient mapping
/// </summary>
public class MappingService : IMappingService
{
    private readonly Dictionary<(Type SourceType, Type DestinationType), Delegate> _mappings = new();

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null)
            return default;

        var mapping = GetOrCreateMapping<TSource, TDestination>();
        return ((Func<TSource, TDestination>)mapping)(source);
    }

    /// <inheritdoc />
    public IEnumerable<TDestination> Map<TSource, TDestination>(IEnumerable<TSource> source)
    {
        if (source == null)
            return Enumerable.Empty<TDestination>();

        return source.Select(Map<TSource, TDestination>);
    }

    /// <inheritdoc />
    public IQueryable<TDestination> Map<TSource, TDestination>(IQueryable<TSource> source)
    {
        if (source == null)
            return Enumerable.Empty<TDestination>().AsQueryable();

        var mapping = GetOrCreateMapping<TSource, TDestination>();
        var parameter = Expression.Parameter(typeof(TSource), "source");
        var body = Expression.Invoke(Expression.Constant(mapping), parameter);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, parameter);

        return source.Select(lambda);
    }

    /// <inheritdoc />
    public void Configure<TSource, TDestination>(Expression<Func<TSource, TDestination>> mappingExpression)
    {
        var key = (typeof(TSource), typeof(TDestination));
        _mappings[key] = mappingExpression.Compile();
    }

    private Delegate GetOrCreateMapping<TSource, TDestination>()
    {
        var key = (typeof(TSource), typeof(TDestination));
        
        if (_mappings.TryGetValue(key, out var existingMapping))
            return existingMapping;

        // Create a default mapping based on property names
        var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");
        var destinationType = typeof(TDestination);
        
        var bindings = new List<MemberBinding>();
        
        foreach (var destProp in destinationProperties)
        {
            if (!destProp.CanWrite)
                continue;
                
            var sourceProp = sourceProperties.FirstOrDefault(p => 
                p.Name.Equals(destProp.Name, StringComparison.OrdinalIgnoreCase) && 
                p.PropertyType == destProp.PropertyType);
                
            if (sourceProp != null)
            {
                var sourceProperty = Expression.Property(sourceParameter, sourceProp);
                var binding = Expression.Bind(destProp, sourceProperty);
                bindings.Add(binding);
            }
        }
        
        var newExpression = Expression.New(destinationType);
        var memberInit = Expression.MemberInit(newExpression, bindings);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(memberInit, sourceParameter);
        
        var mapping = lambda.Compile();
        _mappings[key] = mapping;
        
        return mapping;
    }
} 