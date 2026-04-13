using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_EmployeeReview
{
    public class ESS_EmployeeReview_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_EmployeeReview_Menu
        public ActionResult ESS_EmployeeReview_Menu(int? id, int? ScreenId)
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

                var ReviewerEmployeeId = Session["EmployeeId"];
                var data = DapperORM.DynamicQuerySingle("Select count(ReviewEmployeeId) as CountReview_Employee, replace(convert(nvarchar(12), NextReviewDate, 103), ' ', '/') as NextReviewDate from Review_Employee where ReviewerEmployeeId = " + ReviewerEmployeeId + " and Deactivate = 0 group by NextReviewDate").ToList();
                ViewBag.ReviewList = data;

                var Reviewday = DapperORM.DynamicQuerySingle("Select top 1 replace(convert(nvarchar(12), NextReviewDate, 103), ' ', '/') as NextReviewDate,DATEDIFF(DAY,(GETDATE()),( NextReviewDate)) AS RemainingDay from Review_Employee where ReviewEmployeeId= " + ReviewerEmployeeId + " and Review_Employee.Deactivate=0 and NextReviewDate>=Getdate() ");
                if (Reviewday != null)
                {
                    TempData["RemainingDay"] = Reviewday.RemainingDay;
                    TempData["NextReviewDate"] = Reviewday.NextReviewDate;
                }
                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_EmployeeReview_Menu");
            }
        }

        [HttpGet]
        public ActionResult EmployeeDetials(string NextReviewDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var parsedDate = DateTime.Parse(NextReviewDate);
                var ReviewerEmployeeId = Session["EmployeeId"];
                param.Add("@p_ReviewerEmployeeId", ReviewerEmployeeId);
                param.Add("@p_NextReviewDate", parsedDate);
                var data = DapperORM.ExecuteSP<dynamic>("sp_Review_Employee", param).ToList();
                ViewBag.EmployeeReviewList = data;
                return Json(data = ViewBag.EmployeeReviewList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_EmployeeReview_Menu");
            }

        }
    }
}