using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_LanguageInfoController : Controller
    {
        // GET: Module/Module_Employee_LanguageInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        #region LanguageInfo Main View
        [HttpGet]
        public ActionResult Module_Employee_LanguageInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 432;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_Employee_Language mas_employeeLanguage = new Mas_Employee_Language();
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                param.Add("@query", "Select LanguageId as  Id,LanguageName as Name from Mas_Language Where Deactivate=0");
                var List_Language = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetLanguageName = List_Language;

                var countEMP = DapperORM.DynamicQuerySingle("Select count(LanguageEmployeeID)  as LanguageEmployeeID  from Mas_Employee_Language where Mas_Employee_Language.LanguageEmployeeID=" + Session["OnboardEmployeeId"] + "");
                TempData["CountLanguageEmployeeID"] = countEMP.LanguageEmployeeID;

                var countPreBoard = DapperORM.DynamicQuerySingle("select  COUNT(LanguagePreboardingFid) as LanguagePreboardingFid from Preboarding_Mas_EmployeeLanguage left join mas_employee on preboardingfid=LanguagePreboardingFid where employeeid= " + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.LanguagePreboardingFid;

                param = new DynamicParameters();
                param.Add("@p_LanguageEmployeeID", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Language", param).ToList();
                ViewBag.GetEmployeeLanguageList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IslanguageExists(string LangEmpID_Encrypted, string LanguageID)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_LangEmpID_Encrypted", LangEmpID_Encrypted);
                param.Add("@p_LanguageID", LanguageID);
                param.Add("@p_LanguageEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Language", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_Employee_Language EmployeeLanguage)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeLanguage.LangEmpID_Encrypted) ? "Save" : "Update");
                param.Add("@p_LangEmpID", EmployeeLanguage.LangEmpID);
                param.Add("@p_LangEmpID_Encrypted", EmployeeLanguage.LangEmpID_Encrypted);
                param.Add("@p_LanguageEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_LanguageID", EmployeeLanguage.LanguageID);
                param.Add("@p_empRead", EmployeeLanguage.empRead);
                param.Add("@p_empWrite", EmployeeLanguage.empWrite);
                param.Add("@p_empSpeak", EmployeeLanguage.empSpeak);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Language]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_LanguageInfo", "Module_Employee_LanguageInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string LangEmpID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LangEmpID_Encrypted", LangEmpID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Language", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_LanguageInfo", "Module_Employee_LanguageInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region PreboardingGetDetails
        public ActionResult PreboardingGetDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();

                // 🔎 Get PreboardingFid
                var GetPreboardingFid = DapperORM.QuerySingle(
                    "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeID = @empId",
                    new { empId = Session["OnboardEmployeeId"] }
                );

                if (GetPreboardingFid == null)
                {
                    TempData["Message"] = "Pre-boarding record not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_LanguageInfo", "Module_Employee_LanguageInfo");
                }

                var PreboardingFid = GetPreboardingFid.PreboardingFid;

                // 📥 Insert from Preboarding table
                string insertSql = $@"
INSERT INTO Mas_Employee_Language
(Deactivate, CreatedBy, CreatedDate, MachineName, LanguageEmployeeID,
 LanguageID, EmpRead, EmpWrite, EmpSpeak)
SELECT 
    0,
    '{Session["EmployeeName"]}',
    GETDATE(),
    '{Dns.GetHostName()}',
    {Session["OnboardEmployeeId"]},
    LanguageID,
    EmpRead,
    EmpWrite,
    EmpSpeak
FROM Preboarding_Mas_EmployeeLanguage
WHERE LanguagePreboardingFid = {PreboardingFid};";

                string errMsg = "";
                if (!objcon.SaveStringBuilder(new StringBuilder(insertSql), out errMsg))
                {
                    TempData["Message"] = errMsg;
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_LanguageInfo");
                }

                // 🔑 Encrypt LangEmpID
                string updateEncryptSql = @"
UPDATE M
SET M.LangEmpID_Encrypted =
    CONVERT(VARCHAR(70), HASHBYTES('SHA2_256', CAST(M.LangEmpID AS NVARCHAR(50))), 2)
FROM Mas_Employee_Language M
WHERE M.LanguageEmployeeID = @EmpId
  AND (M.LangEmpID_Encrypted IS NULL OR M.LangEmpID_Encrypted = '');";

                DapperORM.Execute(updateEncryptSql, new { EmpId = Session["OnboardEmployeeId"] });

                // ✅ Update SetupLanguages flag in Mas_Employee_Setup
                string updateSetupSql = @"
UPDATE Mas_Employee_Setup
SET SetupLanguage = 1
WHERE SetupEmployeeId = @EmpId;";

                DapperORM.Execute(updateSetupSql, new { EmpId = Session["OnboardEmployeeId"] });

                // 🎉 Success message
                TempData["Message"] = "Record Saved Successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("Module_Employee_LanguageInfo", "Module_Employee_LanguageInfo");
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