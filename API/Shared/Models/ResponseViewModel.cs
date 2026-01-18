using API.Domain.Enums;

namespace API.Shared.Models
{
    public class ResponseViewModel<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public bool IsAuthorized { get; set; } = true;
        public ErrorCode ErrorCode { get; set; }
        public ResponseViewModel()
        {

        }
        public ResponseViewModel(T data, string message = "", bool success = true, bool isAuthorized = true, ErrorCode errorCode = ErrorCode.None)
        {
            Data = data;
            Success = success;
            Message = message;
            IsAuthorized = isAuthorized;
            ErrorCode = errorCode;
        }

    }
}