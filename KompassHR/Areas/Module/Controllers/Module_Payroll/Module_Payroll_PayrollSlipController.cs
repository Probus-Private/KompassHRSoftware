using Dapper;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Reports.Models;
using System.Drawing;
using System.IO;

using SelectPdf;
using System.Net.Mail;

namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_PayrollSlipController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Payroll_PayrollSlip
      
        #region Main View
        public ActionResult Module_Payroll_PayrollSlip(int? CmpId, int? BranchId, DateTime? Month)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 167;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;



                if (Month != null)
                {
                    var Getmonth = Month?.ToString("yyyy-MM-dd");
                    DynamicParameters paramCompany = new DynamicParameters();
                    if(BranchId!=null)
                    {
                        paramCompany.Add("@query", @"Select SalaryID,SalaryEmployeeName,SalaryEmployeeNo,SalaryDepartment,SalaryDesignation,SalaryNetPay " +
                                            "From Payroll_Salary Where Deactivate=0 AND Month(SalaryMonthYear) = Month('" + Getmonth + "')AND YEAR(SalaryMonthYear) = YEAR('" + Getmonth + "')" +
                                            "AND SalaryCmpId=" + CmpId + " AND (" + BranchId + " IS NULL OR SalaryBranchId = " + BranchId + ")");
                        var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                        ViewBag.GetPaySlipList = PayslipList;
                        if (PayslipList.Count==0)
                        {
                            TempData["Message"] = "Record Not Found";
                            TempData["Icon"] = "info";
                        }
                    }
                    else
                    {
                        paramCompany.Add("@query", @"Select SalaryID,SalaryEmployeeName,SalaryEmployeeNo,SalaryDepartment,SalaryDesignation,SalaryNetPay " +
                                                                    "From Payroll_Salary Where Deactivate=0 AND Month(SalaryMonthYear) = Month('" + Getmonth + "') AND YEAR(SalaryMonthYear) = YEAR('" + Getmonth + "')" +
                                                                    "AND SalaryCmpId=" + CmpId + " ");
                        var PayslipList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramCompany).ToList();
                        ViewBag.GetPaySlipList = PayslipList;
                        if (PayslipList.Count == 0)
                        {
                            TempData["Message"] = "Record Not Found";
                            TempData["Icon"] = "info";
                        }
                    }
                    //GET BRANCH NAME
                    var GetBranch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = GetBranch;


                    //SET COMPANY LOGO COMPANY WISE
                    var path = DapperORM.DynamicQuerySingle("Select Logo from Mas_CompanyProfile Where CompanyId = " + CmpId + "");
                    var SecondPath = path.Logo;
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyLogo'");
                    var FisrtPath = GetDocPath.DocInitialPath + CmpId + "\\";
                    string GetBase64 = null;
                    string fullPath = "";
                    fullPath = FisrtPath + SecondPath;

                    string extensionType = Path.GetExtension(fullPath).ToLower();
                    string mimeType = "image/jpeg";

                    if (extensionType == ".png")
                    {
                        mimeType = "image/png";
                    }
                    else if (extensionType == ".jpg" || extensionType == ".jpeg")
                    {
                        mimeType = "image/jpeg";
                    }

                    //string Extention = System.IO.Path.GetExtension(fullPath);
                    if (path.Logo != null)
                    {
                        try
                        {
                            string directoryPath = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(directoryPath))
                            {
                                Session["PaySlipCompanyLogo"] = "";
                            }
                            else
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        Session.Remove("PaySlipCompanyLogo");
                                        //Session["PaySlipCompanyLogo"] = "data:image; base64," + base64String;
                                        Session["PaySlipCompanyLogo"] = "data:" + mimeType + ";base64," + base64String;

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message != null)
                            {
                                Session["CompanyLogo"] = "";
                            }
                        }
                    }




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

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetPaySlipDetails(int SalaryID)
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
                employeeNo.Add("@query", @"select SalaryEmployeeNo from Payroll_Salary where salaryId="+SalaryID+" ");
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

                // Replace company logo (if your template has {{CompanyLogo}})
                string logoPath = Session["CompanyLogo"]?.ToString() ?? "/images/default-logo.png";
                html = html.Replace("{{CompanyLogo}}", logoPath);

                // Optional: you can also pass other data if needed, but usually not necessary
                return Json(new{success = true,html = html, empNo = empNo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region Code For Send Mail
        public class PDFRequest
        {
            public string HtmlContent { get; set; }
        }

        [HttpPost]
        public JsonResult GenerateAndSendEmail(PDFRequest request, string EmpNo)
        {
            try
            {
                if (string.IsNullOrEmpty(request.HtmlContent))
                {
                    return Json(new { success = false, error = "HTML content is empty." });
                }

                var GetEmpEmailAndName = DapperORM.DynamicQuerySingle($@"Select PersonalEmailId,NameAsPerAadhaar from Mas_Employee_Personal where Deactivate=0 and PersonalEmployeeId=(Select EmployeeId from Mas_Employee where EmployeeNo='"+ EmpNo +"' AND Deactivate = 0 )");

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
                    var GetBody = "Dear "+GetEmpEmailAndName.NameAsPerAadhaar+" Your Salary Slip<br>.</br><br>KompassHR</br>";
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