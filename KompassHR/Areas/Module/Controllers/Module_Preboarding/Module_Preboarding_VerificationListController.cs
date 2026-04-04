using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Preboarding
{
    public class Module_Preboarding_VerificationListController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Module/Module_Preboarding_VerificationList
        public ActionResult Module_Preboarding_VerificationList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }             
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_FinalSubmit").ToList();
                ViewBag.GetFinalSubmitList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}