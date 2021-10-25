namespace PostgresqlConnector.DapperGenericRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using PostgresqlConnector.DapperGenericRepository.Exceptions;
    using PostgresqlConnector.DapperGenericRepository.Extensions;
    using PostgresqlConnector.DapperGenericRepository.Interfaces;
    using PostgresqlConnector.DatabaseInitializer.Attributes;
    using PostgresqlConnector.DatabaseInitializer.Extensions;
    using PostgresqlConnector.DatabaseInitializer.Interfaces;

    public class GenericRepository<T> : IRepository<T>
        where T : class, IEntity
    {
        private readonly ITransactionManager transactionManager;
        private readonly string tableName;

        public GenericRepository(ITransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
            this.tableName = typeof(T).Name.ToUnderscore();
        }

        public virtual async Task<T> Get(int id)
        {
            try
            {
                var parameters = new { Id = id };
                var sql = $"SELECT * FROM {this.tableName} WHERE id=@Id";
                var result =
                    await this.transactionManager.BeginTransactionFor(
                        RepositoryQueryExtensions.GetByFilterAsync<T>(sql, parameters));

                return result.SingleOrDefault();
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "get", e.Message);
            }
        }

        public virtual async Task<ICollection<T>> GetAll()
        {
            try
            {
                var result =
                    await this.transactionManager.BeginTransactionFor(RepositoryQueryExtensions.GetAllAsync<T>());

                return result.ToList();
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "getAll", e.Message);
            }
        }

        public virtual async Task<ICollection<T>> GetByFilter(StringBuilder filter, object parameters = null)
        {
            try
            {
                var result =
                    await this.transactionManager.BeginTransactionFor(
                        RepositoryQueryExtensions.GetByFilterAsync<T>(filter.Invoke(this.tableName), parameters));

                return result?.ToList();
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "getByFilter", e.Message);
            }
        }

        public virtual async Task<ICollection<T>> GetRaw(string filter, object parameters = null)
        {
            try
            {
                var result =
                    await this.transactionManager.BeginTransactionFor(
                        RepositoryQueryExtensions.GetRawAsync<T>(filter, parameters));

                return result?.ToList();
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "getRaw", e.Message);
            }
        }

        public virtual async Task Update(T entity)
        {
            try
            {
                var propertiesToUpdate = GetProperties(entity);
                var updateSql = $@"UPDATE {this.tableName} SET ";
                updateSql = propertiesToUpdate.Aggregate(
                    updateSql,
                    (current, field) => current + $"{field.Name.ToUnderscore().WithQuotes()}=@{field.Name},");
                updateSql = updateSql.Remove(updateSql.Length - 1);
                updateSql += " WHERE id=@id AND network_id=@NetworkId";

                await this.transactionManager.BeginTransactionWithNoResultFor<T>(
                    RepositoryQueryExtensions.UpdateAsync(updateSql, entity));
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "update", e.Message);
            }
        }

        public virtual async Task Insert(T entity)
        {
            try
            {
                var propertiesToInsert = GetProperties(entity).ToArray();
                var insertSql = $@"INSERT INTO {this.tableName} (";
                insertSql = propertiesToInsert.Aggregate(insertSql, (current, field) => current + field.Name.ToUnderscore().WithQuotes() + ",");
                insertSql = insertSql.Remove(insertSql.Length - 1);
                insertSql += ") VALUES (";
                insertSql = propertiesToInsert.Aggregate(insertSql, (current, field) => current + $"@{field.Name},");
                insertSql = insertSql.Remove(insertSql.Length - 1);
                insertSql += ") ON CONFLICT DO NOTHING"; // ON CONFLICT clause: https://www.postgresql.org/docs/9.5/sql-insert.html

                await this.transactionManager.BeginTransactionWithNoResultFor<T>(
                    RepositoryQueryExtensions.InsertAsync(insertSql, entity));
            }
            catch (Exception e)
            {
                throw new DapperQueryException(this.tableName, "insert", e.Message);
            }
        }

        private static IEnumerable<PropertyInfo> GetProperties(T entity)
        {
            var properties = entity.GetType().GetProperties();
            var propertiesToUpdate = properties.Where(x =>
                (!x.PropertyType.IsClass || Type.GetTypeCode(x.PropertyType) == TypeCode.String ||
                 x.PropertyType.IsArray)
                && !x.CustomAttributes.ToArray().Any(y =>
                    y.AttributeType == typeof(PrimaryKeyAttribute) || y.AttributeType == typeof(PrimaryKeyGenerated)));
            return propertiesToUpdate;
        }
    }
}