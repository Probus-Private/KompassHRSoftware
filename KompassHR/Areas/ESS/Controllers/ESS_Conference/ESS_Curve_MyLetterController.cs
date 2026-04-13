using Dapper;
using KompassHR.Models;
using KompassHR.Areas.ESS.Models.ESS_HRSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Conference
{
    public class ESS_Curve_MyLetterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Curve_MyLetter

        #region MyLetter
        public ActionResult ESS_Curve_MyLetter(string Letter_DetailId_Encrypeted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 134;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param = new DynamicParameters();
                param.Add("@p_Letter_DetailId_Encrypeted", Letter_DetailId_Encrypeted);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var HRSpace_LetterDetails = DapperORM.ReturnList<HRSpace_LetterDetails>("sp_List_HRSpace_LetterDetails", param).FirstOrDefault();
                ViewBag.Description = HRSpace_LetterDetails.Description;
                ViewBag.LetterName = HRSpace_LetterDetails.LetterName;
                return View(HRSpace_LetterDetails);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 518;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param = new DynamicParameters();
                param.Add("@p_Letter_DetailId_Encrypeted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var HRSpace_Letter = DapperORM.ReturnList<dynamic>("sp_List_HRSpace_LetterDetails", param).ToList();
                ViewBag.GetLetterList = HRSpace_Letter;
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