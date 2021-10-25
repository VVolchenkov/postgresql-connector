namespace PostgresqlConnector.DatabaseInitializer.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexMulticolumnAttribute : Attribute
    {
        public IndexMulticolumnAttribute(string[] columns, bool unique = false)
        {
        }
    }
}