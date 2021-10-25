namespace PostgresqlConnector.DatabaseInitializer.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(bool unique = false)
        {
        }
    }
}