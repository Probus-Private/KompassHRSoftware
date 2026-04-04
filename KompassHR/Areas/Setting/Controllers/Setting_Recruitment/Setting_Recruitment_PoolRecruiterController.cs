using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Leave;
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
    public class Setting_Recruitment_PoolRecruiterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Recruitment_PoolRecruiter

        #region PoolRecruiter Main View 
        [HttpGet]
        public ActionResult Setting_Recruitment_PoolRecruiter(int? BranchId, int? CmpId, string RecruitmentPoolID_Encrypted,int? RecruitmentAssignEmployeeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_Pool RecruitmentPool = new Recruitment_Pool();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@query", "Select  CompanyId As Id, CompanyName As Name from Mas_CompanyProfile where Deactivate=0 order by Name");
                var listCompanyName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramCompany).ToList();
                ViewBag.GetCompanyName = listCompanyName;

                if (CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpId);
                    var listBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.GetBranchName = listBranchName;
                }
                else
                {
                    ViewBag.GetBranchName = "";
                }

                if (BranchId != null)
                {
                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@query", "Select EmployeeId As Id,EmployeeName As Name from Mas_employee where Deactivate=0 and EmployeeBranchId=" + BranchId + "order by Name");
                    var GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramName).ToList();
                    ViewBag.GetAssignList = GetEmployee;
                }
                else
                {
                    ViewBag.GetAssignList = "";
                }


                if (BranchId != null)
                {
                    DynamicParameters paramName = new DynamicParameters();
                    paramName.Add("@query", "Select EmployeeId As Id,EmployeeName As Name from Mas_employee where Deactivate=0 and EmployeeBranchId=" + BranchId + "order by Name");
                    var GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramName).ToList();
                    ViewBag.GetRecruiterList = GetEmployee;
                }
                else
                {
                    ViewBag.GetRecruiterList = "";
                }
                if (RecruitmentPoolID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_RecruitmentPoolID_Encrypted", RecruitmentPoolID_Encrypted);
                    RecruitmentPool = DapperORM.ReturnList<Recruitment_Pool>("sp_List_Recruitment_Pool", param).FirstOrDefault();
                }
                return View(RecruitmentPool);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetAssignName(int BranchId,int CmpID)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select RecruitmentAssignEmployeeID As Id, Mas_employee.EmployeeName As Name from Recruitment_Assign ,Mas_employee where Recruitment_Assign.Deactivate = 0 and Mas_employee.Deactivate = 0 and Mas_employee.EmployeeId = Recruitment_Assign.RecruitmentAssignEmployeeID  and Recruitment_Assign.BranchId = '" + BranchId + "' and Recruitment_Assign.CmpID = '" + CmpID + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.EmployeeList = data;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            
        }
        #endregion


        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetPoolRecruiterName(int BranchId, int CmpID)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select EmployeeId As Id,EmployeeName As Name from Mas_employee where Deactivate=0 and EmployeeBranchId='" + BranchId + "' and CmpID='" + CmpID + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult IsPoolExists(int CmpID, string RecruitmentPoolID_Encrypted, int BranchId, int RecruitmentPoolEmployeeID)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_BranchId", BranchId);
                param.Add("@p_RecruitmentPoolEmployeeID", RecruitmentPoolEmployeeID);
                param.Add("@p_RecruitmentPoolID_Encrypted", RecruitmentPoolID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Pool", param);
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
        public ActionResult SaveUpdate(Recruitment_Pool Pool)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(Pool.RecruitmentPoolID_Encrypted) ? "Save" : "Update");
                param.Add("@p_RecruitmentPoolID", Pool.RecruitmentPoolID);
                param.Add("@p_RecruitmentPoolID_Encrypted", Pool.RecruitmentPoolID_Encrypted);
                param.Add("@p_CmpID", Pool.CmpID);
                param.Add("@p_BranchId", Pool.BranchId);
                param.Add("@p_RecruitmentAssignEmployeeID", Pool.RecruitmentAssignEmployeeID );
                param.Add("@p_RecruitmentPoolEmployeeID", Pool.RecruitmentPoolEmployeeID);
                param.Add("@p_IsActive", Pool.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Pool", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Recruitment_PoolRecruiter", "Setting_Recruitment_PoolRecruiter");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList Views
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_RecruitmentPoolID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_Pool", param).ToList();
                ViewBag.GetPoolList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region MyRegion
        public ActionResult Delete(string RecruitmentPoolID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_RecruitmentAssignID_Encrypted", RecruitmentPoolID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Pool", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Recruitment_PoolRecruiter");
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