namespace PostgresqlConnector.DatabaseInitializer.DatabaseInitialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Dapper;
    using PostgresqlConnector.DatabaseInitializer.Attributes;
    using PostgresqlConnector.DatabaseInitializer.Interfaces;

    public class DatabaseInitializer
    {
        private readonly DataContextFactory dataContextFactory;

        public DatabaseInitializer(DataContextFactory dataContextFactory)
        {
            this.dataContextFactory = dataContextFactory;
        }

        public void InitializeDatabase()
        {
            Console.WriteLine("Starting to create database");
            var connection = this.dataContextFactory.CreateConnection();

            var type = typeof(IEntity);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && type.IsAssignableFrom(t));

            var schemaName = "public";
            connection.Execute($"CREATE SCHEMA IF NOT EXISTS {schemaName}");
            List<(CustomAttributeData Attribute, string EntityName)> foreignKeysContainer = new List<ValueTuple<CustomAttributeData, string>>();

            var sqlTables = new StringBuilder();
            foreach (var entity in types)
            {
                var typeProperties = entity.GetProperties();

                sqlTables.AppendCreateTable(schemaName, entity.Name);
                sqlTables.Append("(");

                foreach (var property in typeProperties)
                {
                    if (ContinueIfClass(property))
                    {
                        continue;
                    }

                    sqlTables.AppendColumn(property);
                    sqlTables.AppendAttributeFromPostgresAttribute(property);
                    sqlTables.AppendNotNullIfNotExistsNullable(property);
                    sqlTables.Append(",");
                }

                sqlTables.Remove(sqlTables.Length - 1, 1);
                sqlTables.Append(");");
                sqlTables.AppendIndexes(typeProperties, schemaName, entity);

                foreignKeysContainer.AddRange(GetForeignKeysAttributes(typeProperties, entity));
            }

            sqlTables.AppendForeignKeys(foreignKeysContainer, schemaName);

            connection.Execute(sqlTables.ToString());

            Console.WriteLine("Ending database creation");
        }

        private static IEnumerable<(CustomAttributeData, string)> GetForeignKeysAttributes(IEnumerable<PropertyInfo> typeProperties, Type entity)
        {
            return (from property in typeProperties
                select property.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(ForeignKeyAttribute))
                into foreignKeyAttribute
                where foreignKeyAttribute != null
                select new ValueTuple<CustomAttributeData, string>(foreignKeyAttribute, entity.Name)).ToList();
        }

        private static bool ContinueIfClass(PropertyInfo property)
        {
            return property.PropertyType.IsClass && Type.GetTypeCode(property.PropertyType) != TypeCode.String && !property.PropertyType.IsArray;
        }
    }
}