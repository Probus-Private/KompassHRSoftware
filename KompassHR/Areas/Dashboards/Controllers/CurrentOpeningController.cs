using Dapper;
using KompassHR.Areas.Dashboards.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Dashboards.Controllers
{
    public class CurrentOpeningController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Dashboard_1/CurrentOpening
        #region Current Opening Main View Page
        public ActionResult CurrentOpening(string JobOpeningId_Encrypted)
        {
            SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
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
        #endregion

        #region Current Opening More Details View Page
        public ActionResult CurrentOpeningMoreDetails(int JobOpeningId)
        {
            try
            {
                param.Add("@p_JobOpeningId", JobOpeningId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_JobApplication_MoreDetails", param);
                ViewBag.GetJobOpeningMoreDetailsList = data;
                TempData["JobOpeningId"] = JobOpeningId;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Current Opening More Details Save Page
        [HttpPost]
        public ActionResult SaveUpdate(HttpPostedFileBase Image, Recruitment_Resume RecruitmentResume)
        {
            try
            {
                string filename = Image.FileName;
                var EmployeeId = Session["EmployeeId"];
                int JobOpeningId = Convert.ToInt32(TempData["JobOpeningId"]);
                param.Add("@p_process", string.IsNullOrEmpty(RecruitmentResume.ResumeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ResumeId", RecruitmentResume.ResumeId);
                param.Add("@p_ResumeId_Encrypted", RecruitmentResume.ResumeId_Encrypted);
                param.Add("@p_ResumeJobOpeningId", JobOpeningId);
                param.Add("@p_ResumeReferEmployeeId", Session["EmployeeId"]);
                param.Add("@p_CandidateName", RecruitmentResume.CandidateName);
                param.Add("@p_EmailId", RecruitmentResume.EmailId);
                param.Add("@p_MobileNo", RecruitmentResume.MobileNo);
                param.Add("@p_Gender", RecruitmentResume.Gender);
                param.Add("@p_TotalExperience", RecruitmentResume.TotalExperience);
                param.Add("@p_CTC", RecruitmentResume.CTC);
                param.Add("@p_CurrentCity", RecruitmentResume.CurrentCity);
                param.Add("@p_CurrentWorking", RecruitmentResume.CurrentWorking);


                var GetPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Resume'");
                var MoveLocation = GetPath.DocInitialPath;
                if (!Directory.Exists(MoveLocation))
                {
                    Directory.CreateDirectory(MoveLocation);
                }
                string path = "";
                path = MoveLocation + Session["EmployeeId"] + "_" + filename; //Save Image in this path

                param.Add("@p_ResumePath", path);
                param.Add("@p_ResumeStatus", RecruitmentResume.ResumeStatus);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Recruitment_Resume", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("CurrentOpening", "CurrentOpening");
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