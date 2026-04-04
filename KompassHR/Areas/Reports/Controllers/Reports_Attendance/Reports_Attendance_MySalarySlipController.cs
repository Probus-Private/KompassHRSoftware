using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SelectPdf;
using System.Net.Mail;


namespace KompassHR.Areas.Reports.Controllers.Reports_Attendance
{
    public class Reports_Attendance_MySalarySlipController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Reports/Reports_Attendance_MySalarySlip

        #region Reports_Attendance_MySalarySlip
        public ActionResult Reports_Attendance_MySalarySlip()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 504;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters SalarySlip = new DynamicParameters();
                SalarySlip.Add("@p_EmployeeId", Session["EmployeeId"]);
                var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_App_SalarySlipList", SalarySlip).ToList();
                ViewBag.GetPaySlipList = PayslipList;

                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@query",$@"SELECT 
                //                MAX(SalaryID) AS SalaryID,
                //                SalaryEmployeeName, 
                //                SalaryEmployeeNo, 
                //                SalaryDepartment, 
                //                SalaryDesignation, 
                //                MAX(SalaryNetPay) AS SalaryNetPay,
                //                FORMAT(SalaryMonthYear, 'MMM/yyyy') AS GetMonthName
                //            FROM Payroll_Salary
                //            WHERE ShowInESS = 1 
                //              AND Deactivate = 0 
                //              AND SalaryEmployeeId = {Session["EmployeeId"]}
                //              AND SalaryMonthYear >= DATEADD(MONTH, -24, GETDATE())
                //            GROUP BY SalaryEmployeeName, SalaryEmployeeNo, SalaryDepartment, SalaryDesignation, FORMAT(SalaryMonthYear, 'MMM/yyyy')
                //            ORDER BY MAX(SalaryMonthYear) DESC
                //            ");
                //var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                //ViewBag.GetPaySlipList = PayslipList;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Pay Slip Details Employee Wise 
        [HttpGet]
        public ActionResult GetPaySlipDetails(string SalaryID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters PersonalInfo = new DynamicParameters();
                PersonalInfo.Add("@p_SalaryID", SalaryID);
                var GetPersonal = DapperORM.ExecuteSP<dynamic>("sp_Rpt_Payroll_Payslip_Personal", PersonalInfo).ToList();


                DynamicParameters DeductionInfo = new DynamicParameters();
                DeductionInfo.Add("@p_SalaryID", SalaryID);
                var GetDeduction = DapperORM.ReturnList<Payroll_Deduction_Info>("sp_Rpt_Payroll_PaySlip_Deduction", DeductionInfo).ToList();

                DynamicParameters AttendanceInfo = new DynamicParameters();
                AttendanceInfo.Add("@p_SalaryID", SalaryID);
                var GetAttendance = DapperORM.ReturnList<Payroll_Atten_Info>("sp_Rpt_Payroll_PaySlip_Attendance", AttendanceInfo).ToList();


                DynamicParameters EarningInfo = new DynamicParameters();
                EarningInfo.Add("@p_SalaryID", SalaryID);
                var GetEarning = DapperORM.ReturnList<Payroll_Earning_Info>("sp_Rpt_Payroll_PaySlip_Earnings", EarningInfo).ToList();


                return Json(new { GetPersonal = GetPersonal, GetEarning = GetEarning, GetDeduction = GetDeduction, GetAttendance = GetAttendance }, JsonRequestBehavior.AllowGet);

                //using (var connection = new SqlConnection(DapperORM.connectionString))
                //{
                //    var param = new DynamicParameters();
                //    param.Add("@p_SalaryID", SalaryID);

                //    using (var multi = connection.QueryMultiple("sp_Rpt_Payroll_Payslip_Personal", param, commandType: CommandType.StoredProcedure))
                //    {
                //        var GetAttendace = multi.Read<dynamic>().ToList();
                //        var EmployeeDetails = "";
                //        // Return as JSON for AJAX
                //        return Json(new { EmployeeDetails = EmployeeDetails, GetAttendace = GetAttendace }, JsonRequestBehavior.AllowGet);
                //    }
                //}

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Code For Send Mail
        public class PDFRequest
        {
            public string HtmlContent { get; set; }
        }

        [HttpPost]
        public JsonResult GenerateAndSendEmail(PDFRequest request, string EmpId)
        {
            try
            {
                if (string.IsNullOrEmpty(request.HtmlContent))
                {
                    return Json(new { success = false, error = "HTML content is empty." });
                }

                var GetEmpEmailAndName = DapperORM.DynamicQuerySingle($@"Select PersonalEmailId,NameAsPerAadhaar from Mas_Employee_Personal where Deactivate=0 and PersonalEmployeeId=(Select EmployeeId from Mas_Employee where EmployeeId ='" + EmpId + "' AND Deactivate = 0 )");

                // Generate PDF from HTML
                var converter = new HtmlToPdf();
                var pdfDocument = converter.ConvertHtmlString(request.HtmlContent);
                string pdfPath = Server.MapPath("~/Temp/Payslip.pdf");
                pdfDocument.Save(pdfPath);
                pdfDocument.Close();

                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + 1 + "'  and  Origin='" + 5 + "'");
                var GetToolEmail = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramToolEmail).FirstOrDefault();

                // var ServerDetail = DapperORM.DynamicQuerySingle("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + GetCMPID.CmpID + "'  and  Origin='" + 1 + "' ");
                if (GetToolEmail != null)
                {
                    var SMTPServerName = GetToolEmail.SMTPServerName;
                    var PortNo = GetToolEmail.PortNo;
                    var FromEmailId = GetToolEmail.FromEmailId;
                    var SMTPPassword = GetToolEmail.Password;
                    var SSL = GetToolEmail.SSL;

                    SmtpClient smtp = new SmtpClient(SMTPServerName, PortNo);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = true;
                    smtp.Timeout = 100000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    smtp.Credentials = new System.Net.NetworkCredential(FromEmailId, SMTPPassword);

                    MailMessage mail = new MailMessage(FromEmailId, GetEmpEmailAndName.PersonalEmailId);
                    mail.Subject = "Salary Slip";
                    var GetBody = "Dear " + GetEmpEmailAndName.NameAsPerAadhaar + " Your Salary Slip<br>.</br><br>KompassHR</br>";
                    mail.Body = GetBody;
                    //  mail.Attachments.Add(new Attachment(Fbank.ExportToStream(ExportFormatType.PortableDocFormat), "PaymentSlip.pdf"));
                    mail.IsBodyHtml = true;

                    // Attach the PDF
                    mail.Attachments.Add(new Attachment(pdfPath));

                    smtp.Send(mail);
                    // Dispose of the attachment to release the file lock
                    mail.Attachments.Dispose();
                    // Clean up the PDF file
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                    }
                }
                //return Json(data, JsonRequestBehavior.AllowGet);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion  

    }
}