using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_VerifiedController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: Module/Module_Employee_Verified
        public ActionResult Module_Employee_Verified()
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
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Preboarding_VerifiedEmployee", param).ToList();
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