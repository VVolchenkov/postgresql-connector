namespace PostgresqlConnector.DatabaseInitializer.Extensions
{
    using System.Linq;

    public static class CommonDatabaseExtensions
    {
        public static string ToUnderscore(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
        }

        public static string WithQuotes(this string str)
        {
            return $"\"{str}\"";
        }
    }
}