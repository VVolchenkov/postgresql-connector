namespace PostgresqlConnector.DapperGenericRepository.Exceptions
{
    using System;

    public class TransactionManagerException : Exception
    {
        public TransactionManagerException(string message, Exception e)
            : base(message, e)
        {
        }
    }
}