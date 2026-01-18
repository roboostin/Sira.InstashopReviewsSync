namespace API.Shared.Models
{
    public class Error
    {
        public string Code { get; }
        public string Message { get; }
        public string Description { get; }
        public string? Details { get; }

        public Error(string code, string message, string? details = null)
        {
            Code = code;
            Message = message;
            Description = message; // Using message as description by default
            Details = details;
        }

        public Error(string code, string message, string description, string? details = null)
        {
            Code = code;
            Message = message;
            Description = description;
            Details = details;
        }

        public static Error Create(string code, string message, string? details = null)
        {
            return new Error(code, message, details);
        }

        public static Error Create(string code, string message, string description, string? details = null)
        {
            return new Error(code, message, description, details);

        }
    }
}