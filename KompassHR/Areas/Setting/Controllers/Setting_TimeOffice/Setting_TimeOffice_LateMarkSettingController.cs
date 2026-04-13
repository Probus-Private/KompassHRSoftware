using Dapper;
using KompassHR.Areas.Setting.Models;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
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
using static KompassHR.Areas.Setting.Models.Setting_TimeOffice.Atten_LateMarkSetting;

namespace KompassHR.Areas.Setting.Controllers.Setting_TimeOffice
{
    public class Setting_TimeOffice_LateMarkSettingController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: Setting/Setting_TimeOffice_LateMarkSetting
        #region Setting_TimeOffice_LateMarkSetting
        public ActionResult Setting_TimeOffice_LateMarkSetting(string LateMarkSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 661;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Atten_LateMarkSetting LateMarkSetting = new Atten_LateMarkSetting();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.BusinessUnit = "";

                param = new DynamicParameters();
                if (LateMarkSettingId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_LateMarkSettingId_Encrypted", LateMarkSettingId_Encrypted);
                    LateMarkSetting = DapperORM.ReturnList<Atten_LateMarkSetting>("sp_List_LateMarkSetting", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select FromLateMarkCount,ToLateMarkCount,Day from dbo.Atten_LarkMarkSettingDetails where LateMarkSettingId=" + LateMarkSetting.LateMarkSettingId + " and Deactivate=0");
                    var GetLateMarkSettingDetails = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetLateMarkSettingDetails = GetLateMarkSettingDetails;

                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", LateMarkSetting.CmpId);
                    ViewBag.BusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();

                }
                return View(LateMarkSetting);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        [HttpGet]
        public ActionResult IsLateMarkSettingExists(string LateMarkSettingId_Encrypted, string CmpId, string LateMarkSettingBranchId, string LateMarkSettingName)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LateMarkSettingId_Encrypted", LateMarkSettingId_Encrypted);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_LateMarkSettingBranchId", LateMarkSettingBranchId);
                param.Add("@p_LateMarkSettingName", LateMarkSettingName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_LateMarkSetting", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon;
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
                return Json(new { Message = "An error occurred while checking for duplicates: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(List<LateMarkSettingDetails> Task, string LateMarkSettingId_Encrypted, string CmpId, string LateMarkSettingName, string LateMarkSettingBranchId, int LateMarkSettingId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { Message = "Session expired. Please log in again.", Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                var param = new DynamicParameters();
                StringBuilder strBuilder = new StringBuilder();

                param.Add("@p_process", string.IsNullOrEmpty(LateMarkSettingId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LateMarkSettingId_Encrypted", LateMarkSettingId_Encrypted);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_LateMarkSettingBranchId", LateMarkSettingBranchId);
                param.Add("@p_LateMarkSettingName", LateMarkSettingName);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Atten_LateMarkSetting", param);
                var PID = param.Get<string>("@p_Id");
                string Query = "";
                if (string.IsNullOrEmpty(LateMarkSettingId_Encrypted))
                {
                    DapperORM.DynamicQuerySingle("update dbo.Atten_LarkMarkSettingDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where LateMarkSettingId=" + LateMarkSettingId + "");

                    {
                        if (Task != null)
                        {
                            foreach (var detail in Task)
                            {
                                Query = $"INSERT INTO dbo.Atten_LarkMarkSettingDetails (Deactivate, CreatedBy, CreatedDate, MachineName, LateMarkSettingId, FromLateMarkCount, ToLateMarkCount, Day) " +
                                                     $"VALUES (0, '{Session["EmployeeName"]}', GETDATE(), '{Dns.GetHostName().ToString()}', {PID}, {detail.FromLateMarkCount}, {detail.ToLateMarkCount}, {detail.Day});";
                                strBuilder.Append(Query);
                            }

                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record save successfully";
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
                    DapperORM.DynamicQuerySingle("update dbo.Atten_LarkMarkSettingDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where LateMarkSettingId=" + LateMarkSettingId + "");
                    {
                        if (Task != null)
                        {
                            foreach (var detail in Task)
                            {
                                Query = $"INSERT INTO dbo.Atten_LarkMarkSettingDetails (Deactivate, CreatedBy, CreatedDate, MachineName, LateMarkSettingId, FromLateMarkCount, ToLateMarkCount, Day) " +
                                                     $"VALUES (0, '{Session["EmployeeName"]}', GETDATE(), '{Dns.GetHostName().ToString()}', {LateMarkSettingId}, {detail.FromLateMarkCount}, {detail.ToLateMarkCount}, {detail.Day});";
                                strBuilder.Append(Query);
                            }

                            string abc = "";
                            if (objcon.SaveStringBuilder(strBuilder, out abc))
                            {

                                TempData["Message"] = "Record update successfully";
                                TempData["Icon"] = "success";
                            }

                        }
                    }

                }

                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { Message = "An error occurred while saving the record: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 661;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // param.Add("@p_QuestionId_Encrypted", "List");
                param.Add("@P_LateMarkSettingId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_LateMarkSetting", param);
                ViewBag.GetLateMarkSettingList = data;
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
        public ActionResult Delete(string LateMarkSettingId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LateMarkSettingId_Encrypted", LateMarkSettingId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Atten_LateMarkSetting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TimeOffice_LateMarkSetting");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region View
        public ActionResult View(string LateMarkSettingId)
        {
            try
            {
                var details = DapperORM.DynamicQuerySingle("select FromLateMarkCount,ToLateMarkCount,Day from dbo.Atten_LarkMarkSettingDetails where LateMarkSettingId=" + LateMarkSettingId + "and Deactivate=0 ");
                return Json(details, JsonRequestBehavior.AllowGet);
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