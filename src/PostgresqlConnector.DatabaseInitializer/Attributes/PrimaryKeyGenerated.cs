namespace PostgresqlConnector.DatabaseInitializer.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyGenerated : Attribute
    {
    }
}