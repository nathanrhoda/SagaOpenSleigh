using System;

namespace Bank.Saga
{
    public class RetryException : Exception
    {
        public RetryException(string message)
            : base(message)
        {

        }
    }
}
