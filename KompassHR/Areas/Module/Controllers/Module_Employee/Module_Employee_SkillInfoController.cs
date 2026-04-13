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
    public class Module_Employee_SkillInfoController : Controller
    {
        // GET: Module/Module_Employee_SkillInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        #region SkillInfo MAin View
        [HttpGet]
        public ActionResult Module_Employee_SkillInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 431;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Skill mas_employeeSkill = new Mas_Employee_Skill();
                param.Add("@query", "Select SkillId As Id,SkillName As Name from Mas_Skill Where Deactivate=0");
                var List_skills = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetSkillName = List_skills;

                var countEMP = DapperORM.DynamicQuerySingle("Select count(SkillEmployeeID)  as SkillEmployeeID  from Mas_Employee_Skill where Mas_Employee_Skill.SkillEmployeeID=" + Session["OnboardEmployeeId"] + "");
                TempData["CountSkillEmployeeID"] = countEMP.SkillEmployeeID;

                var countPreBoard = DapperORM.DynamicQuerySingle("select  COUNT(EmployeeSkillID) as EmployeeSkillID from Preboarding_Mas_EmployeeSkill left join mas_employee on preboardingfid=SkillPreboardingFid where employeeid= " + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.EmployeeSkillID;

                param = new DynamicParameters();
                param.Add("@p_SkillEmployeeID", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Skill", param).ToList();
                ViewBag.GetEmployeeSkillList = data;
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
        public ActionResult IsSkillsExists(string EmployeeSkillID_Encrypted, string SkillID)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_EmployeeSkillID_Encrypted", EmployeeSkillID_Encrypted);
                param.Add("@p_SkillID", SkillID);
                param.Add("@p_SkillEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Skill", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Skill EmployeeSkills)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];

                param.Add("@p_process", string.IsNullOrEmpty(EmployeeSkills.EmployeeSkillID_Encrypted) ? "Save" : "Update");
                param.Add("@p_EmployeeSkillID", EmployeeSkills.EmployeeSkillID);
                param.Add("@p_EmployeeSkillID_Encrypted", EmployeeSkills.EmployeeSkillID_Encrypted);
                param.Add("@p_SkillEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_SkillID", EmployeeSkills.SkillID);
                param.Add("@p_Experience", EmployeeSkills.Experience);
                param.Add("@p_Remark", EmployeeSkills.Remark);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Skill]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_SkillInfo", "Module_Employee_SkillInfo");
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
        public ActionResult Delete(string EmployeeSkillID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EmployeeSkillID_Encrypted", EmployeeSkillID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Skill", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_SkillInfo", "Module_Employee_SkillInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetPersonalAsPrebording
        [HttpGet]
        public ActionResult GetSkillAsPrebording()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramPersonal = new DynamicParameters();
                paramPersonal.Add("@p_DocOrigin", "Skill");
                paramPersonal.Add("@p_EmployeeId", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetPreboardingEmployeeInfo", paramPersonal).FirstOrDefault();
                return Json(data, JsonRequestBehavior.AllowGet);
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
                // 🔑 Validate login
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                if (Session["OnboardEmployeeId"] == null)
                {
                    TempData["Message"] = "On-boarding employee not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_SkillInfo", "Module_Employee_SkillInfo");
                }

                int onboardEmpId = Convert.ToInt32(Session["OnboardEmployeeId"]);

                // 🔎 Get Pre-boarding Fid
                var getPreboardingFid = DapperORM.QuerySingle(
                    "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeID = @EmpId",
                    new { EmpId = onboardEmpId }
                );

                if (getPreboardingFid == null)
                {
                    TempData["Message"] = "Pre-boarding record not found.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_SkillInfo", "Module_Employee_SkillInfo");
                }

                int preboardingFid = Convert.ToInt32(getPreboardingFid.PreboardingFid);

                // 📥 Insert skills from pre-boarding
                string insertSql = @"
            INSERT INTO Mas_Employee_Skill
                (Deactivate, CreatedBy, CreatedDate, MachineName, SkillEmployeeID, SkillID, Experience, Remark)
            SELECT
                0,
                @CreatedBy,
                GETDATE(),
                @MachineName,
                @EmpId,
                SkillID,
                Experience,
                Remark
            FROM Preboarding_Mas_EmployeeSkill
            WHERE SkillPreboardingFid = @PreboardingFid";

                var parameters = new
                {
                    CreatedBy = Session["EmployeeName"]?.ToString(),
                    MachineName = Dns.GetHostName(),
                    EmpId = onboardEmpId,
                    PreboardingFid = preboardingFid
                };

                // Execute insert
                int rows = DapperORM.Execute(insertSql, parameters);

                if (rows > 0)
                {
                    // 🔐 Encrypt new skill IDs
                    string updateEncryptSql = @"
                UPDATE Mas_Employee_Skill
                SET EmployeeSkillID_Encrypted = master.dbo.fn_varbintohexstr(
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(70), EmployeeSkillID))
                )
                WHERE SkillEmployeeID = @EmpId
                  AND (EmployeeSkillID_Encrypted IS NULL OR EmployeeSkillID_Encrypted = '')";

                    DapperORM.Execute(updateEncryptSql, new { EmpId = onboardEmpId });

                    // ✅ Mark setup flag
                    string updateSetupSql = @"
                UPDATE Mas_Employee_Setup
                SET SetupSkills = '1'
                WHERE SetupEmployeeId = @EmpId";

                    DapperORM.Execute(updateSetupSql, new { EmpId = onboardEmpId });

                    TempData["Message"] = "Record Saved Successfully";
                    TempData["Icon"] = "success";
                }
                else
                {
                    TempData["Message"] = "No skill records found to transfer.";
                    TempData["Icon"] = "error";
                }

                return RedirectToAction("Module_Employee_SkillInfo", "Module_Employee_SkillInfo");
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