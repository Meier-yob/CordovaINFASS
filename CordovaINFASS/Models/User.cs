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

        public string ToInsertSqlQuery()
        {
            string[] fields =
            {
                "FirstName",
                "LastName",
                "Email",
                "PasswordHash",
                "CreatedAt"
            };

            object[] values =
            {
                FirstName,
                LastName,
                Email,
                PasswordHash,
                CreatedAt
            };

            return ToInsertSqlQuery("Users", fields, values);
        }

        public static string ToInsertSqlQuery(string tableName, string[] fields, object[] values)
        {
            if (fields == null || values == null || fields.Length != values.Length)
            {
                throw new ArgumentException("Fields and values must match.");
            }

            string columns = "";
            string vals = "";

            for (int i = 0; i < fields.Length; i++)
            {
                columns += fields[i];

                if (values[i] == null)
                {
                    vals += "NULL";
                }
                else if (values[i] is DateTime)
                {
                    vals += "'" + ((DateTime)values[i]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else
                {
                    vals += "'" + values[i].ToString().Replace("'", "''") + "'";
                }

                if (i < fields.Length - 1)
                {
                    columns += ", ";
                    vals += ", ";
                }
            }

            return "INSERT INTO " + tableName + " (" + columns + ") VALUES (" + vals + ");";
        }
    }
}