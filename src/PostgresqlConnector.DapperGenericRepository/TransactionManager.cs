namespace PostgresqlConnector.DapperGenericRepository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using Npgsql;
    using PostgresqlConnector.DapperGenericRepository.Exceptions;
    using PostgresqlConnector.DapperGenericRepository.Interfaces;
    using PostgresqlConnector.DatabaseInitializer;

    public class TransactionManager : ITransactionManager
    {
        private readonly DataContextFactory dataContextFactory;

        public TransactionManager(DataContextFactory dataContextFactory)
        {
            this.dataContextFactory = dataContextFactory;
        }

        public async Task<T> BeginTransactionFor<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> func)
        {
            try
            {
                await using var connection = this.dataContextFactory.CreateConnection();
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                return await func.Invoke(connection, transaction);
            }
            catch (Exception e)
            {
                throw new TransactionManagerException($"Cannot execute BeginTransactionFor. Original message {e.Message}", e);
            }
        }

        public async Task<IEnumerable<T>> BeginTransactionFor<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> func)
        {
            await using var connection = this.dataContextFactory.CreateConnection();
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var result = await func.Invoke(connection, transaction);
                await transaction.CommitAsync();

                return result;
            }
            catch (Exception e)
            {
                throw new TransactionManagerException($"Cannot execute BeginTransactionFor collection. Original message {e.Message}", e);
            }
        }

        public async Task BeginTransactionWithNoResultFor<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task> func)
        {
            try
            {
                await using var connection = this.dataContextFactory.CreateConnection();
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                await func.Invoke(connection, transaction);

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                throw new TransactionManagerException($"Cannot execute BeginTransactionWithNoResultFor. Original message {e.Message}", e);
            }
        }
    }
}