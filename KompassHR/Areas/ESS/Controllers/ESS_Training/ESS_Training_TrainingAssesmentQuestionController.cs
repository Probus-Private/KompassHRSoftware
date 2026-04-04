using Dapper;
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
    public class ESS_Training_TrainingAssesmentQuestionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        [HttpGet]
        #region Trainer Main View 
        public ActionResult ESS_Training_TrainingAssesmentQuestion(string Training_AssesmentQuestionId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 565;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Training_TrainingAssesmentQuestion AssesmentQuestion = new Training_TrainingAssesmentQuestion();
                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters paramtrainingCalender = new DynamicParameters();
                paramtrainingCalender.Add("@query", "select TrainingCalenderId as id,TrainingCalenderName as Name  from Training_Calender where deactivate=0 and AssesmentRequired='Yes' order by Name");
                var GetTrainingCalender = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramtrainingCalender).ToList();
                ViewBag.TrainingCalender = GetTrainingCalender;

                DynamicParameters paramlangauage = new DynamicParameters();
                paramlangauage.Add("@query", "select LangauageId as id,LangauageName as Name  from Training_AssesmentLangauage where deactivate=0  order by Name");
                var GetTrainingAssesmentLangauge = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramlangauage).ToList();
                ViewBag.TrainingAssesmentLangauge = GetTrainingAssesmentLangauge;

                if (Training_AssesmentQuestionId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@P_Training_AssesmentQuestionId_Encrypted", Training_AssesmentQuestionId_Encrypted);
                    AssesmentQuestion = DapperORM.ReturnList<Training_TrainingAssesmentQuestion>("sp_List_TrainingAssesmentQuestion", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS SrNo, Options as Answer,IsAnswer,InputAnswer from Training_AssesmentQuestionDetails where Training_AssesmentQuestionId=" + AssesmentQuestion.Training_AssesmentQuestionId + " and Deactivate=0");
                    var List_Options = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetOptions = List_Options;
                    if (List_Options.Any())
                    {
                        AssesmentQuestion.InputAnswer = List_Options[0].InputAnswer;
                    }
                }

                return View(AssesmentQuestion);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 565;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_Training_AssesmentQuestionId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingAssesmentQuestion", param).ToList();
                ViewBag.GetTrainingAssesmentQuesion = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region IsTrainingAssementQuestionExists
        public ActionResult IsTrainingAssementQuestionExists(string TrainingCalenderId, string TrainingLangaugeId, string TrainingAssesmentQuestionId_Encrypted, string Question)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@P_Training_AssesmentQuestionId_Encrypted", TrainingAssesmentQuestionId_Encrypted);
                    param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                    param.Add("@p_TrainingLangaugeId", TrainingLangaugeId);
                    param.Add("@p_Question", Question);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingAssesmentQuestion", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
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
        public ActionResult SaveUpdate(List<Training_TrainingAssesmentQuestion_option> Task, string Question, string InputAnswer, int TrainingCalenderId, int TrainingLangaugeId, int Type, string Training_AssesmentQuestionId_Encrypted, int Training_AssesmentQuestionId, bool IsActive)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var Logo = Request.Files["Logo"];
                var Stamp = Request.Files["Stamp"];
                param.Add("@p_process", string.IsNullOrEmpty(Training_AssesmentQuestionId_Encrypted) ? "Save" : "Update");
                param.Add("@P_Training_AssesmentQuestionId_Encrypted", Training_AssesmentQuestionId_Encrypted);
                // param.Add("@p_Training_AssesmentQuestionId", Training_AssesmentQuestionId);
                param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                param.Add("@p_TrainingLangaugeId", TrainingLangaugeId);
                param.Add("@p_QuestionType", Type);
                param.Add("@p_Question", Question);
                param.Add("@p_IsActive", IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter

                var data = DapperORM.ExecuteReturn("sp_SUD_TrainingAssesmentQuestion", param);
                var PID = param.Get<string>("@p_Id");

                if (Training_AssesmentQuestionId_Encrypted != "")
                {
                    DapperORM.DynamicQuerySingle("update Training_AssesmentQuestionDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where Training_AssesmentQuestionId=" + Training_AssesmentQuestionId + "");
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "Insert Into Training_AssesmentQuestionDetails(" +
                                                      "   Deactivate " +
                                                      " , CreatedBy " +
                                                      " , CreatedDate " +
                                                      " , MachineName " +
                                                      " , Training_AssesmentQuestionId " +
                                                      " , Options " +
                                                       " , IsAnswer " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + PID + "'," +
                                                        "N'" + Data.Options + "'," +
                                                       "'" + Data.IsAnswer + "'" +
                                                      ")" +
                                                       " " +
                                                      " " +
                                                       " ";
                                strBuilder.Append(Answer);

                            }
                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }

                    }

                    else
                    {
                        string Answer = "Insert Into Training_AssesmentQuestionDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , Training_AssesmentQuestionId " +
                                              " , Options " +
                                              " , IsAnswer " +
                                              " , InputAnswer " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + Training_AssesmentQuestionId + "'," +
                                              "'Input'," +
                                              "''," +
                                              "N'" + InputAnswer + "'" +
                                              ")" +
                                               " " +
                                              " " +
                                               " "; 
                      strBuilder.Append(Answer);

                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            TempData["Message"] = "Record update successfully";
                            TempData["Icon"] = "success";
                        }

                        if (abc != "")
                        {
                            DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                "   Error_Desc " +
                                                                                " , Error_FormName " +
                                                                                " , Error_MachinceName " +
                                                                                " , Error_Date " +
                                                                                " , Error_UserID " +
                                                                                " , Error_UserName " + ") values (" +
                                                                                "'" + strBuilder + "'," +
                                                                                "'BuklInsert'," +
                                                                                "'" + Dns.GetHostName().ToString() + "'," +
                                                                                "GetDate()," +
                                                                                "'" + Session["EmployeeId"] + "'," +
                                                                                "'" + Session["EmployeeName"] + "'");
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    }
                }
                else
                {
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "INSERT INTO Training_AssesmentQuestionDetails (" +
                                 "   Deactivate, " +
                                 "   CreatedBy, " +
                                 "   CreatedDate, " +
                                 "   MachineName, " +
                                 "   Training_AssesmentQuestionId, " +
                                 "  Options " +
                                 " , IsAnswer " +
                                 ") values (" +
                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                  "'" + PID + "'," +
                                 "N'" + Data.Options + "'," +
                                  "'" + Data.IsAnswer + "'" +
                                  ")" +
                                  " " +
                                 " " +
                                 " ";
                                strBuilder.Append(Answer);
                            }
                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }
                        else
                        {
                            string Answer = "Insert Into Training_AssesmentQuestionDetails(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , Training_AssesmentQuestionId " +
                                                   " , Options " +
                                                      " , IsAnswer " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + Training_AssesmentQuestionId + "'," +
                                                      "'Input'," +
                                                      "''" +
                                                      ")" +
                                                       " " +
                                                      " " +
                                                       " ";
                            strBuilder.Append(Answer);

                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                            if (abc != "")
                            {
                                DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                    "   Error_Desc " +
                                                                                    " , Error_FormName " +
                                                                                    " , Error_MachinceName " +
                                                                                    " , Error_Date " +
                                                                                    " , Error_UserID " +
                                                                                    " , Error_UserName " + ") values (" +
                                                                                    "'" + strBuilder + "'," +
                                                                                    "'BuklInsert'," +
                                                                                    "'" + Dns.GetHostName().ToString() + "'," +
                                                                                    "GetDate()," +
                                                                                    "'" + Session["EmployeeId"] + "'," +
                                                                                    "'" + Session["EmployeeName"] + "'");
                                TempData["Message"] = abc;
                                TempData["Icon"] = "error";
                            }
                        }
                    }
                    else
                    {
                        string Answer = "Insert Into Training_AssesmentQuestionDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , Training_AssesmentQuestionId " +
                                              " , Options " +
                                                  " , IsAnswer " +
                                                   " , InputAnswer " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + PID + "'," +
                                                  "'Input'," +
                                                  "''," +
                                                "N'" + InputAnswer + "'" +
                                                  ")" +
                                                   " " +
                                                  " " +
                                                   " ";
                        strBuilder.Append(Answer);

                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {

                            TempData["Message"] = "Record update successfully";
                            TempData["Icon"] = "success";
                        }

                        if (abc != "")
                        {
                            DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                                "   Error_Desc " +
                                                                                " , Error_FormName " +
                                                                                " , Error_MachinceName " +
                                                                                " , Error_Date " +
                                                                                " , Error_UserID " +
                                                                                " , Error_UserName " + ") values (" +
                                                                                "'" + strBuilder + "'," +
                                                                                "'BuklInsert'," +
                                                                                "'" + Dns.GetHostName().ToString() + "'," +
                                                                                "GetDate()," +
                                                                                "'" + Session["EmployeeId"] + "'," +
                                                                                "'" + Session["EmployeeName"] + "'");
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    }
                }

                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(double? Training_AssesmentQuestionId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Training_AssesmentQuestionId", Training_AssesmentQuestionId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TrainingAssesmentQuestion", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("GetList", "ESS_Training_TrainingAssesmentQuestion");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region View
        public ActionResult View(string Training_AssesmentQuestionId)
        {
            try
            {
                var ques = DapperORM.DynamicQuerySingle("select options from Training_AssesmentQuestionDetails where Training_AssesmentQuestionId=" + Training_AssesmentQuestionId + "and Deactivate=0 ");
                var que = ques != null ? ques.options : null;
                return Json(que, JsonRequestBehavior.AllowGet);
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