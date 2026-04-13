using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using static KompassHR.Areas.ESS.Controllers.ESS_Recruitment.ESS_Recruitment_OfferLetterApprovedListController;

namespace KompassHR.Areas.App.Controllers.App_Dashboard
{
    public class App_DashboardController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();

        // GET: App/App_Dashboard
        public ActionResult App_Dashboard()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        public ActionResult App_ShowMoreDashboard()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }

        #region
        public ActionResult App_Holiday()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@query", "Select EmployeeBranchId from Mas_Employee where Deactivate=0 and EmployeeId="+Session["EmployeeId"]+"");
                var GetBranchId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCount).FirstOrDefault();
                DynamicParameters PublicHoliday = new DynamicParameters();
                PublicHoliday.Add("@p_BranchId", GetBranchId.EmployeeBranchId);
                var data = DapperORM.DynamicList("sp_List_Atten_PublicHoliday_ESS", PublicHoliday);
                ViewBag.GetPublicHolidaysList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
            }
        }
        #endregion

        #region
        public ActionResult App_SalarySlip()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                }
                DynamicParameters SalarySlip = new DynamicParameters();
                SalarySlip.Add("@p_EmployeeId", Session["EmployeeId"]);
                var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_App_SalarySlipList", SalarySlip).ToList();
                ViewBag.GetPaySlipList = PayslipList;

                //SalarySlip.Add("@query", @"SELECT SalaryID, SalaryEmployeeName, SalaryEmployeeNo, SalaryDepartment, SalaryDesignation, SalaryNetPay, FORMAT(SalaryMonthYear, 'MMM/yyyy') AS GetMonthName FROM Payroll_Salary WHERE Deactivate = 0    AND SalaryEmployeeId = " + Session["EmployeeId"] + "   AND SalaryMonthYear >= DATEADD(MONTH, -12, GETDATE()) ORDER BY SalaryMonthYear DESC");
                //var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", SalarySlip).ToList();
                //ViewBag.GetPaySlipList = PayslipList;

                var GetMailId = DapperORM.DynamicQuerySingle("Select PersonalEmailId from Mas_Employee_Personal where PersonalEmployeeId='"+Session["EmployeeId"]+"' and Deactivate=0");
                TempData["GetEmployeeEmailId"] = GetMailId != null ? GetMailId.PersonalEmailId : null;
                //var GetMailId = DapperORM.DynamicQuerySingle<dynamic>($@"Select PersonalEmailId from Mas_Employee_Personal where PersonalEmployeeId='{Session["EmployeeId"]}' and Deactivate=0");
                //TempData["GetEmployeeEmailId"] = GetMailId?.PersonalEmailId != null
                //? GetMailId.PersonalEmailId
                //: "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
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
                    return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
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
                
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #region NewSettingWise
        [HttpGet]
        public JsonResult GetPayslipPreview(int SalaryID)
        {
            try
            {
                // Get the active template ID
                var paramTemplate = new DynamicParameters();
                paramTemplate.Add("@query", @"SELECT TOP 1 Payslip_TemplateId FROM Payroll_PayslipSetting");
                var templateResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramTemplate).FirstOrDefault();
                var templateId = templateResult?.Payslip_TemplateId ?? 1;

                var employeeNo = new DynamicParameters();
                employeeNo.Add("@query", @"select SalaryEmployeeNo from Payroll_Salary where salaryId=" + SalaryID + " ");
                var emp = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", employeeNo).FirstOrDefault();
                var empNo = emp.SalaryEmployeeNo ?? 1;

                // Generate payslip HTML
                var payslipParams = new DynamicParameters();
                payslipParams.Add("@p_SalaryID", SalaryID);
                payslipParams.Add("@p_Template_Id", templateId);

                var payslipResult = DapperORM.ExecuteSP<dynamic>("sp_GeneratePayslipHTML", payslipParams)
                                             .FirstOrDefault();

                if (payslipResult == null || string.IsNullOrEmpty(payslipResult.PayslipHTML))
                {
                    return Json(new { success = false, message = "Failed to generate payslip" });
                }

                string html = payslipResult.PayslipHTML;


                string logoPath = Session["CompanyLogo"].ToString();
                html = html.Replace("{{CompanyLogo}}", logoPath);



                return Json(new { success = true, html = html, empNo = empNo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion NewSettingWise

        #endregion

        [HttpPost]
        public JsonResult GenerateAndSendEmail(PDFRequest request, string EmpNo)
        {
            try
            {
                if (string.IsNullOrEmpty(request.HtmlContent))
                {
                    return Json(new { success = false, error = "HTML content is empty." });
                }

                DynamicParameters GetEmail = new DynamicParameters();
                GetEmail.Add("@query", $@"SELECT PersonalEmailId, NameAsPerAadhaar
                                                            FROM Mas_Employee_Personal
                                                            WHERE Deactivate = 0
                                                              AND PersonalEmployeeId IN(
                                                                  SELECT EmployeeId
                                                                  FROM Mas_Employee
                                                                  WHERE EmployeeNo = '"+EmpNo+ "' and Deactivate=0)");
                var GetEmpEmailAndName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", GetEmail).FirstOrDefault();

                //var GetEmpEmailAndName = DapperORM.DynamicQuerySingle($@"SELECT PersonalEmailId, NameAsPerAadhaar
                //                                            FROM Mas_Employee_Personal
                //                                            WHERE Deactivate = 0
                //                                              AND PersonalEmployeeId IN (
                //                                                  SELECT EmployeeId
                //                                                  FROM Mas_Employee
                //                                                  WHERE EmployeeNo = '{EmpNo}'
                 //                                            );");
                //var GetEmpEmailAndName = "ashafak007@gmail.com";
                // Generate PDF from HTML
                var converter = new HtmlToPdf();
                var pdfDocument = converter.ConvertHtmlString(request.HtmlContent);
                string pdfPath = Server.MapPath("~/Temp/Payslip.pdf");
                pdfDocument.Save(pdfPath);
                pdfDocument.Close();

                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + 1 + "'  and  Origin='" + 1 + "'");
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
                TempData["PersonalEmailId"] = GetEmpEmailAndName.PersonalEmailId;
                //return Json(data, JsonRequestBehavior.AllowGet);
                return Json(new { success = true  , email = TempData["PersonalEmailId"] });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult SendPayslipEmail(int SalaryID , string EmpNo)
        {
            try
            {
                var paramTemplate = new DynamicParameters();
                paramTemplate.Add("@query", @"SELECT TOP 1 Payslip_TemplateId FROM Payroll_PayslipSetting");
                var templateResult = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramTemplate).FirstOrDefault();
                var templateId = templateResult?.Payslip_TemplateId ?? 1;

                var employeeNo = new DynamicParameters();
                employeeNo.Add("@query", @"select SalaryEmployeeNo from Payroll_Salary where salaryId=" + SalaryID + " ");
                var emp = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", employeeNo).FirstOrDefault();
                var empNo = emp.SalaryEmployeeNo ?? 1;

                // Generate payslip HTML
                var payslipParams = new DynamicParameters();
                payslipParams.Add("@p_SalaryID", SalaryID);
                payslipParams.Add("@p_Template_Id", templateId);

                var payslipResult = DapperORM.ExecuteSP<dynamic>("sp_GeneratePayslipHTML", payslipParams)
                                             .FirstOrDefault();
                string html = payslipResult.PayslipHTML;

                string logoPath = Session["CompanyLogo"].ToString();
                html = html.Replace("{{CompanyLogo}}", logoPath);

                EmpNo = "001668";
                DynamicParameters GetEmail = new DynamicParameters();
                GetEmail.Add("@query", $@"SELECT PersonalEmailId, NameAsPerAadhaar
                                                            FROM Mas_Employee_Personal
                                                            WHERE Deactivate = 0
                                                              AND PersonalEmployeeId IN(
                                                                  SELECT EmployeeId
                                                                  FROM Mas_Employee
                                                                  WHERE EmployeeNo = '" + EmpNo + "' and Deactivate=0)");
                var GetEmpEmailAndName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", GetEmail).FirstOrDefault();

                // Convert HTML → PDF
                //byte[] pdfBytes = GeneratePdf(html);
                var converter = new HtmlToPdf();
                var pdfDocument = converter.ConvertHtmlString(html);
                string pdfPath = Server.MapPath("~/Temp/Payslip.pdf");
                pdfDocument.Save(pdfPath);
                pdfDocument.Close();
                // Send email

                DynamicParameters paramToolEmail = new DynamicParameters();
                paramToolEmail.Add("@query", "Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where  Deactivate=0 and CmpId='" + 1 + "'  and  Origin='" + 1 + "'");
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
                TempData["PersonalEmailId"] = GetEmpEmailAndName.PersonalEmailId;

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult App_CRM()
        {
            try
            {
                try
                {
                    if (Session["EmployeeId"] == null)
                    {
                        return RedirectToAction("App_SessionExpire", "App_Login", new { area = "App" });
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    Session["GetErrorMessage"] = ex.Message;
                    return RedirectToAction("App_ErrorPage", "App_Login", new { area = "App" });
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }

    }
}