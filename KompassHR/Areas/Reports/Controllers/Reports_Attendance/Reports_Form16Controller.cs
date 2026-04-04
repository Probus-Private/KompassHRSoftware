using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Form16Controller : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Reports/Reports_Form16
        public ActionResult Reports_Form16()
        {
            try
            {
                // Session Check
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                int screenId = Request.QueryString["ScreenId"] != null
                    ? Convert.ToInt32(Request.QueryString["ScreenId"])
                    : 643;

                bool hasAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!hasAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                // Fetch Form16 folder path from Tool_CommonTable
                var param = new DynamicParameters();
                param.Add("@query", "SELECT Form16Path FROM Tool_CommonTable");
                var result = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                // Fetch employee PAN
                int empId = Convert.ToInt32(Session["EmployeeId"]);
                var paramEmp = new DynamicParameters();
                paramEmp.Add("@query", $"SELECT PAN FROM Mas_Employee_Personal WHERE deactivate = 0 AND PersonalEmployeeId = '{empId}'");
                var empData = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramEmp).FirstOrDefault();

                string pan = empData?.PAN?.ToString()?.ToUpper();

                if (result != null && !string.IsNullOrEmpty(pan))
                {
                    string folderPath = result.Form16Path;
                    if (Directory.Exists(folderPath))
                    {
                        var files = Directory.GetFiles(folderPath)
                                             .Where(f => Path.GetFileName(f).ToUpper().Contains(pan))
                                             .Select(f => Tuple.Create(Path.GetFileName(f), f))
                                             .ToList();

                        ViewBag.FileList = files;
                        ViewBag.DirectoryPath = folderPath;
                    }
                    else
                    {
                        ViewBag.FileList = new List<Tuple<string, string>>();
                        ViewBag.Error = "Form16 folder does not exist.";
                    }
                }
                else
                {
                    ViewBag.FileList = new List<Tuple<string, string>>();
                    ViewBag.Error = "Employee PAN not found or Form16 path missing.";
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult DownloadFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return HttpNotFound();

                // Get the folder path from DB again or reuse logic
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT Form16Path FROM Tool_CommonTable");
                var result = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                if (result == null) return HttpNotFound();

                string folderPath = result.Form16Path;
                string fullFilePath = Path.Combine(folderPath, fileName);

                if (!System.IO.File.Exists(fullFilePath))
                    return HttpNotFound("File not found.");

                string contentType = MimeMapping.GetMimeMapping(fileName);
                return File(fullFilePath, contentType, fileName);
            }
            catch
            {
                return RedirectToAction("ErrorPage", "Login");
            }
        }

    }
}