using API.Domain.Enums;
using API.Helpers;

namespace API.Shared.Models;

public record RequestResult<T>(T Data, bool IsSuccess, string Message, ErrorCode ErrorCode)
{
    public static RequestResult<T> Success(T data, string message)
    {
        return new RequestResult<T>(data, true, message, ErrorCode.None);
    }

    public static RequestResult<T> Success(T data)
    {
        return new RequestResult<T>(data, true, string.Empty, ErrorCode.None);
    }
    public static RequestResult<T> Success()
    {
        return new RequestResult<T>(default, true, string.Empty, ErrorCode.None);
    }

    public static RequestResult<T> Failure(ErrorCode errorCode)
    {
        string message = errorCode.GetDescription();

        return new RequestResult<T>(default, false, message, errorCode);
    }

    public static RequestResult<T> Failure(ErrorCode errorCode, string message)
    {
        return new RequestResult<T>(default, false, message, errorCode);
    }

    public static RequestResult<T> Failure()
    {
        return new RequestResult<T>(default, false, null, ErrorCode.None);
    }
}