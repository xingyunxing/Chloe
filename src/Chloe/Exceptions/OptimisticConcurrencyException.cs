namespace Chloe.Exceptions
{
    public class OptimisticConcurrencyException : ChloeException
    {
        public OptimisticConcurrencyException() : this("The number of affected rows is 0 when performing optimistic concurrent operation.")
        {

        }

        public OptimisticConcurrencyException(string message) : base(message)
        {

        }
    }
}
