using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_ESSInfoController : Controller
    {
        // GET: Module/Module_Employee_ESSInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Main View
        [HttpGet]
        public ActionResult Module_Employee_ESSInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                //int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 417;
                //bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                //if (!CheckAccess)
                //{
                //    Session["AccessCheck"] = "False";
                //    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                //}
                ViewBag.AddUpdateTitle = "Add";
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                Mas_Employee_ESS Mas_employeeESS = new Mas_Employee_ESS();

                var CheckExist = DapperORM.DynamicQuerySingle(@"Select ESSEmployeeId from Mas_Employee_ESS where Deactivate=0 and ESSEmployeeId= " + Session["EmployeeId"] + "");
                TempData["CheckExist"] = CheckExist.ESSEmployeeId != null ? true : false;
                Mas_employeeESS.ESSIsActive = CheckExist != null;


                var result = DapperORM.DynamicQuerySingle(@"SELECT IsAppApplicable FROM Tool_CommonTable");
                TempData["GetIsAppApplicable"] = result.IsAppApplicable; 

                param = new DynamicParameters();
                param.Add("@p_ESSEmployeeId", Session["OnboardEmployeeId"]);
                Mas_employeeESS = DapperORM.ReturnList<Mas_Employee_ESS>("sp_List_Mas_Employee_ESS", param).FirstOrDefault();

                if (Mas_employeeESS != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }

                DynamicParameters param3 = new DynamicParameters();
                //param3.Add("@query", "select UserGroupId as Id , UserGroupName as Name  from Tool_UserAccessPolicyMaster  where Deactivate=0 order by UserGroupName");
                //param3.Add("@query", $@"Select UserGroupId Id, UserGroupName Name  from Tool_UserPolicyMapping
                //                        left Join Tool_UserAccessPolicyMaster on Tool_UserAccessPolicyMaster.UserGroupId = Tool_UserPolicyMapping.MapPolicyId
                //                        where Tool_UserAccessPolicyMaster.Deactivate = 0 And EmployeeID = {Session["EmployeeId"]}  order by UserGroupName");
                param3.Add("@p_EmployeeId", Session["EmployeeId"]);
                param3.Add("@p_OnboardEmployeeid", Session["OnboardEmployeeId"]);
                var UserGroupPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_GetUserPolicyMappedDropdown", param3).ToList();
                ViewBag.GroupName = UserGroupPolicy;


                //var DesinationName = Session["OnboardEmpDesignation"];
                //param = new DynamicParameters();
                //param.Add("@query", "select DesignationId as Id from Mas_Designation where Deactivate=0 and DesignationName='" + DesinationName + "'");
                //var GetDesignationId = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();

                param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and employeeid in (select distinct UserRightsEmployeeId from tool_userrights where deactivate = 0) and EmployeeDesignationID = '" + Session["OnboardEmpDesignationId"] + "'  and employeeid<> '" + Session["OnboardEmployeeId"] + "'");
                var GetFromEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetMasEmployee = GetFromEmployee;
                ViewBag.count = GetFromEmployee.Count();

                param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorId=1 and employeeid not in (select distinct UserRightsEmployeeId from tool_userrights where deactivate = 0 and UserRightsEmployeeId ='" + Session["OnboardEmployeeId"] + "') and EmployeeId ='" + Session["OnboardEmployeeId"] + "'");
                var GetToEmployee = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).FirstOrDefault();
                if (GetToEmployee != null)
                {
                    var Name = GetToEmployee.Name;
                    ViewBag.GetToEmployee = Name;
                }
                else
                {
                    ViewBag.GetToEmployee = null;
                }
                if (GetToEmployee != null || GetToEmployee != "")
                {
                    ViewBag.countToUser = 1;
                }
                else
                {
                    ViewBag.countToUser = 0;
                }

                return View(Mas_employeeESS);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVerification
        [HttpGet]
        public ActionResult IsESSLoginExists(string ESSId_Encrypted, string ESSLoginID, int PolicyId)
        {
            try
            {
                var EmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", "IsValidation");
                param.Add("@p_ESSId_Encrypted", ESSId_Encrypted);
                param.Add("@p_ESSLoginID", ESSLoginID);
                param.Add("@p_ESSEmployeeId", EmployeeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_UserAccessPolicyId", PolicyId);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_ESS", param);
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
        public ActionResult SaveUpdate(Mas_Employee_ESS EmployeeESS)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeESS.ESSId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ESSId", EmployeeESS.ESSId);
                param.Add("@p_ESSId_Encrypted", EmployeeESS.ESSId_Encrypted);
                param.Add("@p_ESSEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_ESSLoginID", EmployeeESS.ESSLoginID);
                param.Add("@p_ESSPassword", EmployeeESS.ESSPassword);
                param.Add("@p_ESSSecurityQuestion", EmployeeESS.ESSSecurityQuestion);
                param.Add("@p_ESSAnswer", EmployeeESS.ESSAnswer);
                //param.Add("@p_IsAdmin", EmployeeESS.IsAdmin);
                param.Add("@p_ESSIsActive", EmployeeESS.ESSIsActive);
                param.Add("@p_ESSIsLock", EmployeeESS.ESSIsLock);
                param.Add("@p_UserAccessPolicyId", EmployeeESS.UserAccessPolicyId);
                param.Add("@p_ESSLoginAttemptCount", EmployeeESS.ESSLoginAttemptCount);
                param.Add("@p_IsExit", EmployeeESS.IsExit);
                param.Add("@p_IsApp", EmployeeESS.IsApp);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_ESS]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_ESSInfo", "Module_Employee_ESSInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveReplicationUser
        public ActionResult SaveReplicationUser(int FromUser, string ToUser)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramGet = new DynamicParameters();
                paramGet.Add("@query", "select employeeid as Id,EmployeeName as Name from maS_employee where deactivate=0 and employeeid not in (select distinct UserRightsEmployeeId from tool_userrights where deactivate = 0 and UserRightsEmployeeId ='" + Session["OnboardEmployeeId"] + "') and EmployeeId ='" + Session["OnboardEmployeeId"] + "'");
                var ToUserId = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramGet).FirstOrDefault();
                param.Add("@p_process", "Save");
                //param.Add("@p_UserRightsID_Encrypted", UserRightsID_Encrypted);
                param.Add("@p_FromEmployeeid", FromUser);
                param.Add("@p_ToEmployeeid", ToUserId.Id);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_UserRightsCopy", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Module_Employee_ESSInfo", "Module_Employee_ESSInfo");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetUserAccessPolicyHistory
        public ActionResult GetUserAccessPolicyHistory(int UserPolicyAccessId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param = new DynamicParameters();
                param.Add("@p_UserGroupId", UserPolicyAccessId);
                var data = DapperORM.ReturnList<UserAccessPolicyHistory>("sp_Access_PolicyList", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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