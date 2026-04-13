using Dapper;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using KompassHR.Areas.ESS.Models.ESS_FNF;

namespace KompassHR.Models

{
    public class DapperORM
    {
        //public static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;
        //Commnmet Start
        public static string connectionStrings = System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;

        public static string connectionString;
        public static void SetConnection(string Con_Server, string Con_UserId, string Con_Password, string Con_Database)
        {
            string connectionString1 = @"Data Source=" + Con_Server + ";uid=" + Con_UserId + ";pwd=" + Con_Password + ";Initial Catalog=" + Con_Database + ";Integrated Security=false; Connection Timeout=0";
            HttpContext.Current.Session["MyNewConnectionString"] = connectionString1;
        }

        public static void SetConnectionHelpDesk(string Con_Server, string Con_UserId, string Con_Password, string Con_Database)
        {
            string connectionString1 = @"Data Source=" + Con_Server + ";uid=" + Con_UserId + ";pwd=" + Con_Password + ";Initial Catalog=" + Con_Database + ";Integrated Security=false; Connection Timeout=0";
            HttpContext.Current.Session["HelpDeskConnectionString"] = connectionString1;
        }

        public static object ExecuteReturn(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Execute(procedureName, param, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public static object ExecuteReturn1(string procedureName, DynamicParameters param = null, int commandTimeout = 0)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Execute(procedureName, param, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
            }
        }

        public static T ExecuteReturnScler<T>(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return (T)Convert.ChangeType(sqlcon.ExecuteScalar(procedureName, param, commandTimeout: 0, commandType: CommandType.StoredProcedure), typeof(T));
            }

        }

        public static IEnumerable<T> ReturnList<T>(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Query<T>(procedureName, param, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public static dynamic DynamicList(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Query(procedureName, param, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public static dynamic DynamicMultipleResult(string procedureName, DynamicParameters param = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            SqlConnection sqlcon = new SqlConnection(connectionString);
            sqlcon.Open();

            // NOTE: Do not use `using` here as the caller will need to use the reader before disposing the connection
            return sqlcon.QueryMultiple(procedureName, param, commandType: CommandType.StoredProcedure);
        }

        public static SqlMapper.GridReader DynamicMultipleResultList(string procedureName, DynamicParameters param = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            SqlConnection sqlcon = new SqlConnection(connectionString);
            sqlcon.Open();

            // NOTE: Do not use `using` here as the caller will need to use the reader before disposing the connection
            return sqlcon.QueryMultiple(procedureName, param, commandType: CommandType.StoredProcedure);
        }

        public static List<List<dynamic>> DynamicQueryMultiple(string query)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                using (var multi = sqlcon.QueryMultiple(query))
                {
                    var resultSets = new List<List<dynamic>>();

                    while (!multi.IsConsumed)
                    {
                        var result = multi.Read().ToList(); // read as dynamic
                        resultSets.Add(result);
                    }
                    return resultSets;
                }
            }
        }



        public static dynamic DynamicQuerySingle(string Query)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.QuerySingleOrDefault(Query);
                return data;
            }
        }

        public static dynamic QuerySingle(string query, object param = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"]?.ToString();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.QueryFirstOrDefault(query, param);
            }
        }

        public static List<dynamic> DynamicQueryList(string query)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.Query(query).ToList();
                return data;
            }
        }

        public static List<T> ListOfDynamicQueryList<T>(string query)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.Query<T>(query).ToList();
                return data;
            }
        }

        public static List<T> ListOfDynamicQueryListWithParam<T>(string query, object parameters = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Query<T>(query, parameters).ToList();
            }
        }


        public static List<dynamic> DynamicQueryListWithParam(string query, object param = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.Query(query, param).ToList();
                return data;
            }
        }


        public static IEnumerable<TEntity> ExecuteSP<TEntity>(string spName, object parameters = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Query<TEntity>(spName, parameters, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public static dynamic ExecuteQuery(string Query)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.Query(Query);
                return data;
            }
        }

        public static int Execute(string query, object param = null)
        {
            connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                return sqlcon.Execute(query, param);
            }
        }

        public static int Executes(string query, object param = null)
        {
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"]?.ToString();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Execute(query, param);
            }
        }
        //Comment END
        internal DataTable ConvertToDataTableTest(List<FNF_FeedbackMaster_Answer> questionAnswers)
        {
            throw new NotImplementedException();
        }

        #region ConvertToDataTable
        public DataTable ConvertToDataTable(IEnumerable<dynamic> data)
        {
            var dataTable = new DataTable();
            var firstRecord = data.FirstOrDefault() as IDictionary<string, object>;

            if (firstRecord != null)
            {
                // Create columns
                foreach (var kvp in firstRecord)
                {
                    dataTable.Columns.Add(kvp.Key, kvp.Value == null ? typeof(object) : kvp.Value.GetType());
                }
                // Populate rows
                foreach (var record in data)
                {
                    var dataRow = dataTable.NewRow();
                    foreach (var kvp in record)
                    {
                        dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }
        #endregion

        #region HelpDesk
        public static int ExecutesHelpDeskQuery(string query, object param = null)
        {
            string connectionString = HttpContext.Current.Session["HelpDeskConnectionString"]?.ToString();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                return con.Execute(query, param);
            }
        }

        public static object ExecuteReturnHelpDesk(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["HelpDeskConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Execute(procedureName, param, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public static dynamic DynamicQuerySingleHelpDesk(string Query)
        {
            connectionString = HttpContext.Current.Session["HelpDeskConnectionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                var data = sqlcon.QuerySingleOrDefault(Query);
                return data;
            }
        }

        #endregion
    }
}