namespace PostgresqlConnector.DapperGenericRepository.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Dapper;
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using PostgresqlConnector.DatabaseInitializer.Interfaces;

    public static class RepositoryQueryExtensions
    {
        public static Func<NpgsqlConnection, NpgsqlTransaction, Task<T>> GetAsync<T>(int id)
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.GetAsync<T>(id, transaction);
        }

        public static Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> GetAllAsync<T>()
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.GetAllAsync<T>(transaction);
        }

        public static Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> GetByFilterAsync<T>(
            string sql,
            object param)
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.QueryAsync<T>(sql, param, transaction);
        }

        public static Func<NpgsqlConnection, NpgsqlTransaction, Task<IEnumerable<T>>> GetRawAsync<T>(
            string sql,
            object param)
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.QueryAsync<T>(sql, param, transaction);
        }

        public static Func<NpgsqlConnection, NpgsqlTransaction, Task> UpdateAsync<T>(string sql, T entity)
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.ExecuteAsync(sql, entity, transaction);
        }

        public static Func<NpgsqlConnection, NpgsqlTransaction, Task> InsertAsync<T>(string sql, T entity)
            where T : class, IEntity
        {
            return async (connection, transaction) => await connection.ExecuteAsync(sql, entity, transaction);
        }
    }
}