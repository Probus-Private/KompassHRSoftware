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
    public class Module_Employee_QualificationInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Module/Module_Employee_QualificationInfo
        #region Qualification Main View
        [HttpGet]
        public ActionResult Module_Employee_QualificationInfo()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 430;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Qualification mas_Employeequalification = new Mas_Employee_Qualification();

                var countEMP = DapperORM.DynamicQuerySingle("Select count(QualificationEmployeeID)  as QualificationEmployeeID  from Mas_Employee_Qualification where Mas_Employee_Qualification.QualificationEmployeeID=" + Session["OnboardEmployeeId"] + "");
                TempData["CountQualificationEmployeeID"] = countEMP.QualificationEmployeeID;

                var countPreBoard = DapperORM.DynamicQuerySingle("select  COUNT(QualificationID) as QualificationID from Preboarding_Mas_EmployeeQualification left join mas_employee on preboardingfid=PreboardingQualificationFid where employeeid= " + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.QualificationID;

                param = new DynamicParameters();
                param.Add("@p_QualificationEmployeeID", Session["OnboardEmployeeId"]);
                var data = DapperORM.ReturnList<Mas_Employee_Qualification>("sp_List_Mas_Employee_Qualification", param).ToList();
                ViewBag.GetEmployeeQualificationList = data;               
                return View(mas_Employeequalification);
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
        public ActionResult IsQualificationExists(string QualificationID_Encrypted, string QualificationName)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_QualificationID_Encrypted", QualificationID_Encrypted);
                param.Add("@p_QualificationName", QualificationName);
                param.Add("@p_QualificationEmployeeID", Session["OnboardEmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Qualification", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Qualification EmployeeQualification)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeQualification.QualificationID_Encrypted) ? "Save" : "Update");
                param.Add("@p_QualificationID", EmployeeQualification.QualificationID);
                param.Add("@p_QualificationID_Encrypted", EmployeeQualification.QualificationID_Encrypted);
                param.Add("@p_QualificationEmployeeID", OnboardEmployeeId);
                param.Add("@p_QualificationName", EmployeeQualification.QualificationName);
                param.Add("@p_QualificationType", EmployeeQualification.QualificationType);
                param.Add("@p_QualificationUniversity", EmployeeQualification.QualificationUniversity);
                param.Add("@p_QualificationPassingYear", EmployeeQualification.QualificationPassingYear);
                param.Add("@p_QualificationMark", EmployeeQualification.QualificationMark);
                param.Add("@p_QualificationRollNo", EmployeeQualification.QualificationRollNo);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Qualification]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_QualificationInfo", "Module_Employee_QualificationInfo");
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
        public ActionResult Delete(string QualificationID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_QualificationID_Encrypted", QualificationID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parame
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Qualification", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_QualificationInfo", "Module_Employee_QualificationInfo");
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

                var GetPreboardingFid = DapperORM.DynamicQuerySingle(
                    "SELECT PreboardingFid FROM Mas_Employee WHERE EmployeeID = " + Session["OnboardEmployeeId"]
                );
                var PreboardingFid = GetPreboardingFid.PreboardingFid;

                string Employee_Admin = "  INSERT INTO Mas_Employee_Qualification ( " +
                                         " Deactivate, CreatedBy, CreatedDate, MachineName, QualificationEmployeeID, " +
                                         " QualificationName, QualificationType, QualificationUniversity, " +
                                         " QualificationPassingYear, QualificationMark, QualificationRollNo) " +
                                         " SELECT " +
                                         " 0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName().ToString() + "', " + Session["OnboardEmployeeId"] + ", " +
                                         " QualificationName, QualificationType, QualificationUniversity, QualificationYear, QualificationMark, PRN_No " +
                                         " FROM Preboarding_Mas_EmployeeQualification " +
                                         " WHERE PreboardingQualificationFid = " + PreboardingFid;

                strBuilder.Append(Employee_Admin);

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    // 🔐 Encryption logic after insert
                    string updateSql = @"
                UPDATE Mas_Employee_Qualification
                SET QualificationId_Encrypted = master.dbo.fn_varbintohexstr(
                    HASHBYTES('SHA2_256', CONVERT(NVARCHAR(70), QualificationId))
                )
                WHERE QualificationEmployeeID = @EmpId
                  AND (QualificationId_Encrypted IS NULL OR QualificationId_Encrypted = '')";

                    DapperORM.Execute(updateSql, new { EmpId = Session["OnboardEmployeeId"] });

                    // ✅ ADDITION: SetupQualification flag update
                    string updateSetupSql = @"
                UPDATE Mas_Employee_Setup
                SET SetupQualification = '1'
                WHERE SetupEmployeeId = @EmpId";

                    DapperORM.Execute(updateSetupSql, new { EmpId = Session["OnboardEmployeeId"] });
                }

                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                }

                return RedirectToAction("Module_Employee_QualificationInfo", "Module_Employee_QualificationInfo");
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