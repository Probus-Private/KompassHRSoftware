using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_ExitFeedbackController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_FNF_ExitFeedback

        #region ExitFeedback Main View
        [HttpGet]
        public ActionResult ESS_FNF_ExitFeedback()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 170;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                
                ViewBag.AddUpdateTitle = "Add";
                FNF_FeedbackMaster FeedbackMaster = new FNF_FeedbackMaster();

                DynamicParameters param = new DynamicParameters();
                var EmployeeDepartmentID = DapperORM.DynamicQuerySingle("select EmployeeDepartmentID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");
                var EmployeeDesignationID = DapperORM.DynamicQuerySingle("select EmployeeDesignationID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");

                /*QUESTION LIST*/
                param.Add("@query", $"select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=0 and  Deactivate=0 and FeedbackFor='Employee'  Union all select Question,QuestionType as OptionType,FeedbackID as QustionID from FNF_FeedbackMaster where SpecificDepartment=1  and  Deactivate=0 and  DepartmentID =" + EmployeeDepartmentID.EmployeeDepartmentID + " and DesignationID=" + EmployeeDesignationID.EmployeeDesignationID + "");
                var Question = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.GetQuestionList = Question;
                /*OPTIONS LIST*/
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EmployeeID", Session["EmployeeID"]);
                var Options = DapperORM.ReturnList<FNF_FeedbackMaster_Answer>("sp_List_Feedback_GetQuestionList", param1).ToList();
                ViewBag.GetOptionsList = Options;

                /*QUESTION AND ANSWER CPUNT*/
                var TotalQue = DapperORM.DynamicQuerySingle("select COUNT(*) from FNF_FeedbackMaster where  DepartmentID=" + EmployeeDepartmentID.EmployeeDepartmentID + " or SpecificDepartment=0 and Deactivate=0 and FNF_FeedbackMaster.FeedbackFor='Employee' ");
                // var AtempQue = DapperORM.DynamicQuerySingle<int>("SELECT COUNT(DISTINCT QustionID) AS FeedbackCount FROM FNF_Feedback_Employee_Master WHERE FeedbackEmployeeID=" + Session["EmployeeID"] + " and Deactivate = 0").FirstOrDefault();
               // var AtempQue = DapperORM.DynamicQuerySingle("select DISTINCT QustionID from FNF_FeedbackMaster,FNF_Feedback_Employee_Master where FeedbackEmployeeID=" + Session["EmployeeID"]+ " and FNF_FeedbackMaster.FeedbackID=FNF_Feedback_Employee_Master.FeedbackEmployeeMasterID and FNF_FeedbackMaster.Deactivate=0");
                var AtempQue = DapperORM.DynamicQuerySingle("select count(QustionID) from FNF_FeedbackMaster,FNF_Feedback_Employee_Master where FeedbackEmployeeID=" + Session["EmployeeID"] + " and FNF_FeedbackMaster.FeedbackID=FNF_Feedback_Employee_Master.FeedbackEmployeeMasterID and FNF_FeedbackMaster.Deactivate=0");

                var PendingQue = TotalQue.TotalQue - AtempQue.AtempQue;
                ViewBag.TotalQue = TotalQue.AtempQue;
                ViewBag.AtempQue = AtempQue.AtempQue;
                ViewBag.PendingQue = PendingQue;
                return View(FeedbackMaster);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_ExitFeedback");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(int questionIDs, List<FNF_FeedbackMaster_Answer> questionAnswers, int QuestionType)
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
                                             "IsSelected, InputRemark) VALUES (" +
                                             "'0', " + "'" + Session["EmployeeName"] + "', GETDATE(), " +
                                             "'" + Dns.GetHostName() + "','" + Session["EmployeeID"] + "', " + PID + ", " + questionIDs + ", " +
                                             row["OptionID"] + ", " + QuestionType + ", '" + row["Selected"] + "', '" + row["InputRemark"] + "')";
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
    }
}