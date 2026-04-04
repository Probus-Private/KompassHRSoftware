using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Warning;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Warning
{
    public class ESS_Warning_MyWarningController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Warning_MyWarning
        public ActionResult ESS_Warning_MyWarning()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param = new DynamicParameters();
                param.Add("@p_EmployeerID", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Warrning", param);
                ViewBag.GetMyWarning= data;
                return View(data);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }



        #region IsValidation And IsApprovedOrRejectedCheck
        [HttpGet]
        public JsonResult IsApprovedOrRejectedCheck(string WarningEmpID_Encrypted, string Status, string Remark)
        {
            try
            {
                param.Add("@p_WarningEmpID_Encrypted", WarningEmpID_Encrypted);
                param.Add("@p_Status", Status);
                param.Add("@p_RejectRemarks", Remark);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_Approvl_Reject_Warning_Employee", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    var EmployeeId = Session["EmployeeId"];
                    param = new DynamicParameters();
                    param.Add("@p_EmployeerID", EmployeeId);
                    var data = DapperORM.DynamicList("sp_List_Warrning", param);
                    ViewBag.GetMyWarning = data;

                    return Json(new { Message, Icon , data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }
        #endregion

    }
}