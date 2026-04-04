using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_UnblockUserController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Module/Module_Employee_UnblockUser
        public ActionResult Module_Employee_UnblockUser()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int CmpId = Convert.ToInt32(Session["CompanyId"]);
                int BranchId = Convert.ToInt32(Session["BranchId"]);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchID", BranchId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_BlockedEmployee", param).ToList();
                ViewBag.GetFinalSubmitList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 927;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                int CmpId = Convert.ToInt32(Session["CompanyId"]);
                int BranchId = Convert.ToInt32(Session["BranchId"]);
                param.Add("@p_CmpId", CmpId);
                param.Add("@p_BranchID", BranchId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_ESS_BlockedEmployee", param).ToList();
                ViewBag.GetBlockeUserList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region SaveUpdate
        public ActionResult SaveUpdate(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramess1 = new DynamicParameters();
                paramess1.Add("@query", "UPDATE Mas_Employee_ESS SET ESSLoginAttemptCount = 0 , ESSIsLock=0 WHERE ESSEmployeeId = '" + EmployeeId + "' AND Deactivate = 0");
                var currentCountResult1 = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramess1).FirstOrDefault();
                TempData["Message"] = "Record saved successfully !";
                TempData["Icon"] = "success";
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion SaveUpdate
    }
}