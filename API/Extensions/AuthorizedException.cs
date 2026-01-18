namespace API.Extensions
{
    public class AuthorizedException : BusinessLogicException
    {
        public AuthorizedException(string message)
            : base(message)
        {

        }
    }
}
