using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

namespace KompassHR.Models
{
    public class clsCommonFunction : IDisposable
    {
        private readonly SqlConnection con;
        private readonly SqlCommand cmd;
        private readonly SqlDataAdapter ado;

        public clsCommonFunction()
        {
            // Always fetch connection string at runtime, not at class load
            string connectionString = HttpContext.Current.Session["MyNewConnectionString"]?.ToString();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not available in Session.");
            }

            con = new SqlConnection(connectionString);
            cmd = new SqlCommand();
            ado = new SqlDataAdapter();
        }

        // ✅ For SELECT queries
        public DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();

            try
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

                cmd.CommandText = query;
                cmd.Connection = con;
                cmd.CommandTimeout = 0;
                ado.SelectCommand = cmd;

                ado.Fill(dt);  // No need for cmd.ExecuteNonQuery()
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetDataTable: " + ex.Message, ex);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return dt;
        }

        // ✅ For INSERT/UPDATE/DELETE queries (wrapped in transaction)
        public bool SaveStringBuilder(StringBuilder sqlQuery, out string errorCheck)
        {
            errorCheck = "";
            bool result = false;
            SqlTransaction objTransaction = null;

            if (sqlQuery == null || sqlQuery.Length == 0)
            {
                errorCheck = "Query is empty.";
                return false;
            }

            try
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();

                objTransaction = con.BeginTransaction();
                using (SqlCommand cmd = new SqlCommand(sqlQuery.ToString(), con, objTransaction))
                {
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }

                objTransaction.Commit();
                result = true;
            }
            catch (Exception ex)
            {
                errorCheck = ex.Message;
                if (objTransaction != null)
                {
                    objTransaction.Rollback();
                }
                result = false;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            return result;
        }

        // Cleanup resources
        public void Dispose()
        {
            if (con != null)
                con.Dispose();

            if (cmd != null)
                cmd.Dispose();

            if (ado != null)
                ado.Dispose();
        }
    }
}


//--------------------OLD CODE BELOW------------



//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Web;

//namespace KompassHR.Models
//{
//    public class clsCommonFunction
//    {
//        SqlCommand cmd = new SqlCommand();     
//        SqlDataAdapter ado = new SqlDataAdapter();
//        SqlDataReader read;
//        public static string  connectionString = HttpContext.Current.Session["MyNewConnectionString"].ToString();
//        //SqlConnection con = new SqlConnection(DapperORM.connectionString);
//        SqlConnection con = new SqlConnection(connectionString);
//        public DataTable GetDataTable(string Query)
//        {
//            DataTable dt = new DataTable();
//            try
//            {
//                if (con != null && con.State == ConnectionState.Closed)
//                {
//                    con.Open();
//                }
//                con.Close();
//                con.Open();
//                DataSet ds = new DataSet();
//                cmd.CommandText = Query;
//                cmd.Connection = con;
//                cmd.CommandTimeout = 0;
//                ado.SelectCommand = cmd;

//                cmd.ExecuteNonQuery();
//                ado.Fill(ds);
//                dt = ds.Tables[0];
//                con.Close();
//            }
//            catch (Exception ex)
//            {

//            }
//            finally
//            {
//                con.Close();
//            }
//            return dt;

//        }



//        public bool SaveStringBuilder(StringBuilder sqlQuery ,  out string  ErrorCheck)
//        {
//            ErrorCheck = "";
//            if (con != null && con.State == ConnectionState.Closed)
//            {

//                con.Open();
//            }


//            SqlCommand cmd = new SqlCommand();
//            SqlTransaction objTransaction = null/* TODO Change to default(_) if this is not a reference type */;
//            bool result = false;
//            try
//            {
//                if (sqlQuery.Length == 0)
//                {
//                    return false;
//                }
//                else
//                {

//                    objTransaction = con.BeginTransaction();
//                    cmd = new SqlCommand(sqlQuery.ToString(), con, objTransaction);
//                    cmd.CommandTimeout = 0;
//                    cmd.ExecuteNonQuery();
//                    objTransaction.Commit();

//                    // Close the connection.
//                    con.Close();
//                    result = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                ErrorCheck = ex.Message.ToString();
//                if ((objTransaction) != null)
//                {
//                    objTransaction.Rollback();
//                    result = false;
//                }
//            }
//            finally
//            {
//                if (con.State == ConnectionState.Open)
//                    con.Close();
//            }

//            return result;
//        }




//    }



//}