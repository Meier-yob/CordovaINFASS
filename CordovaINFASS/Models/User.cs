using System;
using System.ComponentModel.DataAnnotations;

namespace CordovaINFASS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instance method to generate SQL for the current User.
        /// </summary>
        public string ToInsertSqlQuery()
        {
            string[] fields = { "FirstName", "LastName", "Email", "PasswordHash", "CreatedAt" };
            object[] values = { FirstName, LastName, Email, PasswordHash, CreatedAt };

            return ToInsertSqlQuery("Users", fields, values);
        }

        /// <summary>
        /// Reusable static helper for generating INSERT queries without string.Join or Append.
        /// </summary>
        public static string ToInsertSqlQuery(string tableName, string[] fields, object[] values)
        {
            if (fields == null || values == null || fields.Length != values.Length)
            {
                throw new ArgumentException("Fields and values must not be null and must match in length.");
            }

            string Safe(object value)
            {
                if (value == null) return "NULL";

                if (value is DateTime date)
                    return $"'{date:yyyy-MM-dd HH:mm:ss}'";

                return $"'{value.ToString()?.Replace("'", "''")}'";
            }

            string columnsStr = "";
            string valuesStr = "";

            for (int i = 0; i < fields.Length; i++)
            {
                columnsStr += fields[i];
                valuesStr += Safe(values[i]);

                // Add comma separator for all elements except the last one
                if (i < fields.Length - 1)
                {
                    columnsStr += ", ";
                    valuesStr += ", ";
                }
            }

            return $"INSERT INTO {tableName} ({columnsStr}) VALUES ({valuesStr});";
        }
    }
}