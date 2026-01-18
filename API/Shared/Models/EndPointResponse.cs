using API.Domain.Enums;
using API.Helpers;

namespace API.Shared.Models;

public record EndPointResponse<T>(T Data, bool IsSuccess, string Message, ErrorCode ErrorCode, bool IsAuthorized)
{
    public static EndPointResponse<T> Success(T data)
    {
        return new EndPointResponse<T>(data, true, string.Empty, ErrorCode.None, true);
    }

    public static EndPointResponse<T> Success(T data, string message, bool isAuthorized = true)
    {
        return new EndPointResponse<T>(data, true, message, ErrorCode.None, isAuthorized);
    }

    public static EndPointResponse<T> Failure(ErrorCode errorCode, bool isAuthorized = true)
    {
        string message = errorCode.GetDescription();
        return new EndPointResponse<T>(default, false, message, errorCode, isAuthorized);
    }
    public static EndPointResponse<T> Failure(ErrorCode errorCode, string message, bool isAuthorized = true)
    {
        return new EndPointResponse<T>(default, false, message, errorCode, isAuthorized);
    }

    public static implicit operator EndPointResponse<T>(RequestResult<T> result)
    {
        if (result.IsSuccess)
            return Success(result.Data, result.Message);

        return Failure(result.ErrorCode, result.Message);
    }
}
