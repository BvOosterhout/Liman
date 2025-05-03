namespace Liman
{
    public class InvalidServiceLifetimeException : Exception
    {
        public InvalidServiceLifetimeException(string message)
            : base(message)
        {
        }
    }
}
