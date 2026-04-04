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

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Module_Employee_PolicyInfoController : Controller
    {
        // GET: Module/Module_Employee_PolicyInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region PolicyInfo Main View
        [HttpGet]
        public ActionResult Module_Employee_PolicyInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                Mas_Employee_Policy Mas_employeePolicy = new Mas_Employee_Policy();
                var CmpId = Session["CompanyId"];
                TempData["CmpId"] = CmpId;
                //param.Add("@query", "Select  PolicyGroupMasterId, GroupName from Mas_PolicyGroup_Master where Deactivate=0");
                //var GetGroupName= DapperORM.ReturnList<Mas_PolicyGroup_Master>("sp_QueryExcution", param).ToList();
                //ViewBag.CompanyName = GetGroupName;
                return View();

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetPolicy
        [HttpGet]
        public ActionResult GetPolicy(int CmpId)
        {
            try
            {
                DynamicParameters Group = new DynamicParameters();
                Group.Add("@p_CmpID", CmpId);
                var GetList = DapperORM.ExecuteSP<dynamic>("sp_GetPolicyGroup_List", Group).ToList();

                param = new DynamicParameters();
                param.Add("@p_EmployeePolicyEmployeeID", Session["EmployeeId"]);
                var PolicyList = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Policy", param).ToList();
                //ViewBag.PolicyListBag = PolicyList;

                return Json(new { GetList, PolicyList }, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(List<Mas_Employee_Policy> RecordList)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                for (var i = 0; i < RecordList.Count; i++)
                {
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@@p_process", "Save");
                    param.Add("@p_EmployeePolicyEmployeeID", EmployeeId);
                    param.Add("@p_PolicyId", RecordList[i].PolicyId);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Policy", param);

                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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