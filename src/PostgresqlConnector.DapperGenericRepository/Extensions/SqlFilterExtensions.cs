namespace PostgresqlConnector.DapperGenericRepository.Extensions
{
    using System.Text;

    public static class SqlFilterExtensions
    {
        public static StringBuilder Select(string fieldName = "*")
        {
            var str = new StringBuilder();
            return str.Append($"SELECT {fieldName} FROM ");
        }

        public static StringBuilder Where(this StringBuilder str, string condition)
        {
            return str.ToString().Contains("WHERE") ? str.Append($"AND {condition} ") : str.Append($"WHERE {condition} ");
        }

        public static StringBuilder OrderBy(this StringBuilder str, string orderFieldName, string sort = "DESC")
        {
            return str.Append($"ORDER BY {orderFieldName} {sort} ");
        }

        public static StringBuilder GroupBy(this StringBuilder str, string groupByFieldNames)
        {
            return str.Append($"GROUP BY {groupByFieldNames} ");
        }

        public static StringBuilder Limit(this StringBuilder str, int count)
        {
            return str.Append($"LIMIT {count}");
        }

        public static string Invoke(this StringBuilder str, string tableName)
        {
            return str.Replace("FROM", $"FROM {tableName}").ToString();
        }
    }
}