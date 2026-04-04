using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_HRSpace;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_HRSpace
{
    public class ESS_HRSpace_AddLettersController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_HRSpace_AddLetters
        public ActionResult ESS_HRSpace_AddLetters(string LetterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 669;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                HRSpace_Letter HRSpace_Letter = new HRSpace_Letter();

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                DynamicParameters paramletters = new DynamicParameters();
                paramletters.Add("@query", "Select ID as Id,LetterName As Name from HRSpace_LetterMaster where Deactivate=0");
                ViewBag.GetLetters = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", paramletters).ToList();

                if (LetterId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_LetterId_Encrypted", LetterId_Encrypted);
                    HRSpace_Letter = DapperORM.ReturnList<HRSpace_Letter>("sp_List_HRSpace_Letter", param).FirstOrDefault();
                }
                else
                {
                    TempData["ToDate"] = null;
                    TempData["FromDate"] = null;
                }
                return View(HRSpace_Letter);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region IsLetterExists
        [HttpGet]
        public ActionResult IsLetterExists(int CmpId, string LetterId_Encrypted,int HRSpace_letterMaster_Id)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_LetterId_Encrypted", LetterId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_HRSpace_letterMaster_Id", HRSpace_letterMaster_Id);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_HRSpace_Letter", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    if (Message != "")
                    {
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        [ValidateInput(false)]
        public ActionResult SaveUpdate(HRSpace_Letter Letter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //var id = Letter.HRSpace_letterMaster_Id;
                DynamicParameters paramletters = new DynamicParameters();
                paramletters.Add("@query", "Select LetterName from HRSpace_LetterMaster where Deactivate=0 and HRSpace_LetterMaster.ID = '"+ Letter.HRSpace_letterMaster_Id + "'");
                var GetLetterName = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramletters).FirstOrDefault();

                param.Add("@p_process", string.IsNullOrEmpty(Letter.LetterId_Encrypted) ? "Save" : "Update");
                param.Add("@p_LetterId_Encrypted", Letter.LetterId_Encrypted);
                param.Add("@p_CmpId", Letter.CmpId);
                param.Add("@p_LetterName", GetLetterName.LetterName);
                param.Add("@p_HRSpace_letterMaster_Id", Letter.HRSpace_letterMaster_Id);
                param.Add("@p_Letter_Description", Letter.Letter_Description);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_IsSendToEmployee", Letter.IsSendToEmployee);
                param.Add("@p_Letter_Header", Letter.Letter_Header);
                param.Add("@p_Letter_Footer", Letter.Letter_Footer);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_HRSpace_Letter", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_HRSpace_AddLetters");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion SaveUpdate

        #region GetList 
        public ActionResult GetList(int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 669;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                param.Add("@p_LetterId_Encrypted", "List");
                param.Add("@p_CompanyId", CmpId);
                var data = DapperORM.DynamicList("sp_List_HRSpace_Letter", param);
                ViewBag.GetLetterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region Delete
        public ActionResult Delete(string LetterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LetterId_Encrypted", LetterId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_HRSpace_Letter", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_HRSpace_AddLetters");
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