using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Recruitment;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;


namespace KompassHR.Areas.Setting.Controllers.Setting_Recruitment
{
    public class Setting_Recruitment_TeamAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        // GET: Setting/Setting_Recruitment_TeamAssign
        #region Setting_Recruitment_TeamAssign
        public ActionResult Setting_Recruitment_TeamAssign()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 541;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetEmployee = new BulkAccessClass().AllEmployeeName();
                ViewBag.AllEmployeeName = GetEmployee;

                DynamicParameters HeadEmployee = new DynamicParameters();
                HeadEmployee.Add("@query", "SELECT     Recruitment_Assign.RecruitmentAssignEmployeeID AS Id,    CONCAT(Mas_Employee.EmployeeName, ' - ', Mas_Employee.EmployeeNo) AS Name  FROM     Recruitment_Assign LEFT JOIN     Mas_Employee ON Recruitment_Assign.RecruitmentAssignEmployeeID = Mas_Employee.EmployeeID WHERE     Mas_Employee.Deactivate = 0    AND Recruitment_Assign.Deactivate = 0    AND Recruitment_Assign.IsActive = 1 GROUP BY     Recruitment_Assign.RecruitmentAssignEmployeeID,    Mas_Employee.EmployeeName,    Mas_Employee.EmployeeNo");
                var Head = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", HeadEmployee).ToList();
                ViewBag.HeadEmployee = Head;

                Recruitment_TeamAssign Recruitment_TeamAssign = new Recruitment_TeamAssign();

                DynamicParameters paramTeam = new DynamicParameters();
                paramTeam.Add("@query", "select ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS SrNo, TeamAssignEmployeeId as Answer from Recruitment_TeamAssign where RecruitmentHeadEmployeeId=" + Recruitment_TeamAssign.RecruitmentHeadEmployeeId + " and Deactivate = 0");
                var TeamAssign = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramTeam).ToList();
                ViewBag.TeamAssign = TeamAssign;

                //if (FNFNoDuesId_Encrypted != null)
                //{
                //    //param = new DynamicParameters();
                //    //param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                //    //fnf_NoDuesCheckList = DapperORM.ReturnList<FNF_NoDuesCheckList>("sp_List_FNF_NoDuesCheckList", param).FirstOrDefault();
                //    ViewBag.AddUpdateTitle = "Update";
                //    param = new DynamicParameters();
                //    param.Add("@P_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                //    fnf_NoDuesCheckList = DapperORM.ReturnList<FNF_NoDuesCheckList>("sp_List_FNF_NoDuesCheckList", param).FirstOrDefault();

                //    DynamicParameters paramOption = new DynamicParameters();
                //    paramOption.Add("@query", "select ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS SrNo, RequiredClearanceName as Answer from FNF_DuesAndClearence_Details where NoDuesAndClearenceTitle_MasterID = " + fnf_NoDuesCheckList.FNFNoDuesId + " and Deactivate = 0");
                //    var List_Options = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                //    ViewBag.GetOptions = List_Options;

                //}
                //return View(fnf_NoDuesCheckList);
                return View();
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
        public ActionResult SaveUpdate(List<Recruitment_TeamAssign_Employee> Employee, int? Head)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (Head != 0)
                {
                    DapperORM.DynamicQuerySingle("update Recruitment_TeamAssign set Deactivate=1 where RecruitmentHeadEmployeeId=" + Head + " ");
                }

                for (var i = 0; i < Employee.Count; i++)
                {
                    param.Add("@p_process", "Save");
                    param.Add("@P_RecruitmentHeadEmployeeId", Head);
                    param.Add("@P_RecruitmentAssignedEmployeeId", Employee[i].EmployeeId);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Recruitment_TeamAssign", param);
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

        #region IsValidaton
        [HttpGet]
        public ActionResult IsExists(int Head, string Recruitment_TeamAssignID_Encrypted)
        {
            try
            {
                //param.Add("@p_process", "IsValidation");
                //param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                //param.Add("@p_NoDuesCheckListName", ClearanceTitle);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                //var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoDuesCheckList", param);
                //var Message = param.Get<string>("@p_msg");
                //var Icon = param.Get<string>("@p_Icon");
                var Message = "";
                var Icon = "";
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


        #region GetTeam
        public ActionResult GetTeam(int? EmployeeId)
        {
            try
            {
                var data = DapperORM.DynamicQueryList("Select concat( Mas_Employee.EmployeeName,' - ', Mas_Employee.EmployeeNo) as Name,Recruitment_TeamAssign.TeamAssignEmployeeId from Recruitment_TeamAssign,Mas_Employee WHERE RecruitmentHeadEmployeeId=" + EmployeeId + " and Mas_Employee.EmployeeId=Recruitment_TeamAssign.TeamAssignEmployeeId and Recruitment_TeamAssign.Deactivate=0");
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