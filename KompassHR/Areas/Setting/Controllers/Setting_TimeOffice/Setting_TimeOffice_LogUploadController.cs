using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;


namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_LogUploadController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_TimeOffice_LogUpload
        #region Setting_TimeOffice_LogUpload
        public ActionResult Setting_TimeOffice_LogUpload()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT AttenLogSettingId, LogSettingName, IsMonthWise FROM Atten_LogDownloadSetting WHERE Deactivate = 0");
                ViewBag.Get_LogDownloadSetting = DapperORM.ReturnList<Atten_LogDownloadSetting>("sp_QueryExcution", param).ToList();
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select USBId as Id, USBName as Name from Atten_LogColumnSettings where Deactivate=0");
                ViewBag.LogColumnSettings = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(Atten_LogUpload _LogUpload)
        {
            try
            {
                // Fetch DB connection details
                param.Add("@query", "Select * from Atten_LogDownloadSetting where Deactivate=0 and AttenLogSettingId=" + _LogUpload.Log_SettingId);
                var GetName = DapperORM.ReturnList<Atten_LogDownloadSetting>("sp_QueryExcution", param).FirstOrDefault();

                if (GetName == null)
                {
                    TempData["Message"] = "Settings not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
                }

                // Source database connection string
                //var sourceConnectionString = $"Server={GetName.ServerName};Database={GetName.DatabaseName};User Id={GetName.UserName};Password={GetName.Password};";

                // ✅ Build connection string safely

                string sourceConnectionString = @"Data Source=" + GetName.ServerName + ";uid=" + GetName.UserName + ";pwd=" + GetName.Password + ";Initial Catalog=" + GetName.DatabaseName + ";Integrated Security=false; Connection Timeout=0";
                DataTable dt = new DataTable();
                try
                {
                    using (SqlConnection Log_connection = new SqlConnection(sourceConnectionString))
                    {
                        Log_connection.Open();

                        // ✅ Table Name Handling
                        string tableName = GetName.TableName; // e.g., "DeviceLogs"
                        if (GetName.IsMonthWise)
                        {
                            int month = _LogUpload.FromDate.Month;
                            int year = _LogUpload.FromDate.Year;
                            tableName = $"{tableName}_{month}_{year}";
                        }
                        // ✅ First check if table exists
                        int tableExists = Log_connection.ExecuteScalar<int>(
                            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName",
                            new { tableName });

                        if (tableExists == 0)
                        {
                            TempData["Message"] = $"❌ Source table {tableName} does not exist.";
                            TempData["Icon"] = "error";
                            return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
                        }

                        // ✅ Build query safely (columns from GetName must exist in table)
                        string query = $@"
                        SELECT {GetName.UserId} AS UserId,
                       {GetName.DeviceId} AS DeviceId,
                       {GetName.LogDate} AS LogDate,
                       {GetName.Direction} AS Direction
                        FROM {tableName}
                        WHERE CONVERT(date, {GetName.LogDate}) 
                        BETWEEN @FromDate AND @ToDate";

                        var result = Log_connection.Query(query, new
                        {
                            FromDate = _LogUpload.FromDate.Date,
                            ToDate = _LogUpload.ToDate.Date
                        }).ToList();

                        // ✅ Convert result to DataTable
                        DapperORM dprObj = new DapperORM();
                        dt = dprObj.ConvertToDataTable(result);

                        TempData["Message"] = $"✅ {dt.Rows.Count} log records fetched from {tableName}.";
                        TempData["Icon"] = "success";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "❌ Error while fetching logs: " + ex.Message;
                    TempData["Icon"] = "error";
                    return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
                }
                //// Destination database connection string (assuming same for now, but you might need a different one)
                //var destinationConnectionString = sourceConnectionString;

                //DataTable dt = new DataTable();

                //// Query data from source DB
                //using (var Log_connection = new SqlConnection(sourceConnectionString))
                //{
                //    Log_connection.Open();

                //    string tableName = GetName.TableName;

                //    if (GetName.IsMonthWise)
                //    {
                //        int month = _LogUpload.FromDate.Month;
                //        int year = _LogUpload.FromDate.Year;
                //        tableName = $"{tableName}_{month}_{year}";
                //    }

                //    // First check if table exists
                //    var tableExists = Log_connection.QueryFirstOrDefault<int>(
                //        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName",
                //        new { tableName });

                //    if (tableExists == 0)
                //    {
                //        TempData["Message"] = $"Source table {tableName} does not exist.";
                //        TempData["Icon"] = "error";
                //        return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
                //    }

                //    var query = $"SELECT {GetName.UserId} AS UserId, {GetName.DeviceId} AS DeviceId, {GetName.LogDate} AS LogDate, {GetName.Direction} AS Direction " +
                //                $"FROM {tableName} " +
                //                $"WHERE CONVERT(date, {GetName.LogDate}) BETWEEN @FromDate AND @ToDate";

                //    var result = Log_connection.Query(query, new { FromDate = _LogUpload.FromDate.Date, ToDate = _LogUpload.ToDate.Date }).ToList();

                //    DapperORM dprObj = new DapperORM();
                //    dt = dprObj.ConvertToDataTable(result);
                //}

                if (dt.Rows.Count == 0)
                {
                    TempData["Message"] = "No records found for the specified date range.";
                    TempData["Icon"] = "info";
                    return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
                }

                // Add DownloadDate column to DataTable
                if (!dt.Columns.Contains("DownloadDate"))
                {
                    dt.Columns.Add("DownloadDate", typeof(DateTime));
                }
                foreach (DataRow row in dt.Rows)
                {
                    row["DownloadDate"] = DateTime.Now;
                }

                // Pad UserId to 10 digits with leading zeros
                foreach (DataRow row in dt.Rows)
                {
                    if (row["UserId"] != DBNull.Value)
                    {
                        string userId = row["UserId"].ToString().Trim();
                        row["UserId"] = userId.PadLeft(10, '0');
                    }
                }

                // Insert data into DeviceLogs using SqlBulkCopy
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    sqlcon.Open();

                    using (var transaction = sqlcon.BeginTransaction())
                    {
                        try
                        {
                            using (var bulkCopy = new SqlBulkCopy(sqlcon, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.DestinationTableName = "DeviceLogs";
                                bulkCopy.BatchSize = 5000; // Adjust batch size as needed
                                bulkCopy.BulkCopyTimeout = 600; // 10 minutes timeout

                                // Explicit column mappings
                                bulkCopy.ColumnMappings.Add("DownloadDate", "DownloadDate");
                                bulkCopy.ColumnMappings.Add("DeviceId", "DeviceId");
                                bulkCopy.ColumnMappings.Add("UserId", "UserId");
                                bulkCopy.ColumnMappings.Add("LogDate", "LogDate");
                                bulkCopy.ColumnMappings.Add("Direction", "Direction");

                                // Write to server
                                bulkCopy.WriteToServer(dt);
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                TempData["Message"] = $"Successfully inserted {dt.Rows.Count} records";
                TempData["Icon"] = "success";
                return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                // Log the full exception details including stack trace
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        [HttpPost]
        public ActionResult SaveLogFile(int USBName, string Direction, HttpPostedFileBase LogFile)
        {
            try
            {
                if (LogFile == null || LogFile.ContentLength == 0)
                {
                    TempData["Error"] = "No file uploaded.";
                    return RedirectToAction("Index");
                }

                var allowedExtensions = new[] { ".log", ".txt", ".csv", ".json", ".xml", ".dat" };
                var ext = Path.GetExtension(LogFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Invalid file type.";
                    return RedirectToAction("Index");
                }

                // Get column settings from DB
                var setting = DapperORM.DynamicQuerySingle(@"SELECT * FROM Atten_LogColumnSettings WHERE USBId = '" + USBName + "'");
                if (setting == null)
                {
                    TempData["Error"] = "No USB setting found.";
                    return RedirectToAction("Index");
                }

                int userIdCol = Convert.ToInt32(setting.UserId) - 1;
                int logDateCol = Convert.ToInt32(setting.LogDate) - 1;
                int deviceIdCol = Convert.ToInt32(setting.DeviceId) - 1;
                int directionCol = Convert.ToInt32(setting.Direction) - 1;

                string inValue = (setting.DirectionIn ?? "").Trim().ToUpper();
                string outValue = (setting.DirectionOut ?? "").Trim().ToUpper();

                using (var reader = new StreamReader(LogFile.InputStream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var cols = line.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();

                        if (cols.Length <= Math.Max(Math.Max(userIdCol, logDateCol), Math.Max(deviceIdCol, directionCol)))
                            continue;

                        string userId = cols[userIdCol]?.Trim();
                        string logDateStr = cols[logDateCol]?.Trim();
                        string deviceIdStr = cols[deviceIdCol]?.Trim();
                        string directionStr = cols[directionCol]?.Trim();

                        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(logDateStr) || string.IsNullOrEmpty(deviceIdStr))
                            continue;

                        // ✅ Pad UserId to 10 digits with leading zeros
                        userId = userId.PadLeft(10, '0');

                        DateTime logDate;
                        if (!DateTime.TryParse(logDateStr, out logDate)) continue;

                        int deviceId;
                        if (!int.TryParse(deviceIdStr, out deviceId)) continue;

                        string fileDirection = directionStr.ToUpper();
                        string finalDirection = null;

                        if (Direction == "IN")
                        {
                            finalDirection = "IN";
                        }
                        else if (Direction == "OUT")
                        {
                            finalDirection = "OUT";
                        }
                        else if (Direction == "BOTH")
                        {
                            if (fileDirection == inValue)
                                finalDirection = "IN";
                            else if (fileDirection == outValue)
                                finalDirection = "OUT";
                            else
                                continue;
                        }
                        else
                        {
                            continue;
                        }

                        // Insert into DeviceLogs
                        DapperORM.Execute(@"INSERT INTO DeviceLogs (DownloadDate, DeviceId, UserId, LogDate, Direction)
                    VALUES (@DownloadDate, @DeviceId, @UserId, @LogDate, @Direction)",
        new
        {
            DownloadDate = DateTime.Now,
            DeviceId = deviceId,
            UserId = userId,
            LogDate = logDate,
            Direction = finalDirection
        });

                    }
                }
                TempData["Message"] = "File Uploaded successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("Setting_TimeOffice_LogUpload", "Setting_TimeOffice_LogUpload", new { Area = "Setting" });

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("ErrorPage", "Login");

            }

        }
    }
}