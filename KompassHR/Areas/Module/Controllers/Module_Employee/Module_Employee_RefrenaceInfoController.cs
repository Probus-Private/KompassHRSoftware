using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_RefrenaceInfoController : Controller
    {
        
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Employee/Module_Employee_RefrenaceInfo
        #region RefrenaceInfo MAin View
        [HttpGet]
        public ActionResult Module_Employee_RefrenaceInfo(string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 426;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";
                param.Add("@query", "Select RelationId,RelationName from Mas_Relation Where Deactivate=0");
                var List_Relative = DapperORM.ReturnList<Mas_Relation>("sp_QueryExcution", param).ToList();
                ViewBag.GetRelativeName = List_Relative;
                Mas_Employee_Reference mas_employeeReference = new Mas_Employee_Reference();

                var countEMP = DapperORM.DynamicQuerySingle("Select count(ReferenceEmployeeId)  as ReferenceEmployeeId  from Mas_Employee_Reference where Mas_Employee_Reference.ReferenceEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountReferenceEmployeeId"] = countEMP.ReferenceEmployeeId;
                if (Status == "Pre")
                {
                    var GetPreboardingFid = DapperORM.DynamicQuerySingle("Select PreboardingFid from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");
                    var PreboardingFid = GetPreboardingFid.PreboardingFid;

                    param = new DynamicParameters();
                    param.Add("@p_FId", PreboardingFid);
                    mas_employeeReference = DapperORM.ReturnList<Mas_Employee_Reference>("sp_GetList_Mas_PreboardEmployeeDetails", param).FirstOrDefault();
                    ViewBag.GetEmployeePersonal = mas_employeeReference;
                    return View(mas_employeeReference);

                }

               

                param = new DynamicParameters();
                param.Add("@p_ReferenceEmployeeId", Session["OnboardEmployeeId"]);
                mas_employeeReference = DapperORM.ReturnList<Mas_Employee_Reference>("sp_List_Mas_Employee_Reference", param).FirstOrDefault();
                if (mas_employeeReference != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                }
                return View(mas_employeeReference);
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
        public ActionResult SaveUpdate(Mas_Employee_Reference EmployeeReference)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeReference.ReferenceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ReferenceId", EmployeeReference.ReferenceId);
                param.Add("@p_ReferenceId_Encrypted", EmployeeReference.ReferenceId_Encrypted);
                param.Add("@p_ReferenceEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_PrimaryNeighboursName", EmployeeReference.PrimaryNeighboursName);
                param.Add("@p_PrimaryNeighboursMobile", EmployeeReference.PrimaryNeighboursMobile);
                param.Add("@p_SecondaryNeighboursName", EmployeeReference.SecondaryNeighboursName);
                param.Add("@p_SecondaryNeighboursMobile", EmployeeReference.SecondaryNeighboursMobile);
                param.Add("@p_RelativeName", EmployeeReference.RelativeName);
                param.Add("@p_RelativeRelation", EmployeeReference.RelativeRelation);
                param.Add("@p_RelativeMobile", EmployeeReference.RelativeMobile);
                param.Add("@p_PrimaryEmergencyName", EmployeeReference.PrimaryEmergencyName);
                param.Add("@p_PrimaryEmergencyRelation", EmployeeReference.PrimaryEmergencyRelation);
                param.Add("@p_PrimaryEmergencyMobile", EmployeeReference.PrimaryEmergencyMobile);
                param.Add("@p_SecondaryEmergencyName", EmployeeReference.SecondaryEmergencyName);
                param.Add("@p_SecondaryEmergencyRelation", EmployeeReference.SecondaryEmergencyRelation);
                param.Add("@p_SecondaryEmergencyMobile", EmployeeReference.SecondaryEmergencyMobile);
                param.Add("@p_PrimaryCompanyRefName", EmployeeReference.PrimaryCompanyRefName);
                param.Add("@p_PrimaryCompanyRefMobile", EmployeeReference.PrimaryCompanyRefMobile);
                param.Add("@p_SecondaryCompanyRefName", EmployeeReference.SecondaryCompanyRefName);
                param.Add("@p_SecondaryCompanyRefMobile", EmployeeReference.SecondaryCompanyRefMobile);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Reference]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_RefrenaceInfo", "Module_Employee_RefrenaceInfo");
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