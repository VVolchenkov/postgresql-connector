namespace PostgresqlConnector.DapperGenericRepository.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Npgsql;

    public interface ITransactionManager
    {
        Task<IEnumerable<T>> BeginTransactionFor<T>(
            Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> func);

        Task<T> BeginTransactionFor<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> func);

        Task BeginTransactionWithNoResultFor<T>(Func<NpgsqlConnection, NpgsqlTransaction, Task> func);
    }
}