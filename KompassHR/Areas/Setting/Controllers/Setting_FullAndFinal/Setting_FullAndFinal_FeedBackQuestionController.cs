using Dapper;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_FeedBackQuestionController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Setting/Setting_FullAndFinal_FeedBackQuestion
        #region FeedBackQuestion Main View
        [HttpGet]
        public ActionResult Setting_FullAndFinal_FeedBackQuestion(string FeedbackID_Encrypted,string Question)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 73;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // var a = DapperORM.DynamicQuerySingle("").FirstOrDefault();
                FNF_Questions fnf_Question = new FNF_Questions();
                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", "Select DesignationId as Id,DesignationName as [Name] from Mas_Designation Where Deactivate=0");
                var List_Designation = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDesignationName = List_Designation;

                param.Add("@query", "Select DepartmentId as Id,DepartmentName as [Name] from Mas_Department Where Deactivate=0");
                var List_Department = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetDepartmentName = List_Department;

                if (FeedbackID_Encrypted != null)
                {


                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_FeedbackID_Encrypted", FeedbackID_Encrypted);
                    fnf_Question = DapperORM.ReturnList<FNF_Questions>("sp_List_Feedback_Master", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS SrNo, Options as Answer from FNF_FeedbackDetails where FNF_FeedbackMaster_FeedbackID=" + fnf_Question.FeedbackID + " and Deactivate=0");
                    var List_Options = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetOptions = List_Options;

                    //DynamicParameters paramDeactivate = new DynamicParameters();
                    //paramDeactivate.Add("@query", "update FNF_FeedbackDetails set Deactivate=1 where FeedbackID=" + fnf_Question.FeedbackID + "");
                    //var Deactivate = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramDeactivate);


                    //    param.Add("@P_FeedbackID", FeedbackID);
                    //    fnf_Question = DapperORM.ReturnList<FNF_Questions>("sp_List_Feedback_Master", param).FirstOrDefault();
                }
                return View(fnf_Question);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsQuestionExists(string Question, string FeedbackID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_FeedbackID_Encrypted", FeedbackID_Encrypted);
                param.Add("@p_Question", Question);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_FeedbackMaster", param);

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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<FNF_Questions_option> Task, string Question, string FeedbackID_Encrypted, int DeptID, int DesgID, int Type,int FeedbackID,bool IsActive, bool SpecificDepartment,string FeedbackFor)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                param.Add("@p_process", string.IsNullOrEmpty(FeedbackID_Encrypted) ? "Save" : "Update");
                param.Add("@P_DepartmentID", DeptID);
                param.Add("@P_DesignationID", DesgID);
                param.Add("@P_QuestionType", Type);
                param.Add("@p_Question", Question);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_FeedbackID_Encrypted", FeedbackID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_SpecificDepartment", SpecificDepartment);
                param.Add("@p_IsActive",IsActive);
                param.Add("@p_FeedbackFor", FeedbackFor);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_FeedbackMaster", param);
                var PID = param.Get<string>("@p_Id");
               
                if (FeedbackID_Encrypted!="")
                {
                    DapperORM.DynamicQuerySingle("update FNF_FeedbackDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where FNF_FeedbackMaster_FeedbackID=" + FeedbackID + "");
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "Insert Into FNF_FeedbackDetails(" +
                                                      "   Deactivate " +
                                                      " , CreatedBy " +
                                                      " , CreatedDate " +
                                                      " , MachineName " +
                                                      " , FNF_FeedbackMaster_FeedbackID " +
                                                      " , Options " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + FeedbackID + "'," +
                                                      "'" + Data.Options + "'" +
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
                }
                else
                {
                    if (Type != 3)
                    {
                        if (Task != null)
                        {
                            foreach (var Data in Task)
                            {
                                string Answer = "Insert Into FNF_FeedbackDetails(" +
                                                      "   Deactivate " +
                                                      " , CreatedBy " +
                                                      " , CreatedDate " +
                                                      " , MachineName " +
                                                      " , FNF_FeedbackMaster_FeedbackID " +
                                                      " , Options " +
                                                       ") values (" +
                                                      "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                      "'" + PID + "'," +
                                                      "'" + Data.Options + "'" +
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
                            string Answer = "Insert Into FNF_FeedbackDetails(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , FNF_FeedbackMaster_FeedbackID " +
                                                  " , Options " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + PID + "'," +
                                                  "'Input'" +
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
                        string Answer = "Insert Into FNF_FeedbackDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , FNF_FeedbackMaster_FeedbackID " +
                                              " , Options " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'Input'" +
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

                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
               // return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Setting_FullAndFinal_FeedBackQuestion", "Setting_FullAndFinal_FeedBackQuestion");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 73;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // param.Add("@p_QuestionId_Encrypted", "List");
                param.Add("@p_FeedbackID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Feedback_Master", param);
                ViewBag.GetQuestionList = data;
                return View();


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string FeedbackID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FeedbackID_Encrypted", FeedbackID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_FeedbackMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_FeedBackQuestion");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region View
        public ActionResult View(string FeedbackID)
        { 
            try
            {
                var que=DapperORM.DynamicQuerySingle("select options from FNF_FeedbackDetails where FNF_FeedbackMaster_FeedbackID=" + FeedbackID + "and Deactivate=0 ");
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