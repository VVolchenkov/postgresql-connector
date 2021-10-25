namespace PostgresqlConnector.DatabaseInitializer.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultAttribute : Attribute
    {
        public DefaultAttribute(string defaultExpression)
        {
        }
    }
}