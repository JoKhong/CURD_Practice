namespace Exceptions
{
    public class InvalidPersonIdExceptions : ArgumentException
    {
        public InvalidPersonIdExceptions() : base()
        {
            
        }

        public InvalidPersonIdExceptions(string? message) : base(message)
        {
             
        }

        public InvalidPersonIdExceptions(string? message, Exception? innerException)
        {
            
        }
    }
}
