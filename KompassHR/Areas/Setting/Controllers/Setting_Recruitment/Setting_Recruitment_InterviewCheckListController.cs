using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
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
using Z.Dapper.Plus;

namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_InterviewCheckListController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Recruitment_InterviewCheckList
        #region InterviewCheckList
        public ActionResult Setting_Recruitment_InterviewCheckList(int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 36;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                var results = DapperORM.DynamicQueryMultiple(@"SELECT  DepartmentId as Id,DepartmentName as Name FROM Mas_Department WHERE Deactivate =0;
                                                     SELECT DesignationId  as Id, DesignationName as Name FROM Mas_Designation WHERE Deactivate =0
                                                     SELECT GradeId as Id,GradeName as Name FROM Mas_Grade WHERE Deactivate =0                                                   	 
                                                     select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate= 0 ");

                //ViewBag.DepatmentList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DesignationList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GradeList = results.Read<AllDropDownClass>().ToList();
                //ViewBag.ComapnyName = results.Read<AllDropDownClass>().ToList();
                ViewBag.DepatmentList = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationList = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeList = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.ComapnyName = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();



                if (CmpId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", CmpId);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    ViewBag.BranchName = BranchName;
                }
                else
                {
                    ViewBag.BranchName = "";
                }
                return View();
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<RecruitmentRecoardList> RecruitmentRecoardList,
                               int? CmpId, int? BranchId, int? DesignationId,
                               int? DepartmentId, int? GradeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // 🔎 check duplicate checklist
                var checkQuery = @"SELECT COUNT(*) 
                           FROM Recruitment_CheckList 
                           WHERE CmpId = @CmpId 
                             AND CheckListBranchId = @BranchId 
                             AND DepartmentID = @DepartmentId 
                             AND DesignationID = @DesignationId 
                             AND GradeID = @GradeId 
                             AND Deactivate = 0";

                var paramCheck = new DynamicParameters();
                paramCheck.Add("@CmpId", CmpId);
                paramCheck.Add("@BranchId", BranchId);
                paramCheck.Add("@DepartmentId", DepartmentId);
                paramCheck.Add("@DesignationId", DesignationId);
                paramCheck.Add("@GradeId", GradeId);

                using (var connection = new SqlConnection(DapperORM.connectionString))
                {
                    int count = connection.QuerySingle<int>(checkQuery, paramCheck);

                    if (count > 0)
                    {
                        return Json(new { Message = "This CheckList already exists", Icon = "error" }, JsonRequestBehavior.AllowGet);
                    }
                }


                if (RecruitmentRecoardList != null && RecruitmentRecoardList.Count > 0)
                {
                    using (var connection = new SqlConnection(DapperORM.connectionString))
                    {
                        string insertSql = @"
                    INSERT INTO Recruitment_CheckList
                        (Deactivate, CreatedBy, CreatedDate, MachineName, 
                         CmpId, CheckListBranchId, DesignationID, DepartmentID, GradeId, 
                         Origin, Rate, Remarks)
                    VALUES
                        (0, @CreatedBy, GETDATE(), @MachineName,
                         @CmpId, @BranchId, @DesignationId, @DepartmentId, @GradeId,
                         'Interview CheckList', @Rate, @Remarks)";

                        var data = RecruitmentRecoardList.Select(r => new
                        {
                            CreatedBy = Session["EmployeeName"]?.ToString(),
                            MachineName = Dns.GetHostName().ToString(),
                            CmpId,
                            BranchId,
                            DesignationId,
                            DepartmentId,
                            GradeId,
                            Rate = r.Rate,
                            Remarks = r.Remarks
                        }).ToList();

                        int rowsAffected = connection.Execute(insertSql, data);

                        if (rowsAffected > 0)
                        {
                            return Json(new { Message = "Record saved successfully", Icon = "success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { Message = "No records inserted!", Icon = "error" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return Json(new { Message = "No data to insert", Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // log the error
                DapperORM.Execute("INSERT INTO Tool_ErrorLog (Error_Desc, Error_FormName, Error_MachinceName, Error_Date, Error_UserID, Error_UserName) " +
                                  "VALUES (@Error_Desc, 'SaveUpdate', @Error_MachinceName, GETDATE(), @Error_UserID, @Error_UserName)",
                    new
                    {
                        Error_Desc = ex.Message,
                        Error_MachinceName = Dns.GetHostName().ToString(),
                        Error_UserID = Session["EmployeeId"],
                        Error_UserName = Session["EmployeeName"]
                    });

                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 36;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CheckListID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Recruitment_CheckList", param).ToList();
                ViewBag.TaxRateMasterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete With 
        [HttpGet]
        public ActionResult Delete(int? CmpId, int? CheckListBranchId, int? DepartmentID, int? DesignationID, int? GradeID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DapperORM.DynamicQuerySingle("Update Recruitment_CheckList set Deactivate=1 ,ModifiedBy='" + Session["EmployeeId"] + "',ModifiedDate=GetDate() ,MachineName='" + Dns.GetHostName().ToString() + "' where  CmpId=" + CmpId + " and CheckListBranchId=" + CheckListBranchId + " and DepartmentID=" + DepartmentID + " and DesignationID=" + DesignationID + " and GradeID=" + GradeID + "");
                TempData["Message"] = "Record delete successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("GetList", "Setting_Recruitment_InterviewCheckList");
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