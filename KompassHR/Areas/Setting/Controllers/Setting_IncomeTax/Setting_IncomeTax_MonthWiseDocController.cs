using Dapper;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_MonthWiseDocController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: IncomeTaxSetting/MonthWiseDoc
        [HttpGet]
        public ActionResult Setting_IncomeTax_MonthWiseDoc(int? OpenCloseTypeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 76;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "Select  DocumentId as Id, DocumentName as Name from IncomeTax_Document where Deactivate=0");
                var GetDocumentList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.DocumentList = GetDocumentList;

                IncomeTax_DocumentOpenClose DocumentOpenClose = new IncomeTax_DocumentOpenClose();

                if (OpenCloseTypeID != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_OpenCloseTypeID", OpenCloseTypeID);
                    //param1.Add("@P_DocumentOpenCloseId_Encrypted", DocumentOpenCloseId_Encrypted);

                    DocumentOpenClose = DapperORM.ReturnList<IncomeTax_DocumentOpenClose>("sp_List_IncomeTax_DocumentOpenClose", param1).FirstOrDefault();


                   // ViewBag.DocSettingList1 = data1;
                   // return View(DocumentOpenClose);
                }
                return View(DocumentOpenClose);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(IncomeTax_DocumentOpenClose Doc)
        {
            try
            {
                //param.Add("@p_process", string.IsNullOrEmpty(Doc.DocumentOpenCloseId_Encrypted) ? "Save" : "Update");
                //param.Add("@P_DocumentOpenCloseId_Encrypted", Doc.DocumentOpenCloseId_Encrypted);
                param.Add("@P_DocumentOpenCloseId", Doc.DocumentOpenCloseId);
                param.Add("@P_OpenCloseTypeID", Doc.OpenCloseTypeID);

                param.Add("@P_AprMonth", Doc.AprMonth);
                param.Add("@P_AprFromDay", Doc.AprFromDay);
                param.Add("@P_AprToDay", Doc.AprToDay);

                param.Add("@P_MayMonth", Doc.MayMonth);
                param.Add("@P_MayFromDay", Doc.MayFromDay);
                param.Add("@P_MayToDay", Doc.MayToDay);

                param.Add("@P_JunMonth", Doc.JunMonth);
                param.Add("@P_JunFromDay", Doc.JunFromDay);
                param.Add("@P_JunToDay", Doc.JunToDay);

                param.Add("@P_JulMonth", Doc.JulMonth);
                param.Add("@P_JulFromDay", Doc.JulFromDay);
                param.Add("@P_JulToDay", Doc.JulToDay);

                param.Add("@P_AugMonth", Doc.AugMonth);
                param.Add("@P_AugFromDay", Doc.AugFromDay);
                param.Add("@P_AugToDay", Doc.AugToDay);

                param.Add("@P_SepMonth", Doc.SepMonth);
                param.Add("@P_SepFromDay", Doc.SepFromDay);
                param.Add("@P_SepToDay", Doc.SepToDay);

                param.Add("@P_OctMonth", Doc.OctMonth);
                param.Add("@P_OctFromDay", Doc.OctFromDay);
                param.Add("@P_OctToDay", Doc.OctToDay);

                param.Add("@P_NovMonth", Doc.NovMonth);
                param.Add("@P_NovFromDay", Doc.NovFromDay);
                param.Add("@P_NovToDay", Doc.NovToDay);

                param.Add("@P_DecMonth", Doc.DecMonth);
                param.Add("@P_DecFromDay", Doc.DecFromDay);
                param.Add("@P_DecToDay", Doc.DecToDay);

                param.Add("@P_JanMonth", Doc.JanMonth);
                param.Add("@P_JanFromDay", Doc.JanFromDay);
                param.Add("@P_JanToDay", Doc.JanToDay);

                param.Add("@P_FebMonth", Doc.FebMonth);
                param.Add("@P_FebFromDay", Doc.FebFromDay);
                param.Add("@P_FebToDay", Doc.FebToDay);

                param.Add("@P_MarMonth", Doc.MarMonth);
                param.Add("@P_MarFromDay", Doc.MarFromDay);
                param.Add("@P_MarToDay", Doc.MarToDay);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_DocumentOpenClose", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = Message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_IncomeTax_MonthWiseDoc", "Setting_IncomeTax_MonthWiseDoc");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 76;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_DocumentOpenCloseId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_IncomeTax_DocumentOpenClose", param);
                ViewBag.DocSettingList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        //[HttpGet]
        //public ActionResult DocumentNameId(int DocumentNameId)
        //{
        //    param.Add("@query", "Select  DocumentId, DocumentName from IncomeTax_Document where DocumentId=DocumentNameId and Deactivate=0");
        //    var GetDocumentList = DapperORM.ReturnList<IncomeTax_Document>("sp_QueryExcution", param).ToList();          
        //    return Json(GetDocumentList, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        //public ActionResult Delete(int OpenCloseTypeID)
        //{
        //    try
        //    {
        //        param.Add("@p_process", "Delete");
        //        param.Add("@p_OpenCloseTypeID", OpenCloseTypeID);
        //        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //        var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_DocumentOpenClose", param);
        //        var message = param.Get<string>("@p_msg");
        //        var Icon = param.Get<string>("@p_Icon");
        //        TempData["Message"] = message;
        //        TempData["Icon"] = Icon.ToString();
        //        return RedirectToAction("GetList", "Setting_IncomeTax_MonthWiseDoc");
        //    }
        //    catch (Exception Ex)
        //    {
        //        return RedirectToAction(Ex.Message.ToString(), "Setting_IncomeTax_MonthWiseDoc");
        //    }

        //}
    }
}