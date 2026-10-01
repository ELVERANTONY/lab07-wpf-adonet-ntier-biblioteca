using System.Data;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos.Repositorios;

internal static class DbHelper
{
    public static object Db(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
    public static object Db<T>(T? value) where T : struct => value.HasValue ? (object)value.Value : DBNull.Value;
    
    public static string? GetStringOrNull(this SqlDataReader reader, string columnName)
    {
        int ord = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ord) ? null : reader.GetString(ord);
    }
    
    public static DateTime? GetDateOrNull(this SqlDataReader reader, string columnName)
    {
        int ord = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);
    }
}
