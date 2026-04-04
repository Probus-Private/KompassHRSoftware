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
    public class Module_Employee_ExpInfoController : Controller
    {
        // GET: Module/Module_Employee_ExpInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        #region ExpInfo MAin View
        [HttpGet]
        public ActionResult Module_Employee_ExpInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 429;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                var EmployeeId = Session["EmployeeId"];
                ViewBag.AddUpdateTitle = "Add";

                var countEMP = DapperORM.DynamicQuerySingle("Select count(ExperienceEmployeeID)  as ExperienceEmployeeID  from Mas_Employee_Experience where Mas_Employee_Experience.ExperienceEmployeeID=" + Session["OnboardEmployeeId"] + "");
                TempData["CountExperienceEmployeeID"] = countEMP.ExperienceEmployeeID;

                Mas_Employee_Experience mas_employeeExperience = new Mas_Employee_Experience();
                param.Add("@p_ExperienceEmployeeID", Session["OnboardEmployeeId"]);
                var data = DapperORM.ReturnList<Mas_Employee_Experience>("sp_List_Mas_Employee_Experience", param).ToList();
                ViewBag.GetEmployeeExperienceList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        public ActionResult GetExperienceDetails(string ExperienceEmployeeID)
        {
            try
            {
                param.Add("@p_ExperienceEmployeeID", ExperienceEmployeeID);
                var data = DapperORM.ReturnList<Mas_Employee_Experience>("sp_List_Mas_Employee_Experience", param).ToList();

                if (data != null)
                {
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Experience details not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region IsValidation
        [HttpGet]
        public ActionResult IsExpInfoExists(string ExperienceId_Encrypted, string CompanyName)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_ExperienceId_Encrypted", ExperienceId_Encrypted);
                param.Add("@p_CompanyName", CompanyName);
                param.Add("@p_ExperienceEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Experience", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Experience EmployeeExperience)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeExperience.ExperienceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ExperienceID", EmployeeExperience.ExperienceID);
                param.Add("@p_ExperienceId_Encrypted", EmployeeExperience.ExperienceId_Encrypted);
                param.Add("@p_ExperienceEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_CompanyName", EmployeeExperience.CompanyName);
                param.Add("@p_IndustryType", EmployeeExperience.IndustryType);
                param.Add("@p_HRName", EmployeeExperience.HRName);
                param.Add("@p_ManagerName", EmployeeExperience.ManagerName);
                param.Add("@p_HREmailID", EmployeeExperience.HREmailID);
                param.Add("@p_ManagerEmailID", EmployeeExperience.ManagerEmailID);
                param.Add("@p_HRContact", EmployeeExperience.HRContact);
                param.Add("@p_ManagerContact", EmployeeExperience.ManagerContact);
                param.Add("@p_CompanyAddress", EmployeeExperience.CompanyAddress);
                param.Add("@p_Desigantion", EmployeeExperience.Desigantion);
                param.Add("@p_Department", EmployeeExperience.Department);
                param.Add("@p_StartDate", EmployeeExperience.StartDate);
                param.Add("@p_EndDate", EmployeeExperience.EndDate);
                param.Add("@p_Salary", EmployeeExperience.Salary);
                param.Add("@p_JobDescription", EmployeeExperience.JobDescription);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Experience]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_ExpInfo", "Module_Employee_ExpInfo");
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
        public ActionResult Delete(string ExperienceId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ExperienceId_Encrypted", ExperienceId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Experience", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_ExpInfo", "Module_Employee_ExpInfo");
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

                int onboardEmployeeId = Convert.ToInt32(Session["OnboardEmployeeId"]);
                string employeeName = Session["EmployeeName"]?.ToString();
                string machineName = Dns.GetHostName();

                var result = DapperORM.QuerySingle("SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeID = @empId", new { empId = onboardEmployeeId });
                var preboardingFid = result.PreboardingFid;

                string insertSql = @"
            INSERT INTO Mas_Employee_Experience (
                Deactivate,
                CreatedBy,
                CreatedDate,
                MachineName,
                ExperienceEmployeeID,
                CompanyName,
                IndustryType,
                HRName,
                ManagerName,
                HREmailID,
                ManagerEmailID,
                HRContact,
                ManagerContact,
                CompanyAddress,
                Desigantion,
                Department,
                StartDate,
                EndDate,
                Salary,
                JobDescription
            )
            SELECT
                0,
                @EmployeeName,
                GETDATE(),
                @MachineName,
                @EmployeeID,
                CompanyName,
                IndustryType,
                HRName,
                ManagerName,
                HREmailID,
                ManagerEmailID,
                HRContactNo,
                ManagerContactNo,
                CompanyAddress,
                Desigantion,
                Role,
                StartDate,
                EndDate,
                Salary,
                Functional
            FROM Preboarding_Mas_EmployeeExperience
            WHERE ExperiencePreboardingFid = @PreboardingFid";

                var parameters = new
                {
                    EmployeeName = employeeName,
                    MachineName = machineName,
                    EmployeeID = onboardEmployeeId,
                    PreboardingFid = preboardingFid
                };

                int rowsInserted = DapperORM.Execute(insertSql, parameters);

                if (rowsInserted > 0)
                {
                    string updateSql = @"
                UPDATE Mas_Employee_Experience
                SET ExperienceId_Encrypted = master.dbo.fn_varbintohexstr(
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(70), ExperienceEmployeeID))
                )
                WHERE ExperienceEmployeeID = '" + onboardEmployeeId + "'";

                    DapperORM.Execute(updateSql);

                    string updateSetupSql = @"
                UPDATE Mas_Employee_Setup
                SET SetupPreEmployee = 1
                WHERE SetupEmployeeID = '"+ onboardEmployeeId + "'";

                    DapperORM.Execute(updateSetupSql);
                }

                return RedirectToAction("Module_Employee_ExpInfo", "Module_Employee_ExpInfo");
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