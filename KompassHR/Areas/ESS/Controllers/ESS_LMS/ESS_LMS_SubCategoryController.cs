using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Reflection;
using System.Data;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;

namespace KompassHR.Areas.ESS.Controllers.ESS_LMS
{
    public class ESS_LMS_SubCategoryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_LMS_SubCategory
        public ActionResult ESS_LMS_SubCategory(int? CategoryId, string CategoryName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_CompanyId", Session["CompanyId"]);
                param1.Add("@p_BranchId", Session["BranchId"]);
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@P_LMSCategoryId", CategoryId);
                ViewBag.SubCategory = DapperORM.ExecuteSP<dynamic>("sp_LMS_ESS_SubCategory", param1).ToList();
                TempData["CategoryName"] = CategoryName;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region CategoryView
        public ActionResult CategoryView(string filename, int? LMSCategoryId, int? LMSLibraryId, string CategoryName, string CategoryDesc, string SubCategoryName, bool? IsAssessment, bool? IsDigitalSignature)
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {
                    TempData["Cat_SubCatName"] = CategoryName + " - " + SubCategoryName;
                    TempData["CategoryDesc"] = CategoryDesc;
                    TempData["LMSCategoryId"] = LMSCategoryId;
                    TempData["LMSLibraryId"] = LMSLibraryId;
                    TempData["IsAssessment"] = IsAssessment;

                    TempData["IsDigitalSignature"] = IsDigitalSignature ?? false;
                    var GetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='LMS'");
                    var FirstPath = GetPath.DocInitialPath + LMSLibraryId + "\\";

                    var DocRead = DapperORM.DynamicQuerySingle("Select * from Tool_LMS_DownloadUpload_URL where DocOrigin='LMSDocRead'");
                    var GetDocRead = DocRead.DownloadUploadURL;
                    var GetUrl = GetDocRead + LMSLibraryId + "/";

                    var employeeId = Convert.ToInt32(Session["EmployeeId"]);

                    //MULTIQUERY GET DETAILS USING SP
                    var parameters = new DynamicParameters();
                    parameters.Add("@p_LMSLibraryId", LMSLibraryId);
                    parameters.Add("@p_EmployeeId", employeeId);
                    parameters.Add("@p_LMSCategoryId", LMSCategoryId);
                    List<dynamic> GetSecondPath;
                   
                    dynamic GetLMS_EmployeeFinalSubmit;
                    using (var result = DapperORM.DynamicMultipleResult("sp_LMS_GetLibraryDetails", parameters))
                    {
                        GetSecondPath = result.Read<dynamic>();
                        GetLMS_EmployeeFinalSubmit = result.Read<dynamic>();
                    }
                    TempData["LMS_EmployeeFinalSubmit"] = GetLMS_EmployeeFinalSubmit;

                    //var json = JsonConvert.SerializeObject(GetFileTypes);
                    //List<dynamic> fileTypeCounts = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    //string sql = $@"
                    //   SELECT LMSLibrary_Document.LMSLibraryDocId
                    //  ,LMSLibrary_Document.LMS_Library_LMSLibraryId
                    //  ,LMSLibrary_Document.DocumentPath
                    //  ,LMSLibrary_Document.Title
                    //  ,LMS_Employee_ReadDocument.IsReadDocument
                    //   FROM LMSLibrary_Document
                    //   LEFT JOIN LMS_Employee_ReadDocument ON LMSLibrary_Document.LMSLibraryDocId = LMS_Employee_ReadDocument.DocumentId
                    //   AND LMS_Employee_ReadDocument.Deactivate = 0
                    //   WHERE LMSLibrary_Document.Deactivate = 0
                    //   AND LMSLibrary_Document.LMS_Library_LMSLibraryId = {LMSLibraryId};
                    //   SELECT IsAssessmentActive,IsFinalSubmit,CreatedDate from LMS_Employee_FinalSubmit where EmployeeId={employeeId} And CategoryId={LMSCategoryId} And LibraryId={LMSLibraryId};
                    //";

                    //List<dynamic> GetSecondPath;
                    //dynamic GetLMS_EmployeeFinalSubmit;
                    //using (var result = DapperORM.DynamicQuerySingleMultiple(sql))
                    //{
                    //    GetSecondPath = result.Read<dynamic>().ToList();
                    //    GetLMS_EmployeeFinalSubmit = result.Read<dynamic>().FirstOrDefault();
                    //}
                    //TempData["LMS_EmployeeFinalSubmit"] = GetLMS_EmployeeFinalSubmit;

                    // Step 1: Build sourcePaths with FullPath, Title, and Extension
                    var sourcePaths = GetSecondPath
                        .Where(x => x.DocumentPath != null)
                        .Select(x => new
                        {
                            LibraryDocId = x.LMSLibraryDocId,
                            FullPath = Path.Combine(GetUrl, (string)x.DocumentPath),
                            Title = (string)x.Title,
                            Extension = Path.GetExtension((string)x.DocumentPath)?.TrimStart('.').ToUpper(),
                            IsReadDocument = x.IsReadDocument,
                        }).ToList();
                    // Step 2: Filter by valid extensions and build final fileList
                    var fileList = sourcePaths
                        .Where(x => new[] { "PDF", "JPG", "JPEG", "PNG", "MP4", "AVI" }.Contains(x.Extension))
                        .Select(x => new
                        {
                            FileSrc = x.FullPath.Replace("\\", "/"),
                            FileType = x.Extension,
                            Title = x.Title,
                            LibraryDocId = x.LibraryDocId,
                            IsReadDocument = x.IsReadDocument
                        })
                        .ToList();
                    TempData["FileList"] = fileList;
                    string fullUrl = Request.Url.AbsoluteUri;
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

        #region ReadDocuments
        [HttpPost]
        public ActionResult ReadDocuments(int LibraryDocId, int CategoryId, int LibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                //param1.Add("@p_process", "Save");
                param1.Add("@p_DocumentId", LibraryDocId);
                param1.Add("@p_CategoryId", CategoryId);
                param1.Add("@p_LibraryId", LibraryId);
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_IsReadDocument", 1);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_LMS_Employee_ReadDocument", param1);
                var msg = param1.Get<string>("@p_msg");
                var Icon = param1.Get<string>("@p_Icon");
                return Json(new { Message = msg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region FinalSubmit
        [HttpPost]
        public ActionResult FinalSubmit(int CategoryId, int LibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "LMSSubmit");
                param1.Add("@p_CategoryId", CategoryId);
                param1.Add("@p_LibraryId", LibraryId);
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_IsAssessmentActive", 1);
                param1.Add("@p_IsFinalSubmit", 1);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_LMS_Employee_FinalSubmit", param1);
                var msg = param1.Get<string>("@p_msg");
                var Icon = param1.Get<string>("@p_Icon");
                return Json(new { Message = msg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion    

        #region Assesment
        public ActionResult Assesment(int? LibraryId, int? CategoryId)
        {
            try
            {
                if (Session["EmployeeId"] != null)
                {

                    TempData["LMSAssessmentCategoryId"] = CategoryId;
                    TempData["LMSAssessmentLibraryId"] = LibraryId;

                    var GetAssessment = DapperORM.DynamicQuerySingle($@"Select IsAssessmentSubmit from LMS_Employee_FinalSubmit WHERE CategoryId = {CategoryId}  AND LibraryId = {LibraryId}  AND EmployeeId = {Session["EmployeeId"]}");
                    TempData["IsAssessmentSubmit"] = GetAssessment != null ? GetAssessment.IsAssessmentSubmit : 0;

                    /*QUESTION LIST*/
                    param.Add("@query", $@"select Question,QuestionType as OptionType,LMSFeedbackID as QustionID from LMS_LibraryFeedbackMaster 
                                        where  Deactivate = 0  and IsActive = 1 and LMSLibraryId = {LibraryId}
                                        Union select Question, QuestionType as OptionType, LMSFeedbackID as QustionID from LMS_LibraryFeedbackMaster
                                        where Deactivate = 0  and IsActive = 1 and LMSLibraryId = {LibraryId}");
                    var Question = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                    ViewBag.GetQuestionList = Question;
                    /*OPTIONS LIST*/
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_EmployeeID", Session["EmployeeID"]);
                    param1.Add("@p_LMSCategoryId", CategoryId);
                    param1.Add("@p_LMSLibraryId", LibraryId);
                    var Options = DapperORM.ReturnList<dynamic>("sp_LMS_List_Feedback_GetQuestionList", param1).ToList();
                    ViewBag.GetOptionsList = Options;
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

        #region SaveUpdate
        public class LMS_FeedbackMaster_Answer
        {
            public double QustionId { get; set; }
            public double OptionId { get; set; }
            public string IsSelected { get; set; }
            public double OptionType { get; set; }
            public string InputRemark { get; set; }
            public string OptionName { get; set; }
            public double MasterId { get; set; }
        }

        [HttpPost]
        public ActionResult AssesmentSaveUpdate(int questionIDs, List<LMS_FeedbackMaster_Answer> questionAnswers, int QuestionType, int CategoryId, int LibraryId)
        {
            StringBuilder strBuilder = new StringBuilder();
            try
            {
                int PId = 0;
                System.Data.DataTable dt = ConvertToDataTable(questionAnswers);
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "Save");
                param1.Add("@p_LMSFeedbackId", questionIDs);
                param1.Add("@p_LMSFeedBackEmployeeId", Session["EmployeeID"]);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_LMS_Feedback_Employee_Master", param1);

                // Get PID from output parameter
                PId = int.Parse(param1.Get<string>("@p_Id"));
                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");

                DapperORM.DynamicQuerySingle("DELETE LMS_Feedback_Employee_Details FROM LMS_Feedback_Employee_Details INNER JOIN LMS_Feedback_Employee_Master ON LMS_Feedback_Employee_Details.FeedbackMasterId = LMS_Feedback_Employee_Master.FeedbackEmployeeMasterId  WHERE LMS_Feedback_Employee_Master.FeedbackEmployeeId = " + Session["EmployeeId"] + " AND LMS_Feedback_Employee_Details.QustionID = " + questionIDs + "");
                var QId = questionIDs;
                foreach (DataRow row in dt.Rows)
                {
                    // Compare QID with QuestionID from DataTable (dt)
                    if (QId.Equals(questionIDs))
                    {
                        // If they match, create the SQL Insert query using parameters
                        string Answer = "   INSERT INTO LMS_Feedback_Employee_Details (" +
                                             "Deactivate, CreatedBy, CreatedDate, MachineName,Feedback_EmployeeId, FeedbackMasterId, " +
                                             "QustionId, OptionId, QuestionType, " +
                                             "IsSelected, InputRemark, LMSCategoryId,LMSLibraryId) VALUES (" +
                                             "'0', " + "'" + Session["EmployeeName"] + "', GETDATE(), " +
                                             "'" + Dns.GetHostName() + "','" + Session["EmployeeId"] + "', " + PId + ", " + questionIDs + ", " +
                                             row["OptionId"] + ", " + QuestionType + ", '" + row["IsSelected"] + "', '" + row["InputRemark"] + "'," + CategoryId + "," + LibraryId + " )";
                        strBuilder.Append(Answer);
                    }
                }
                // Execute all queries at once if needed
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record saved successfully";
                    TempData["Icon"] = "success";
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LMS_SubCategory");
            }
        }
        #endregion

        #region ConvertToDataTable
        public static System.Data.DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
        #endregion

        #region Generate OTP And Send Mail and Verify OTP on Mail
        public static string GeneratedOTP;

        [HttpPost]
        public JsonResult GenerateAndSendEmail()
        {
            try
            {
                // Generate OTP
                //Random rand = new Random();
                //GeneratedOTP = rand.Next(100000, 999999).ToString();
                GeneratedOTP = GenerateOTPs(6);

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
                    var EmailId = Session["PersonalEmailId"] as string;
                    MailMessage mail = new MailMessage(FromEmailId, EmailId);
                    //MailMessage mail = new MailMessage(FromEmailId, "ashafak007@gmail.com");
                    mail.Subject = "LMS - OTP";
                    var GetBody = $@"
                                <div style='max-width: 500px; margin: auto; background-color: #ffffff; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                                    <p style='color: #555555; font-size: 14px; line-height: 1.6;'>
                                        Dear  {Session["EmployeeName"]},<br/>
                                        Please use the following OTP to proceed:
                                    </p>
                                    <div style='text-align: center; margin: 20px 0;'>
                                        <span style='display: inline-block; padding: 10px 20px; background-color: #eaf4ff; color: #007bff; font-size: 24px; font-weight: bold; border-radius: 6px;'>
                                            {GeneratedOTP}
                                        </span>
                                    </div>
                                    <p style='font-size: 12px; color: #888888;'>
                                        This OTP is valid for a limited time. Please do not share it with anyone.
                                    </p>
                                    <p style='font-size: 13px; color: #555555;'>
                                        Regards,<br/>
                                        LMS Team
                                    </p>
                                </div>";
                    mail.Body = GetBody;
                    mail.IsBodyHtml = true;
                    smtp.Send(mail);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        static string GenerateOTPs(int length)
        {
            Random random = new Random();
            string otp = "";
            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10); // generates a digit between 0 and 9
            }
            return otp;
        }

        [HttpPost]
        public ActionResult ValidateCode(string code)
        {
            string empId = Session["EmployeeId"]?.ToString();

            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(code))
            {
                return Json(new { status = "error", message = "Invalid input." });
            }
            
            if (code == GeneratedOTP)
            {
                return Json(new { status = "Correct", message = "Code already used. Please wait for a new code." });
            }
            else
            {
                return Json(new { status = "failed", message = "Invalid code. Try again." });
            }
        }
        #endregion

        #region AssessmentFinalSubmit
        [HttpPost]
        public ActionResult AssessmentFinalSubmit(int CategoryId, int LibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "AssessmentSubmit");
                param1.Add("@p_CategoryId", CategoryId);
                param1.Add("@p_LibraryId", LibraryId);
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_IsAssessmentFinalSubmit", 1);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_LMS_Employee_FinalSubmit", param1);
                //var data = DapperORM.ExecuteReturn("sp_LMS_Employee_AssessmentFinalSubmit", param1);
                var msg = param1.Get<string>("@p_msg");
                var Icon = param1.Get<string>("@p_Icon");
                return Json(new { Message = msg, Icon = Icon }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Employee Given Feedback
        public ActionResult saveEmployeeRatingFeedback(int rating, string feedbackText, int CategoryId, int LibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                //param1.Add("@p_process", "");
                param1.Add("@p_CategoryId", CategoryId);
                param1.Add("@p_LibraryId", LibraryId);
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                param1.Add("@p_FeedbackRating", rating);
                param1.Add("@p_FeedbackDescription", feedbackText);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500); 
                var data = DapperORM.ExecuteReturn("sp_LMS_Employee_FeedbackRating", param1);
                var msg = param1.Get<string>("@p_msg");
                var Icon = param1.Get<string>("@p_Icon");
                return Json(new { Message = msg, Icon = Icon }, JsonRequestBehavior.AllowGet);
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