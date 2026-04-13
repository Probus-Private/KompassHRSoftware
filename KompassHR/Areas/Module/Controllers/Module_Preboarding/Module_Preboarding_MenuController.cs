using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Preboarding
{
    public class Module_Preboarding_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Module/Module_Preboarding_Menu
        #region Module_Preboarding_Menu Main View
        public ActionResult Module_Preboarding_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                //var GetCandidateVerification = DapperORM.DynamicQuerySingle("Select count(*) as CandidateVerification from Preboarding_Mas_Employee where  Verify_FinalApproval_Status='Pending' and Deactivate=0  and PreboardSubmit=1").FirstOrDefault();
                //TempData["CandidateVerification"] = GetCandidateVerification.CandidateVerification;

                //var GetPreboarding = DapperORM.DynamicQuerySingle("Select count(*) as Preboarding from Preboarding_Mas_Employee where  Verify_FinalApproval_Status='Pending' and Deactivate=0  and PreboardSubmit=1").FirstOrDefault();
                //TempData["Preboarding"] = GetPreboarding.Preboarding;

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_PreboardingDashboardCount", paramCount).ToList(); // SP_getReportingManager
                TempData["PreboardingLinkCount"] = GetCount[0].PreboardingLinkCount;
                TempData["PendingforVerification"] = GetCount[1].PreboardingLinkCount;
                TempData["ApprovedCandidate"] = GetCount[2].PreboardingLinkCount;
                TempData["OnboardCandidate"] = GetCount[3].PreboardingLinkCount;
             

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