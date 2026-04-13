using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using KompassHR.Areas.Setting.Models.Setting_AccountAndFinance;
using System.Data.SqlClient;
using System.Net;
using System.Data;
using KompassHR.Areas.Setting.Models.Setting_Prime;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_JobOpeningController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: Recruitment/JobOpening
        public ActionResult Setting_Recruitment_JobOpening(string JobOpeningId_Encrypted)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Recruitment_JobOpening Recruitment_jobopening = new Recruitment_JobOpening();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select Max(DocNo)+1 As DocNo from Recruitment_JobOpening";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                if (JobOpeningId_Encrypted != null)
                {

                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_JobOpeningId_Encrypted", JobOpeningId_Encrypted);
                    Recruitment_jobopening = DapperORM.ReturnList<Recruitment_JobOpening>("sp_List_Recruitment_JobOpening", param).FirstOrDefault();
                    TempData["IsWalkIn"] = Recruitment_jobopening.IsWalkIn;
                    TempData["JobPostingDate"] = Recruitment_jobopening.JobPostingDate;
                    TempData["JobPostingClosingDate"] = Recruitment_jobopening.JobPostingClosingDate;
                    
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Recruitment_JobOpening where JobOpeningId_Encrypted='" + JobOpeningId_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }


                DynamicParameters paramCompanyName = new DynamicParameters();
                paramCompanyName.Add("@query", "Select CompanyId,CompanyName from Mas_CompanyProfile Where Deactivate=0");
                var list_JobOpening = DapperORM.ReturnList<Models.Setting_AccountAndFinance.Mas_CompanyProfile>("sp_QueryExcution", paramCompanyName).ToList();
                ViewBag.GetCompanyName = list_JobOpening;

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "Select BranchId,BranchName from  Mas_Branch Where Deactivate=0");
                var listmas_Branch = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.GetBranchName = listmas_Branch;

                DynamicParameters paramDesignationName = new DynamicParameters();
                paramDesignationName.Add("@query", "Select DesignationId,DesignationName from  Mas_Designation Where Deactivate=0");
                var listmas_Designation = DapperORM.ReturnList<Mas_Designation>("sp_QueryExcution", paramDesignationName).ToList();
                ViewBag.GetDesignationName = listmas_Designation;

                DynamicParameters paramDepartmentName = new DynamicParameters();
                paramDepartmentName.Add("@query", "Select DepartmentId,DepartmentName from  Mas_Department Where Deactivate=0");
                var listmas_Department = DapperORM.ReturnList<Mas_Department>("sp_QueryExcution", paramDepartmentName).ToList();
                ViewBag.GetDepartmentName = listmas_Department;

                DynamicParameters paramCurrencyName = new DynamicParameters();
                paramCurrencyName.Add("@query", "Select CurrencyId,CurrencyName from  Mas_Currency Where Deactivate=0");
                var listmas_Currency = DapperORM.ReturnList<Mas_Currency>("sp_QueryExcution", paramCurrencyName).ToList();
                ViewBag.GetCurrencyName = listmas_Currency;


                return View(Recruitment_jobopening);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(Recruitment_JobOpening JobOpening)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(JobOpening.JobOpeningId_Encrypted) ? "Save" : "Update");
                param.Add("@p_JobOpeningId", JobOpening.JobOpeningId);
                param.Add("@p_JobOpeningId_Encrypted", JobOpening.JobOpeningId_Encrypted);
                param.Add("@p_CmpId", JobOpening.CmpId);
                param.Add("@p_JobOpeningBranchId", JobOpening.JobOpeningBranchId);
                param.Add("@p_JobOpeningDepartmentId", JobOpening.JobOpeningDepartmentId);
                param.Add("@p_JobOpeningDesignationId", JobOpening.JobOpeningDesignationId);
                param.Add("@p_DocNo", JobOpening.DocNo);
                param.Add("@p_JobPostingEmployeeId", EmployeeId);
                param.Add("@p_JobPostingDate", JobOpening.JobPostingDate);
                param.Add("@p_JobPostingClosingDate", JobOpening.JobPostingClosingDate);
                param.Add("@p_JobTittle", JobOpening.JobTittle);
                param.Add("@p_NoOfPostions", JobOpening.NoOfPostions);
                param.Add("@p_JobLocation", JobOpening.JobLocation);
                param.Add("@p_Experience", JobOpening.Experience);
                param.Add("@p_AnnualSalary", JobOpening.AnnualSalary);
                param.Add("@p_CurrencyID", JobOpening.CurrencyID);
                param.Add("@p_JobType", JobOpening.JobType);
                param.Add("@p_PreferredCandidate", JobOpening.PreferredCandidate);
                param.Add("@p_Skill", JobOpening.Skill);
                param.Add("@p_JobDescription", JobOpening.JobDescription);
                param.Add("@p_RequiredWithinDays", JobOpening.RequiredWithinDays);
                param.Add("@p_NightShiftAllowed", JobOpening.NightShiftAllowed);
                param.Add("@p_WFMAllowed", JobOpening.WFMAllowed);
                param.Add("@p_HybridAllowed", JobOpening.HybridAllowed);
                param.Add("@p_NoOfDayWorkingInWeek", JobOpening.NoOfDayWorkingInWeek);
                param.Add("@p_AgeBetween", JobOpening.AgeBetween);
                param.Add("@p_JobOpeningManpowerRequisitionId", "0");
                param.Add("@p_IsWalkIn", JobOpening.IsWalkIn);
                param.Add("@p_WalkInFromDate", JobOpening.WalkInFromDate);
                param.Add("@p_WalkInToDate", JobOpening.WalkInToDate);
                param.Add("@p_WalkInTime", JobOpening.WalkInTime);
                param.Add("@p_WalkInVenue", JobOpening.WalkInVenue);
                param.Add("@p_ContactPerson", JobOpening.ContactPerson);
                param.Add("@p_MobileNo", JobOpening.MobileNo);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_JobOpening", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Recruitment_JobOpening", "Setting_Recruitment_JobOpening");
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
                param.Add("@p_JobOpeningId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Recruitment_JobOpening", param);
                ViewBag.GetJobOpeningList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult Delete(string JobOpeningId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_JobOpeningId_Encrypted", JobOpeningId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_JobOpening", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();

                return RedirectToAction("GetList", "Setting_Recruitment_JobOpening");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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
    }
}