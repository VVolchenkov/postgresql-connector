namespace PostgresqlConnector.DatabaseInitializer.DatabaseInitialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using PostgresqlConnector.DatabaseInitializer.Attributes;
    using PostgresqlConnector.DatabaseInitializer.Extensions;

    public static class SqlAppendExtensions
    {
        public static void AppendForeignKeys(this StringBuilder sqlTables, List<(CustomAttributeData Attribute, string EntityName)> foreignKeysContainer, string schemaName)
        {
            foreach (var foreignKeyAttribute in foreignKeysContainer)
            {
                var parentTableCustomKey =
                    (string)foreignKeyAttribute.Attribute.ConstructorArguments[2].Value == string.Empty
                        ? ";"
                        : $" (\"{foreignKeyAttribute.Attribute.ConstructorArguments[2].Value.ToString().ToUnderscore()}\");";
                sqlTables.Append(
                    $"ALTER TABLE IF EXISTS {schemaName}.{foreignKeyAttribute.EntityName.ToUnderscore()} " +
                    $"ADD FOREIGN KEY ({foreignKeyAttribute.Attribute.ConstructorArguments[1].Value.ToString().ToUnderscore()}) " +
                    $"REFERENCES {schemaName}.{foreignKeyAttribute.Attribute.ConstructorArguments[0].Value.ToString().ToUnderscore()}");
                sqlTables.Append(parentTableCustomKey);
            }
        }

        public static void AppendIndexes(this StringBuilder sqlTables, IEnumerable<PropertyInfo> typeProperties, string schemaName, MemberInfo entity)
        {
            foreach (var property in typeProperties)
            {
                var indexAttribute = property.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(IndexAttribute));
                if (indexAttribute != null)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var unique = (bool)indexAttribute.ConstructorArguments[0].Value ? "UNIQUE" : string.Empty;
                    sqlTables.Append(
                        $"CREATE {unique} INDEX IF NOT EXISTS {property.Name.ToUnderscore()}_idx ON {schemaName}.{entity.Name.ToUnderscore()} ({property.Name.ToUnderscore()});");
                }
            }
        }

        public static void AppendColumn(this StringBuilder sqlTables, PropertyInfo property)
        {
            sqlTables.Append($"{property.Name.ToUnderscore().WithQuotes()} {property.PropertyType.ToPostgresType()}");
        }

        public static void AppendCreateTable(this StringBuilder sqlTables, string schemaName, string typeName)
        {
            sqlTables.Append($"CREATE TABLE IF NOT EXISTS {schemaName}.{typeName.ToUnderscore()}");
        }

        public static void AppendNotNullIfNotExistsNullable(this StringBuilder sqlTables, MemberInfo property)
        {
            if (property.CustomAttributes.All(attribute => attribute.AttributeType != typeof(NullableAttribute)))
            {
                sqlTables.Append(" NOT NULL");
            }
        }

        public static void AppendAttributeFromPostgresAttribute(this StringBuilder sqlTables, MemberInfo property)
        {
            foreach (var attribute in property.CustomAttributes.ToArray().Order())
            {
                sqlTables.Append(attribute.Attribute.ToPostgresAttribute());
            }
        }

        private static string ToPostgresType(this Type propertyInfo)
        {
            if (propertyInfo.IsGenericType && propertyInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyInfo = Nullable.GetUnderlyingType(propertyInfo);
            }

            return Type.GetTypeCode(propertyInfo) switch
            {
                TypeCode.String => "varchar",
                TypeCode.Boolean => "boolean",
                TypeCode.Int32 => "integer",
                TypeCode.DateTime => "timestamp without time zone",
                TypeCode.Decimal => "numeric",
                TypeCode.Int64 => "bigint",
                TypeCode.Object => GetPostgresType(propertyInfo),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyInfo))
            };
        }

        private static string GetPostgresType(Type propertyInfo)
        {
            if (propertyInfo.IsArray)
            {
                var elementType = propertyInfo.GetElementType();
                return Type.GetTypeCode(elementType) switch
                {
                    TypeCode.String => "text[]",
                    _ => throw new ArgumentOutOfRangeException(nameof(propertyInfo)),
                };
            }

            throw new ArgumentOutOfRangeException(nameof(propertyInfo));
        }

        private static string ToPostgresAttribute(this CustomAttributeData attribute)
        {
            var type = attribute.AttributeType;
            return type switch
            {
                { } when type == typeof(PrimaryKeyAttribute) => " PRIMARY KEY",
                { } when type == typeof(PrimaryKeyGenerated) => " PRIMARY KEY GENERATED ALWAYS AS IDENTITY",
                { } when type == typeof(UniqueAttribute) => " UNIQUE",
                { } when type == typeof(StringLengthAttribute) => $"({attribute.ConstructorArguments[0].Value})",
                { } when type == typeof(DefaultAttribute) => $" DEFAULT {attribute.ConstructorArguments[0].Value}",
                { } when type == typeof(NullableAttribute) => string.Empty,
                { } when type == typeof(IndexAttribute) => string.Empty,
                { } when type == typeof(ForeignKeyAttribute) => string.Empty,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static IEnumerable<(CustomAttributeData Attribute, int priority)> Order(this CustomAttributeData[] customAttributes)
        {
            List<(CustomAttributeData Attribute, int Priority)> orderedCustomAttributes =
                (from attribute in customAttributes
                    let type = attribute.AttributeType
                    let attributePriority = type switch
                    {
                        { } when type == typeof(StringLengthAttribute) => 0,
                        { } when type == typeof(NullableAttribute) => 1,
                        { } when type == typeof(UniqueAttribute) => 2,
                        { } when type == typeof(DefaultAttribute) => 3,
                        _ => int.MaxValue,
                    }
                    select new ValueTuple<CustomAttributeData, int>(attribute, attributePriority)).ToList();

            return orderedCustomAttributes.OrderBy(x => x.Priority);
        }
    }
}