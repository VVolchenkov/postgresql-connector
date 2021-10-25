namespace PostgresqlConnector.DapperGenericRepository.Exceptions
{
    using System;

    public class DapperQueryException : Exception
    {
        public DapperQueryException(string databaseName, string methodName, string originalErrorMessage)
            : base($"Error while trying to query data from db: {databaseName}, method: {methodName}. Original message: {originalErrorMessage}")
        {
        }
    }
}