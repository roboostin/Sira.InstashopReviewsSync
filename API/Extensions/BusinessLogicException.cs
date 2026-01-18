using API.Domain.Enums;

namespace API.Extensions
{
    public class BusinessLogicException : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public BusinessLogicException(string message, ErrorCode errorCode = ErrorCode.None)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
