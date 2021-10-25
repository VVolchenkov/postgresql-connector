namespace PostgresqlConnector.DatabaseInitializer.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}