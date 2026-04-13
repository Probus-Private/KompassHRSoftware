using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.ESS.Models.ESS_Training;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_TrainingAssesmentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        //[HttpGet]
        #region Trainer Main View 
        public ActionResult ESS_Training_TrainingAssesment(Training_TrainingAssesmentQuestion AssesmentQuestion)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 566;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters paramtrainingCalender = new DynamicParameters();
                paramtrainingCalender.Add("@query", "select TrainingCalenderId as id,TrainingCalenderName as Name  from Training_Calender where deactivate=0 and AssesmentRequired='Yes' order by Name");
                var GetTrainingCalender = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramtrainingCalender).ToList();
                ViewBag.TrainingCalender = GetTrainingCalender;

                DynamicParameters paramlangauage = new DynamicParameters();
                paramlangauage.Add("@query", "select LangauageId as id,LangauageName as Name  from Training_AssesmentLangauage where deactivate=0  order by Name");
                var GetTrainingAssesmentLangauge = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramlangauage).ToList();
                ViewBag.TrainingAssesmentLangauge = GetTrainingAssesmentLangauge;

                int employeeId = Convert.ToInt32(Session["EmployeeId"]);

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_TrainingCalenderId", AssesmentQuestion.TrainingCalenderId);
                parameters.Add("@p_TrainingLangaugeId", AssesmentQuestion.TrainingLangaugeId);
                parameters.Add("@p_EmployeeId", employeeId);

                var Question = DapperORM.ReturnList<Training_TrainingAssesmentQuestion>("sp_GetAssesmentQuestionasPerEmployee", parameters).ToList();

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EmployeeID", employeeId);
                var Options = DapperORM.ReturnList<Training_TrainingAssesmentQuestion>("sp_List_Training_Assesment_GetQuestionList", param1).ToList();

                ViewBag.GetQuestionList = Question;
                ViewBag.GetOptionsList = Options;
                return View(AssesmentQuestion);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(int questionIDs, List<Training_TrainingAssesmentQuestion> questionAnswers, int QuestionType)
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
                param1.Add("@p_TrainingPlan_MasterId", questionIDs);
                param1.Add("@p_TrainingAssesmentEmployeeID", Session["EmployeeID"]);
                param1.Add("@p_MachineName", Dns.GetHostName().ToString());
                param1.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param1.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                param1.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);  // Output Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Training_Assesment_Employee_Master", param1);

                // Get PID from output parameter
                PID = int.Parse(param1.Get<string>("@p_Id"));
                TempData["Message"] = param1.Get<string>("@p_msg");
                TempData["Icon"] = param1.Get<string>("@p_Icon");

                DapperORM.DynamicQuerySingle("DELETE Training_AssesmentEmployeeDetails FROM Training_AssesmentEmployeeDetails  INNER JOIN Training_AssesmentEmployeeMaster ON Training_AssesmentEmployeeDetails.TrainingAssesmentEmployeeMasterID = Training_AssesmentEmployeeMaster.TrainingAssesmentEmployeeMasterID  WHERE Training_AssesmentEmployeeMaster.TrainingAssesmentEmployeeID = " + Session["EmployeeId"] + " AND Training_AssesmentEmployeeDetails.QuestionID = " + questionIDs + "");

                var QID = questionIDs;
                foreach (DataRow row in dt.Rows)
                {
                    // Compare QID with QuestionID from DataTable (dt)
                    if (QID.Equals(questionIDs))
                    {
                        // If they match, create the SQL Insert query using parameters
                        string Answer = "INSERT INTO Training_AssesmentEmployeeDetails (" +
                                             "Deactivate, CreatedBy, CreatedDate, MachineName,Assesment_EmployeeId, TrainingAssesmentEmployeeMasterID, " +
                                             "QuestionID, OptionID, QuestionType, " +
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
                return RedirectToAction(ex.Message.ToString(), "ESS_Training_TrainingAssesment");
            }
        }
        #endregion

        #region ConvertToDataTable
        public DataTable ConvertToDataTableTest(IEnumerable<Training_TrainingAssesmentQuestion> data)
        {
            var dataTable = new DataTable();

            if (data != null && data.Any())
            {
                // Get the first record to determine the columns
                var firstRecord = data.First();

                // Create columns based on the properties of Training_TrainingAssesmentQuestion
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