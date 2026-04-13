using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class DatabaseHelper
    {
        public static bool VerifyDatabaseCredentials(string server, string database, string username, string password)
        {
            string Log_connectionString = $"Server={server};Database={database};User Id={username};Password={password};";

            try
            {
                using (var Log_connection = new SqlConnection(Log_connectionString))
                {
                    Log_connection.Open();
                    var result = Log_connection.QuerySingleOrDefault<int>("SELECT 1");
                    return result == 1;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}