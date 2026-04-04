using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_RecruitmentAssign_DesignationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_RecruitmentAssign_Designation

        #region RecruitmentAssign_Designation Main View 
      
        public ActionResult Setting_RecruitmentAssign_Designation(Recruitment_AssignDesignation OBJAssignDesignation)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 236;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_AssignDesignation RecruitmentAssignDesignation = new Recruitment_AssignDesignation();
                var results = DapperORM.DynamicQueryMultiple(@"SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0 ORDER BY Name;
                                                     SELECT GradeId as Id,GradeName as Name FROM Mas_Grade WHERE Deactivate =0 ORDER BY Name;
                                                    select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and ContractorID=1 and Mas_Employee.EmployeeLeft=0");
                ViewBag.DesignationList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.EmployeeName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.DesignationList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GradeList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.EmployeeName = results.Read<AllDropDownClass>().ToList();


                if (OBJAssignDesignation.AssignDesignationEmployeeID!=0)
                {
                    var data = DapperORM.DynamicQueryList("Select '1' as IsActive ,RecruitmentDesignationID as DesignationId , DesignationName from Recruitment_AssignDesignation,Mas_Designation  where RecruitmentDesignationID=DesignationId and AssignDesignationEmployeeID=" + OBJAssignDesignation.AssignDesignationEmployeeID + " union all select '0' as IsActive ,DesignationId, DesignationName from Mas_Designation where Mas_Designation.Deactivate=0 and DesignationId not in (select RecruitmentDesignationID from Recruitment_AssignDesignation where  Recruitment_AssignDesignation.deactivate=0 and AssignDesignationEmployeeID=" + OBJAssignDesignation.AssignDesignationEmployeeID + ")order by DesignationName");
                    ViewBag.AssignDesignationList = data;
                }
                else
                {
                    ViewBag.AssignDesignationList = "";
                }
                return View(OBJAssignDesignation);
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
        public ActionResult SaveUpdate(List<AssignDesignation> DesignationRecord,string DesignationEmployeeID)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                for (var i = 0; i < DesignationRecord.Count; i++)
                {
                    param.Add("@p_process", "Save");                    
                    param.Add("@p_AssignDesignationEmployeeID", DesignationEmployeeID);
                    param.Add("@p_RecruitmentDesignationID", DesignationRecord[i].RecruitmentDesignationID);
                    param.Add("@p_IsActive", '1');
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_AssignDesignation", param);
                    var msg = param.Get<string>("@p_msg");
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 236;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_RecruitmentAssignDesignationID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_AssignDesignation", param).ToList();
                ViewBag.GetAssignDesignationList = data;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(int? AssignDesignationEmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AssignDesignationEmployeeID", AssignDesignationEmployeeID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_AssignDesignation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_RecruitmentAssign_Designation");
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