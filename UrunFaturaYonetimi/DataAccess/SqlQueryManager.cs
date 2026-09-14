using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UrunFaturaYonetimi.DataAccess
{
    public static class SqlQueryManager
    {
        // Modelin adından tablo adını çıkarır (Örn: Product -> Products)
        private static string GetTableName<T>()
        {
            return typeof(T).Name + "s";
        }

        // SELECT Sorgusu Üretici
        public static string GenerateSelectQuery<T>()
        {
            return $"SELECT * FROM {GetTableName<T>()} ORDER BY Id DESC";
        }

        // INSERT Sorgusu Üretici (Reflection ile property'leri dinamik çeker)
        public static string GenerateInsertQuery<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(p => p.Name != "Id") // Id otomatik artandır
                                      .ToList();

            string columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
            string parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            return $"INSERT INTO {GetTableName<T>()} ({columns}) VALUES ({parameters})";
        }

        // UPDATE Sorgusu Üretici
        public static string GenerateUpdateQuery<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(p => p.Name != "Id")
                                      .ToList();

            string setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));

            return $"UPDATE {GetTableName<T>()} SET {setClause} WHERE Id = @Id";
        }

        // DELETE Sorgusu Üretici
        public static string GenerateDeleteQuery<T>()
        {
            return $"DELETE FROM {GetTableName<T>()} WHERE Id = @Id";
        }
    }
}