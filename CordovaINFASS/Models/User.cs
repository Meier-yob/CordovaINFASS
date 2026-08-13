using System;
using System.ComponentModel.DataAnnotations;

namespace CordovaINFASS.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        // NOTE: For demo only. In production store a salted hash.
        [Required, StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone, StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Build INSERT SQL for current instance (keeps project convention)
        public string ToInsertSqlQuery()
        {
            string[] fields =
            {
                "FirstName",
                "LastName",
                "Email",
                "PasswordHash",
                "Phone",
                "Role",
                "IsActive",
                "CreatedAt",
                "UpdatedAt"
            };

            object[] values =
            {
                FirstName,
                LastName,
                Email,
                PasswordHash,
                Phone,
                Role,
                IsActive ? 1 : 0,
                CreatedAt,
                UpdatedAt
            };

            return ToInsertSqlQuery("Users", fields, values);
        }

        // Build UPDATE SQL for current instance (updates by Id)
        public string ToUpdateSqlQuery()
        {
            var updatedAt = (UpdatedAt ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss");
            string setClause =
                $"FirstName = '{Escape(FirstName)}', " +
                $"LastName = '{Escape(LastName)}', " +
                $"Email = '{Escape(Email)}', " +
                $"PasswordHash = '{Escape(PasswordHash)}', " +
                $"Phone = {(Phone == null ? "NULL" : $"'{Escape(Phone)}'")}, " +
                $"Role = '{Escape(Role)}', " +
                $"IsActive = {(IsActive ? 1 : 0)}, " +
                $"UpdatedAt = '{updatedAt}'";

            return $"UPDATE Users SET {setClause} WHERE Id = {Id};";
        }

        // Build DELETE SQL for this instance
        public string ToDeleteSqlQuery()
        {
            return $"DELETE FROM Users WHERE Id = {Id};";
        }

        private static string Escape(object? value)
        {
            if (value == null) return string.Empty;
            return value.ToString()!.Replace("'", "''");
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
                else if (values[i] is DateTime dt)
                {
                    vals += $"'{dt.ToString("yyyy-MM-dd HH:mm:ss")}'";
                }
                else if (values[i] is int || values[i] is long || values[i] is bool)
                {
                    vals += values[i].ToString()!.ToLower();
                }
                else
                {
                    vals += $"'{Escape(values[i])}'";
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