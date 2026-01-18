using API.Shared.Models;

namespace API.EndPoints;

public abstract class EndpointDefinition
{
    public abstract void RegisterEndpoints(IEndpointRouteBuilder app);
    
    protected static IResult Response<T>(RequestResult<T> result)
    {
        return Results.Ok((EndPointResponse<T>)result);
    }
    
    protected static IResult Response<T>(PagingDto<T> result)
    {
        return Results.Ok(EndPointResponse<PagingDto<T>>.Success(result));
    }
}