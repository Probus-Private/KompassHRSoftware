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
    public class Module_Employee_FamilyInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_FamilyInfo

        #region FamilyInfo Main View
        [HttpGet]
        public ActionResult Module_Employee_FamilyInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 428;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Family mas_employeeFamily = new Mas_Employee_Family();
                param.Add("@query", "Select RelationId,RelationName from Mas_Relation Where Deactivate=0");
                var List_Relation = DapperORM.ReturnList<Mas_Relation>("sp_QueryExcution", param).ToList();
                ViewBag.GetRelationName = List_Relation;
                var countEMP = DapperORM.DynamicQuerySingle("Select count(FamilyEmployeeId)  as FamilyEmployeeId  from Mas_Employee_Family where Mas_Employee_Family.FamilyEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountFamilyEmployeeId"] = countEMP.FamilyEmployeeId;
                var countPreBoard = DapperORM.DynamicQuerySingle("select  COUNT(FamilyID) as FamilyID from Preboarding_Mas_EmployeeFamily left join mas_employee on preboardingfid=FamilyPreboardingFid where employeeid= " + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.FamilyID;
                param = new DynamicParameters();
                param.Add("@p_FamilyEmployeeId", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Family", param).ToList();
                ViewBag.GetEmployeeFamilyList = data;
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
        public ActionResult IsFamilyExists(string FamilyId_Encrypted, string AadharNo)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_FamilyId_Encrypted", FamilyId_Encrypted);
                param.Add("@p_AadharNo", AadharNo);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Family", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Family Employeefamily)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Employeefamily.FamilyId_Encrypted) ? "Save" : "Update");
                param.Add("@p_FamilyId", Employeefamily.FamilyId);
                param.Add("@p_FamilyId_Encrypted", Employeefamily.FamilyId_Encrypted);
                param.Add("@p_FamilyEmployeeId", OnboardEmployeeId);
                param.Add("@p_MemberName", Employeefamily.MemberName);
                param.Add("@p_Relation", Employeefamily.Relation);
                param.Add("@p_DOB", Employeefamily.DOB);
                param.Add("@p_AadharNo", Employeefamily.AadharNo);
                param.Add("@p_ESIC_InsuranceType", Employeefamily.ESIC_InsuranceType);
                param.Add("@p_Member_Residing", Employeefamily.Member_Residing);
                param.Add("@p_TownName", Employeefamily.TownName);
                param.Add("@p_StateName", Employeefamily.StateName);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Family]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_FamilyInfo", "Module_Employee_FamilyInfo");
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
        public ActionResult Delete(string FamilyId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_FamilyId_Encrypted", FamilyId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Family", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_FamilyInfo", "Module_Employee_FamilyInfo");
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

                // Get PreboardingFid
                var preboarding = DapperORM.QuerySingle(
                    "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeID = @empId",
                    new { empId = onboardEmployeeId }
                );
                var preboardingFid = preboarding.PreboardingFid;

                // 1️⃣ Insert records
                string insertSql = $@"
INSERT INTO Mas_Employee_Family
(Deactivate, CreatedBy, CreatedDate, MachineName, FamilyEmployeeId,
 MemberName, Relation, Age, DOB, AadharNo, ESIC_InsuranceType,
 TownName, StateName, Member_Residing)
SELECT 
    0,
    '{Session["EmployeeName"]}',
    GETDATE(),
    '{Dns.GetHostName()}',
    {onboardEmployeeId},
    MemberName,
    Relation,
    Age,
    DOB,
    AadharNo,
    ESIC_InsuranceType,
    TownName,
    StateName,
    MemberResiding
FROM Preboarding_Mas_EmployeeFamily
WHERE FamilyPreboardingFid = {preboardingFid};";

                string errorMsg = "";
                if (!objcon.SaveStringBuilder(new StringBuilder(insertSql), out errorMsg))
                {
                    TempData["Message"] = errorMsg;
                    TempData["Icon"] = "error";
                    return RedirectToAction("Module_Employee_FamilyInfo");
                }

                // 2️⃣ Encrypt FamilyId and update setup flag
                string updateSql = @"
UPDATE Mas_Employee_Family
SET FamilyId_Encrypted = master.dbo.fn_varbintohexstr(
                             HASHBYTES('SHA2_256', CONVERT(NVARCHAR(70), FamilyId))
                         )
WHERE FamilyEmployeeId = @EmpId
  AND (FamilyId_Encrypted IS NULL OR FamilyId_Encrypted = '');

UPDATE Mas_Employee_Setup
SET SetupFamily = '1'
WHERE SetupEmployeeId = @EmpId;";

                DapperORM.Execute(updateSql, new { EmpId = onboardEmployeeId });

                TempData["Message"] = "Record saved successfully";
                TempData["Icon"] = "success";

                return RedirectToAction("Module_Employee_FamilyInfo", "Module_Employee_FamilyInfo");
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

