namespace API.Extensions
{
    public class AuthenticatedException : BusinessLogicException
    {
        public AuthenticatedException(string message)
            : base(message)
        {

        }
    }
}
