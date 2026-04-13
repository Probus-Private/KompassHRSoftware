using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_UserRightController : Controller
    {
        // GET: Module/Module_Employee_UserRight
        public ActionResult Module_Employee_UserRight()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param3 = new DynamicParameters();
                ViewBag.AddUpdateTitle = "Add";
                param3.Add("@query", "select UserGroupId as Id , UserGroupName as Name  from Tool_UserAccessPolicyMaster  where Deactivate=0 order by UserGroupName");
                var UserGroupPolicy = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                ViewBag.GroupName = UserGroupPolicy;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetScreenName(int EmployeeId ,  int PolicyGroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_EmployeeId", EmployeeId);
                param2.Add("@p_GroupId", PolicyGroupId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetUserRightsScreenList", param2).ToList();
                
                return Json(new { data= data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult SaveEmployeeAccess(int EmployeeId, int PolicyGroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_EmployeeId", EmployeeId);
                param2.Add("@p_GroupId", PolicyGroupId);
                var data = DapperORM.ExecuteSP<dynamic>("Pending", param2).ToList();

                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


    }
}