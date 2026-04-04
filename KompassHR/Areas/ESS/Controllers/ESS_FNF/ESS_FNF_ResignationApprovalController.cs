using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_ResignationApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: ESS/ESS_FNF_ResignationApproval
        #region ESS_FNF_ResignationApproval
        public ActionResult ESS_FNF_ResignationApproval()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 171;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetApproverLevel = "Select distinct ApproverLevel from Tra_Approval where Deactivate=0 and TraApproval_ModuleId=5 and Origin='FNF_EmployeeResignation' and  TraApproval_ApproverEmployeeId='" + Session["EmployeeId"] + "'";
                var ApproverLevel = DapperORM.DynamicQuerySingle(GetApproverLevel);
                if(ApproverLevel !=null)
                {
                    ViewBag.ApproverLevel = ApproverLevel.ApproverLevel;
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and TraApproval_ApproverEmployeeId = '" + Session["EmployeeId"] + "'");
                param.Add("@P_ApproverEmployeeID", Session["EmployeeId"]);
                param.Add("@P_Process", "Pending");
                var data = DapperORM.DynamicList("sp_FNF_Acknowledgement_Pending", param);
                ViewBag.GetFNFAcknowledgementPendingList = data;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@P_ApproverEmployeeID", Session["EmployeeId"]);
                var FNFApprovalPendingList = DapperORM.DynamicList("sp_FNF_Approval_Pending", param1);
                ViewBag.GetFNFApprovalPendingList = FNFApprovalPendingList;

                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                var info = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeDetails", param2).FirstOrDefault();
                ViewBag.EmployeeDetails = info;

                DynamicParameters paramlist = new DynamicParameters();
                paramlist.Add("@P_ApproverEmployeeID", Session["EmployeeId"]);
                var ApproveRejectList = DapperORM.ReturnList<dynamic>("sp_FNF_ApproveRejectList", paramlist).ToList();
                ViewBag.ResignationApproveRejectList = ApproveRejectList;

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_ResignationApproval");
            }

        }
        #endregion

        #region SaveUpdate Ashafak
        [HttpPost]
        public ActionResult SaveUpdate(int questionIDs, List<FNF_FeedbackMaster_Answer> questionAnswers, int QuestionType, int EmployeeId)
        {
            StringBuilder strBuilder = new StringBuilder();
            try
            {
                int PID = 0;
                DataTable dt = new DataTable();
                //StringBuilder strBuilder = new StringBuilder();
                // DapperORM dprObj = new DapperORM();
                dt = ConvertToDataTableTest(questionAnswers);
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_process", "Save");
                param1.Add("@p_FeedbackID", questionIDs);
                param1.Add("@p_FeedBackEmployeeID", Session["EmployeeID"]);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_Feedback_Employee_Master", param1);

                // Get PID from output parameter
                PID = int.Parse(param1.Get<string>("@p_Id"));
                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");

                DapperORM.DynamicQuerySingle("DELETE FNF_Feedback_Employee_Details FROM FNF_Feedback_Employee_Details INNER JOIN FNF_Feedback_Employee_Master ON FNF_Feedback_Employee_Details.FeedbackMasterID = FNF_Feedback_Employee_Master.FeedbackEmployeeMasterID  WHERE FNF_Feedback_Employee_Master.FeedbackEmployeeID = " + Session["EmployeeId"] + " AND FNF_Feedback_Employee_Details.QustionID = " + questionIDs + "");
                var QID = questionIDs;
                foreach (DataRow row in dt.Rows)
                {
                    // Compare QID with QuestionID from DataTable (dt)
                    if (QID.Equals(questionIDs))
                    {
                        // If they match, create the SQL Insert query using parameters
                        string Answer = "   INSERT INTO FNF_Feedback_Employee_Details (" +
                                             "Deactivate, CreatedBy, CreatedDate, MachineName,Feedback_EmployeeId, FeedbackMasterID, " +
                                             "QustionID, OptionID, QuestionType, " +
                                             "IsSelected, InputRemark,Feedback_For_EmployeeId) VALUES (" +
                                             "'0', " + "'" + Session["EmployeeName"] + "', GETDATE(), " +
                                             "'" + Dns.GetHostName() + "','" + Session["EmployeeID"] + "', " + PID + ", " + questionIDs + ", " +
                                             row["OptionID"] + ", " + QuestionType + ", '" + row["Selected"] + "', '" + row["InputRemark"] + "'," + EmployeeId + ")";
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
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_ExitFeedback");
            }
        }
        #endregion

        #region ViewForRequestApprover
        public ActionResult ViewForRequestApprover(int ResignId, string NoticePeriod, DateTime NoticePeriodEndDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@P_Qry", "and FNFId ='" + ResignId + "'");
                var data = DapperORM.ReturnList<dynamic>("sp_List_FNF_EmployeeResignation", param).FirstOrDefault();
                if (data.RelievingDate != null)
                {
                    TempData["LastWorkingDate"] = Convert.ToDateTime(data.RelievingDate).ToString("yyyy-MM-dd");

                }
                else
                {
                    TempData["LastWorkingDate"] = Convert.ToDateTime(data.LastWorkingDate).ToString("yyyy-MM-dd");
                }
                TempData["ResignationDate"] = Convert.ToDateTime(data.ResignationDate).ToString("yyyy-MM-dd");

                ViewBag.GetData = data;
                ViewBag.GetNoticePeriod = NoticePeriod;

                FNF_EmployeeResignation model = new FNF_EmployeeResignation();
                model.FnfEmployeeId = Convert.ToDouble(data.FnfEmployeeId);
                model.FnfId = ResignId;
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_ResignationApproval");
            }
        }

        #endregion

        #region ForApprovalCandidate
        [HttpPost]
        public ActionResult ForApprovalCandidate(FNF_EmployeeResignation ApprovalquestionAnswers, int FnfID, string FNFEncrypted, string Status, string Remark)
        {
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "Resignation");
                paramApprove.Add("@p_DocId_Encrypted", FNFEncrypted);
                paramApprove.Add("@p_DocId", FnfID);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", Status);
                paramApprove.Add("@p_ApproveRejectRemark", Remark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_Icon");
                //   TempData["Message1"] = Message;
                //   TempData["Icon"] = Icon.ToString();
                if (Message != "")
                {
                    DynamicParameters paramQueAns = new DynamicParameters();
                    paramQueAns.Add("@p_process", "Save");
                    paramQueAns.Add("@P_FNFEmployeeId", ApprovalquestionAnswers.FnfEmployeeId);
                    paramQueAns.Add("@P_LastWorkingDate", ApprovalquestionAnswers.LastWorkingDate);
                    paramQueAns.Add("@P_AnsweredByApprovalId", Session["EmployeeId"]);
                    paramQueAns.Add("@P_IsReasonCorrect", ApprovalquestionAnswers.IsReasonCorrect);
                    paramQueAns.Add("@P_IsReasonCorrectRemark", ApprovalquestionAnswers.IsReasonCorrectRemark);
                    paramQueAns.Add("@P_IsCriticalResource", ApprovalquestionAnswers.IsCriticalResource);
                    paramQueAns.Add("@P_IsCriticalResourceRemark", ApprovalquestionAnswers.IsCriticalResourceRemark);
                    paramQueAns.Add("@P_IsHireagain", ApprovalquestionAnswers.IsHireagain);
                    paramQueAns.Add("@P_IsHireagainRemark", ApprovalquestionAnswers.IsHireagainRemark);
                    paramQueAns.Add("@P_IsImmediateRelease", ApprovalquestionAnswers.IsImmediateRelease);
                    paramQueAns.Add("@P_IsImmediateReleaseRemark", ApprovalquestionAnswers.IsImmediateReleaseRemark);
                    paramQueAns.Add("@P_IsVoluntary", ApprovalquestionAnswers.IsVoluntary);
                    paramQueAns.Add("@P_IsVoluntaryRemark", ApprovalquestionAnswers.VoluntaryRemark);
                    paramQueAns.Add("@P_TaskPending", ApprovalquestionAnswers.TaskPending);
                    paramQueAns.Add("@P_TaskPendingRemark", ApprovalquestionAnswers.TaskPendingRemark);
                    paramQueAns.Add("@P_KPIPending", ApprovalquestionAnswers.KPIPending);
                    paramQueAns.Add("@P_KPIPendingRemark", ApprovalquestionAnswers.KPIPendingRemark);
                    paramQueAns.Add("@P_EmployerNextCompany", ApprovalquestionAnswers.EmployerNextCompany);
                    paramQueAns.Add("@P_ServiceBond", ApprovalquestionAnswers.ServiceBond);
                    paramQueAns.Add("@P_ServiceBondRemark", ApprovalquestionAnswers.ServiceBondRemark);
                    paramQueAns.Add("@P_ManagerRemark", Remark);
                    paramQueAns.Add("@P_ResignationId", ApprovalquestionAnswers.FnfId);
                    paramQueAns.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    paramQueAns.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var Result = DapperORM.ExecuteSP<dynamic>("sp_SUD_FNF_FNFApprovalQuestionAnswer", paramQueAns);
                    var Message1 = paramQueAns.Get<string>("@p_msg");

                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Recruitment_ResourcesRequisition");
            }

        }
        #endregion

        #region ESS_FNF_ExitFeedback
        [HttpGet]
        public ActionResult ESS_FNF_ExitFeedbackManager(string FnfId_Encrypted, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 171;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                FNF_FeedbackMaster FeedbackMaster = new FNF_FeedbackMaster();

                Session["EmployeeIDEncrypted"] = false;

                DynamicParameters EmpInfo = new DynamicParameters();
                EmpInfo.Add("@query", "select EmployeeId, EmployeeName,EmployeeNo,JoiningDate,Mas_Department.DepartmentName,Mas_Designation.DesignationName from Mas_Employee,Mas_Department,Mas_Designation where Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId and Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId and EmployeeId=" + EmployeeId + "");
                var Employee = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", EmpInfo).FirstOrDefault();
                ViewBag.GetEmployeeInfo = Employee;
                var EmpId = Employee.EmployeeId;

                DynamicParameters param = new DynamicParameters();
                var EmployeeDepartmentID = DapperORM.DynamicQuerySingle("select EmployeeDepartmentID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");
                var EmployeeDesignationID = DapperORM.DynamicQuerySingle("select EmployeeDesignationID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");
                var DepartmentId = EmployeeDepartmentID.EmployeeDepartmentID;
                var DesignationId = EmployeeDesignationID.EmployeeDesignationID;
                //param.Add("@query", "select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=0  and  Deactivate=0 Union all select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=1  and  Deactivate=0 and  DepartmentID =" + EmployeeDepartmentID + " and DesignationID=" + EmployeeDesignationID + " ");
                //var Question = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                //ViewBag.GetQuestionList = Question;

                //param.Add("@query", "select options,FNF_FeedbackMaster_FeedbackID,FeedDetailsID from fnf_feedbackdetails where Deactivate=0 ");
                DynamicParameters param1 = new DynamicParameters();
                var a = DapperORM.DynamicQuerySingle("SELECT COUNT(*) AS ManagerCount FROM Mas_Designation  WHERE DesignationName LIKE CONCAT('%',(SELECT DesignationName FROM Mas_Designation WHERE DesignationId = ( SELECT EmployeeDesignationID FROM Mas_Employee  WHERE EmployeeID = " + Session["EmployeeID"] + ")),  '%');");
                var b = a.ManagerCount;
                if (a.ManagerCount > 0)
                {
                    param1.Add("@p_Process", "Manager");
                    param.Add("@query", "select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=0   and  FNF_FeedbackMaster.FeedbackFor='Manager'  and   Deactivate=0 Union all select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=1 and  FNF_FeedbackMaster.FeedbackFor='Manager'  and  Deactivate=0 and  DepartmentID =" + DepartmentId + " and DesignationID=" + DesignationId + " ");
                    var Question = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                    ViewBag.GetQuestionList = Question;
                    var FeedbackID = DapperORM.DynamicQuerySingle("select FeedbackID from FNF_FeedbackMaster where FeedbackFor='Manager'");

                    var TotalQue = DapperORM.DynamicQuerySingle("select COUNT(*) from FNF_FeedbackMaster where  DepartmentID=" + DepartmentId + " or SpecificDepartment=0 and Deactivate=0 and FeedbackFor='Manager'");
                    //var AtempQue = DapperORM.DynamicQuerySingle<int>("SELECT COUNT(DISTINCT QustionID) AS FeedbackCount FROM FNF_Feedback_Employee_Master WHERE FeedbackEmployeeID=" + Session["EmployeeID"] + " and  QustionID=" + FeedbackID + " and FNF_Feedback_Employee_Master.Deactivate = 0 ").FirstOrDefault();
                    var AtempQue = DapperORM.DynamicQuerySingle("select count(QustionID) from FNF_FeedbackMaster,FNF_Feedback_Employee_Master where FeedbackEmployeeID=" + Session["EmployeeID"] + " and FNF_FeedbackMaster.FeedbackID=FNF_Feedback_Employee_Master.FeedbackEmployeeMasterID and FNF_FeedbackMaster.Deactivate=0");

                    //param.Add("@query", "SELECT COUNT(DISTINCT QustionID) AS FeedbackCount FROM FNF_Feedback_Employee_Master WHERE FeedbackEmployeeID=" + Session["EmployeeID"] + " and  QustionID=" + FeedbackID + " and FNF_Feedback_Employee_Master.Deactivate = 0 ");
                    //var AtempQue = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                    var PendingQue = TotalQue.TotalQue - AtempQue.AtempQue;
                    ViewBag.TotalQue = TotalQue.AtempQue;
                    ViewBag.AtempQue = AtempQue.AtempQue;
                    ViewBag.PendingQue = PendingQue;
                }
                else
                {
                    param1.Add("@p_Process", "");
                    param.Add("@query", "select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=0 FNF_FeedbackMaster.FeedbackFor='Employee' and  Deactivate=0 Union all select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=1  and  Deactivate=0 and  DepartmentID =" + DepartmentId + " and DesignationID=" + DesignationId + " ");
                    var Question = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                    ViewBag.GetQuestionList = Question;

                    var TotalQue = DapperORM.DynamicQuerySingle("select COUNT(*) from FNF_FeedbackMaster where  DepartmentID=" + DepartmentId + " or SpecificDepartment=0 and Deactivate=0 and FeedbackFor='Employee'");
                    var AtempQue = DapperORM.DynamicQuerySingle("SELECT COUNT(DISTINCT QustionID) AS FeedbackCount FROM FNF_Feedback_Employee_Master WHERE FeedbackEmployeeID=" + Session["EmployeeID"] + " and Deactivate = 0");
                    var PendingQue = TotalQue - AtempQue;
                    ViewBag.TotalQue = TotalQue;
                    ViewBag.AtempQue = AtempQue;
                    ViewBag.PendingQue = PendingQue;

                }
                param1.Add("@p_EmployeeID", Session["EmployeeID"]);

                var Options = DapperORM.ReturnList<FNF_FeedbackMaster_Answer>("sp_List_Feedback_GetQuestionList", param1).ToList();
                ViewBag.GetOptionsList = Options;

                return View(FeedbackMaster);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_ExitFeedback");
            }
        }

        #endregion


        #region ConvertToDataTable
        public DataTable ConvertToDataTableTest(IEnumerable<FNF_FeedbackMaster_Answer> data)
        {
            var dataTable = new DataTable();

            if (data != null && data.Any())
            {
                // Get the first record to determine the columns
                var firstRecord = data.First();

                // Create columns based on the properties of FNF_FeedbackMaster_Answer
                dataTable.Columns.Add("QuestionID", typeof(double));
                dataTable.Columns.Add("OptionID", typeof(double));
                dataTable.Columns.Add("Selected", typeof(string));
                dataTable.Columns.Add("QuestionType", typeof(double));
                dataTable.Columns.Add("InputRemark", typeof(string));
                dataTable.Columns.Add("MasterID", typeof(double));

                // Populate rows with the rest of the records
                foreach (var record in data)
                {
                    var dataRow = dataTable.NewRow();

                    // Assign values to the DataRow, ensuring DBNull.Value is used for null string values
                    dataRow["QuestionID"] = record.QustionID;
                    dataRow["OptionID"] = record.OptionId;

                    // If 'Selected' is null, assign DBNull.Value, else use the string value
                    dataRow["Selected"] = string.IsNullOrEmpty(record.IsSelected) ? DBNull.Value : (object)record.IsSelected;

                    dataRow["QuestionType"] = record.OptionType;

                    // If 'InputRemark' is null, assign DBNull.Value, else use the string value
                    dataRow["InputRemark"] = string.IsNullOrEmpty(record.InputRemark) ? DBNull.Value : (object)record.InputRemark;
                    dataRow["MasterID"] = record.MasterID;

                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }
        #endregion

        public ActionResult SheduleAcknowledge(int EmployeeId, int ResignId, DateTime Date, TimeSpan Time, string Remark)
        {
            param.Add("@p_process", "Save");
            param.Add("@p_EmployeeId", EmployeeId);
            param.Add("@p_ResignId", ResignId);
            param.Add("@p_AcknowledgedByEmployeeId", Session["EmployeeID"]);
            param.Add("@p_AcknowledgedDate", Date);
            param.Add("@p_Time", Time);
            param.Add("@p_Remark", Remark);
            param.Add("@p_MachineName", Dns.GetHostName().ToString());
            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
            var Result = DapperORM.ExecuteReturn("sp_SUD_FnfAcknowledgement", param);
            TempData["Message"] = param.Get<string>("@p_msg");
            TempData["Icon"] = param.Get<string>("@p_Icon");
            return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
        }
        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
                if (string.IsNullOrEmpty(DownloadAttachment))
                {
                    TempData["Message"] = "Invalid File.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_FNF_ResignationApproval", "ESS_FNF_ResignationApproval");
                }

                if (DownloadAttachment == null)
                {
                    TempData["Message"] = "File path information not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_FNF_ResignationApproval", "ESS_FNF_ResignationApproval");
                }

                var driveLetter = Path.GetPathRoot(DownloadAttachment);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_FNF_ResignationApproval", "ESS_FNF_ResignationApproval");
                }
                // Construct full file path
                var fullPath = DownloadAttachment;

                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on your server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("ESS_FNF_ResignationApproval", "ESS_FNF_ResignationApproval");
                }
                // Return the file for download
                var fileName = Path.GetFileName(fullPath);
                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region AcknowledgeGetList
        [HttpGet]
        public ActionResult AcknowledgeGetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 171;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and TraApproval_ApproverEmployeeId = '" + Session["EmployeeId"] + "'");
                param.Add("@P_ApproverEmployeeID", Session["EmployeeId"]);
                param.Add("@P_Process", "Complete");
                var data = DapperORM.DynamicList("sp_FNF_Acknowledgement_Pending", param);
                ViewBag.GetFNFAcknowledgementCompleteList = data;
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