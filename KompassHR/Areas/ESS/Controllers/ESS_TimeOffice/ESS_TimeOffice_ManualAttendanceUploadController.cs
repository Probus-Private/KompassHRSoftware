using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_ManualAttendanceUploadController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_TimeOffice_ManualAttendanceUpload
        #region ESS_TimeOffice_ManualAttendanceUpload
        public ActionResult ESS_TimeOffice_ManualAttendanceUpload()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 302;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        //#region IsFromatCheck
        //public ActionResult IsFromatCheck(List<BulkCheckInOut> TestOBJAttenINOUT)
        //{

        //    try
        //    {               
        //        for (var i = 0; i < TestOBJAttenINOUT.Count; i++)
        //        {
        //            if (IsValidDateTime(TestOBJAttenINOUT[i].InTime, TestOBJAttenINOUT[i].OutTime))
        //            {
        //                TempData["Icon"] = "success";
        //                TempData["Message"] = "";
        //            }
        //            else
        //            {
        //                TempData["Message"] = $"Invalid datetime format of In Time: {TestOBJAttenINOUT[i].InTime} Employee Card No  " + TestOBJAttenINOUT[i].EmployeeCardNo + "";
        //                TempData["Icon"] = "error";
        //                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        //                //Console.WriteLine($"Invalid datetime format: {TestOBJAttenINOUT[i].InTime}");
        //            }

        //            if (IsValidDateTime(TestOBJAttenINOUT[i].InTime, TestOBJAttenINOUT[i].OutTime))
        //            {
        //                TempData["Message"] = "";
        //                TempData["Icon"] = "success";
        //                //Console.WriteLine($"Valid datetime format: {TestOBJAttenINOUT[i].InTime}");
        //            }
        //            else
        //            {
        //                TempData["Message"] = $"Invalid datetime format of Out Time: {TestOBJAttenINOUT[i].OutTime}  Employee Card No  " + TestOBJAttenINOUT[i].EmployeeCardNo + "";
        //                TempData["Icon"] = "error";
        //                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        //                //Console.WriteLine($"Invalid datetime format: {TestOBJAttenINOUT[i].InTime}");
        //            }
        //        }
        //        if (TempData["Message"] != "")
        //        {
        //            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<BulkCheckInOut> TestOBJAttenINOUT)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 302;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                for (var i = 0; i < TestOBJAttenINOUT.Count; i++)
                {
                    //int desiredLength = 10;
                    //string EmployeeNo = TestOBJAttenINOUT[i].EmployeeNo.ToString();
                    //if(EmployeeNo.Length < 10)
                    //{
                    //    EmployeeNo = EmployeeNo.PadLeft(desiredLength, '0');
                    //}
                    var CheckIsValid = DapperORM.DynamicQueryList(@"Select * from Mas_Employee where EmployeeNo='" + TestOBJAttenINOUT[i].EmployeeNo + "' and Deactivate=0").FirstOrDefault();
                    if (CheckIsValid == null)
                    {
                        TempData["Message"] = "Invalid Employee Crad No. " + TestOBJAttenINOUT[i].EmployeeNo + "";
                        TempData["Icon"] = "error";
                        return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    var p_process = "";
                    string InDateTimeOnly = "";
                    if (TestOBJAttenINOUT[i].InTime != null)
                    {
                        DateTime IndateTime = DateTime.Parse(TestOBJAttenINOUT[i].InTime);
                        InDateTimeOnly = IndateTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        InDateTimeOnly = "";
                    }

                    string OutDateTimeOnly = "";
                    if (TestOBJAttenINOUT[i].OutTime != null)
                    {
                        DateTime OutdateTime = DateTime.Parse(TestOBJAttenINOUT[i].OutTime);
                        OutDateTimeOnly = OutdateTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        OutDateTimeOnly = "";
                    }


                    var CheckIsExist = DapperORM.DynamicQueryList(@"Select * from Atten_ManualLogUpload where UserId='" + TestOBJAttenINOUT[i].EmployeeNo + "' and Deactivate=0 and CONVERT(DATE, InDateTime) = CONVERT(DATE, '" + InDateTimeOnly + "')  and CONVERT(DATE, OutDateTime)= CONVERT(DATE, '" + OutDateTimeOnly + "')").FirstOrDefault();
                    if (CheckIsExist != null)
                    {
                        p_process = "Update";
                        //TempData["Message"] = "Alredy exist this employee no. " + TestOBJAttenINOUT[i].EmployeeNo + "";
                        //TempData["Icon"] = "error";
                        //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        p_process = "Save";
                    }
                    DynamicParameters paramINOUT = new DynamicParameters();
                    paramINOUT.Add("@p_process", p_process);
                    paramINOUT.Add("@p_UserID", TestOBJAttenINOUT[i].EmployeeNo);
                    paramINOUT.Add("@p_CreatedupdateBy", Convert.ToString(Session["EmployeeName"]));
                    paramINOUT.Add("@p_MachineName", System.Net.Dns.GetHostName().ToString());

                    string formattedInTime = "";
                    string formattedOutTime = "";
                    if (TestOBJAttenINOUT[i].InTime != null)
                    {
                        formattedInTime = TestOBJAttenINOUT[i].InTime.Replace("T", " ");
                    }

                    if (TestOBJAttenINOUT[i].OutTime != null)
                    {
                        formattedOutTime = TestOBJAttenINOUT[i].OutTime.Replace("T", " ");
                    }


                    paramINOUT.Add("@p_InDateTime", formattedInTime);
                    paramINOUT.Add("@p_OutDateTime", formattedOutTime);
                    paramINOUT.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    paramINOUT.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_ManualLogsUpload", paramINOUT);
                    TempData["Message"] = paramINOUT.Get<string>("@p_msg");
                    TempData["Icon"] = paramINOUT.Get<string>("@p_Icon");
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region DownloadFile
        public ActionResult DownloadFile()
        {
            // Path to the file you want to download
            string filePath = Server.MapPath("~/assets/BulkUpload/Manual_Attendance.xlsx");

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }

            // Get the file content
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Set the content type
            string contentType = MimeMapping.GetMimeMapping(filePath);

            // Return the file as a byte array
            return File(fileBytes, contentType, "Manual_Attendance.xlsx");
        }
        #endregion

        #region IsValidDateTime
        public bool IsValidDateTime(string input, string input1)
        {
            string[] formats = { "yyyy-MM-dd HH:mm" };
            DateTime result;
            bool isValidate = true;
            bool isValid = true;
            bool isValid1 = true;
            foreach (string format in formats)
            {
                if (input == "")
                {
                    isValid = true;
                }
                else
                {
                    isValid = DateTime.TryParseExact(input, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);
                }
                if (input1 == "")
                {
                    isValid = true;
                }
                else
                {
                    isValid1 = DateTime.TryParseExact(input1, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result);
                }

                if (isValid == true && isValid1 == true)
                {
                    return isValidate;
                }
                else
                {
                    isValidate = false;
                    return isValidate;
                }
            }
            return false;
        }
        #endregion

        #region ImportExcelFile
        [HttpPost]
        public ActionResult ESS_TimeOffice_ManualAttendanceUpload(HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                List<BulkCheckInOut> excelDataList = new List<BulkCheckInOut>();

                if (AttachFile.ContentLength > 0)
                {

                    string directoryPath = Server.MapPath("~/assets/ManualInsert");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    // Get the file name
                    string fileName = Path.GetFileName(AttachFile.FileName);

                    // Combine directory path with file name to get full file path
                    string filePath = Path.Combine(directoryPath, fileName);
                    // Save the file
                    AttachFile.SaveAs(filePath);

                    XLWorkbook xlWorkwook = new XLWorkbook(filePath);
                    int row = 2;
                    if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                    {

                        TempData["Message"] = "Fill in missing information in the first column.";
                        TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                        TempData["Icon"] = "error";
                        return RedirectToAction("ESS_TimeOffice_ManualAttendanceUpload", "ESS_TimeOffice_ManualAttendanceUpload");
                    }
                    while (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() != "")
                    {
                        BulkCheckInOut ManualInsert = new BulkCheckInOut();
                        ManualInsert.EmployeeNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString();

                        string cardNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                        int desiredLength = 10;
                        cardNo = cardNo.PadLeft(desiredLength, '0');
                        //ManualInsert.EmployeeCardNo = cardNo;

                        //ManualInsert.EmployeeCardNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                        string datetime1 = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString();
                        string datetime2 = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                        string formattedDatetime1 = "";
                        string formattedDatetime2 = "";
                        if (datetime1 != "")
                        {
                            //  formattedDatetime1 = datetime1.Replace(" ", "T");

                            formattedDatetime1 = Convert.ToDateTime(datetime1).ToString("yyyy-MM-ddTHH:mm");
                        }

                        if (datetime2 != "")
                        {
                            // formattedDatetime2 = datetime2.Replace(" ", "T");

                            formattedDatetime2 = Convert.ToDateTime(datetime2).ToString("yyyy-MM-ddTHH:mm");
                        }
                        ManualInsert.InTime = formattedDatetime1;
                        ManualInsert.OutTime = formattedDatetime2;

                        //var CheckValidDateFormat = IsValidDateTime(datetime1, datetime2);
                        //if(CheckValidDateFormat==false)
                        //{
                        //    TempData["Message"] = "please insert excel date is valid format";
                        //    TempData["Title"] = "Excel date format is invalid";
                        //    TempData["Icon"] = "error";
                        //    return RedirectToAction("ESS_TimeOffice_ManualAttendanceUpload", "ESS_TimeOffice_ManualAttendanceUpload");
                        //}

                        //ManualInsert.InTime = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString();
                        //ManualInsert.OutTime = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString();
                        excelDataList.Add(ManualInsert);
                        row++;
                    }
                    System.IO.File.Delete(filePath);
                    ViewBag.count = 1;
                    ViewBag.GetExceldata = excelDataList;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}