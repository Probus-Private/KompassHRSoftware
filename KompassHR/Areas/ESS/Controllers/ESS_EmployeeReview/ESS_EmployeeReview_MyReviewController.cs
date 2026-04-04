using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_EmployeeReview
{
    public class ESS_EmployeeReview_MyReviewController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_EmployeeReview_MyReview
        public ActionResult ESS_EmployeeReview_MyReview()
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param = new DynamicParameters();
                param.Add("@P_ReviewEmployeeId", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Review_Employee_My", param);
                ViewBag.GetMyReviwe = data;
                return View();

            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "EmployeeReviewsList");
            }
        }
    }
}