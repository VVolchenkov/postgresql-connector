namespace PostgresqlConnector.DatabaseInitializer
{
    using Dapper.Contrib.Extensions;
    using Npgsql;
    using Npgsql.Logging;
    using PostgresqlConnector.DatabaseInitializer.Extensions;

    public class DataContextFactory
    {
        private readonly string connectionString;

        public DataContextFactory(string connectionString, bool logsEnabled)
        {
            this.connectionString = connectionString;
            if (logsEnabled)
            {
                NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Trace, true, true);
            }

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapperExtensions.TableNameMapper = type => type.Name.ToUnderscore();
        }

        public NpgsqlConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(this.connectionString);
            connection.Open();
            return connection;
        }
    }
}
