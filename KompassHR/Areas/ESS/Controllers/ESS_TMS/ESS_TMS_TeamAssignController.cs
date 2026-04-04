using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_TeamAssignController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_TMS_TeamAssign
        #region ESS_TMS_TeamAssign
        public ActionResult ESS_TMS_TeamAssign(TMS_TeamAssign OBJTeam)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 483;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramClient = new DynamicParameters();
                paramClient.Add("@query", "SELECT ClientId as Id, ClientName as Name FROM TMS_Client WHERE Deactivate = 0 ORDER BY Name");
                var Client = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramClient).ToList();
                ViewBag.TMSClient = Client;

                bool IsAdmin = (bool)Session["IsAdmin"];
                if (IsAdmin == true)
                {
                    DynamicParameters paramMNG = new DynamicParameters();
                    paramMNG.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId in (Select BranchID from UserBranchMapping WHERE EmployeeID=" + Session["EmployeeId"] + " and IsActive=1)    and ContractorId=1 and EmployeeId<>1  order by Name");
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG).ToList();
                    ViewBag.TeamManagerList = data1;
                }
                else
                {
                    DynamicParameters paramMNG2 = new DynamicParameters();
                    paramMNG2.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId=" + Session["EmployeeId"] + "");
                    var data2 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG2).ToList();
                    ViewBag.TeamManagerList = data2;
                }
                ViewBag.GetTeamManagerList = "";
                if (OBJTeam.TeamManagerId != 0)
                {

                    DynamicParameters paramMNG = new DynamicParameters();
                    paramMNG.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and  EmployeeBranchId in (Select BranchID from UserBranchMapping WHERE EmployeeID=" + Session["EmployeeId"] + " and IsActive=1)    and ContractorId=1 and EmployeeId<>1  order by Name");
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG).ToList();
                    ViewBag.TeamEmployeeList = data1;
                }
                else
                {
                    ViewBag.TeamEmployeeList = "";
                }

                if (OBJTeam.ClientID > 0)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters paramProject = new DynamicParameters();
                    paramProject.Add("@p_employeeid", Session["EmployeeId"]);
                    paramProject.Add("@p_ClientId", OBJTeam.ClientID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", paramProject).ToList();
                    ViewBag.TMSProject = data;
                }
                else
                {
                    ViewBag.TMSProject = "";
                }

                if (OBJTeam.TeamAssignId > 0)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    DynamicParameters paramTeamEmployee = new DynamicParameters();
                    paramTeamEmployee.Add("@p_TeamManagerId", OBJTeam.TeamManagerId);
                    paramTeamEmployee.Add("@p_ClientID", OBJTeam.ClientID);
                    paramTeamEmployee.Add("@p_ProjectID", OBJTeam.ProjectID);
                    ViewBag.GetTeamEmployee = DapperORM.ReturnList<dynamic>("sp_List_TMS_TeamEmployeeList", paramTeamEmployee).ToList();
                }
                return View(OBJTeam);
            }


            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 483;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                // param.Add("@p_TeamAssignId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_TeamAssignId", "0");
                var data = DapperORM.DynamicList("sp_List_TMS_TeamAssign", param);
                ViewBag.GetTeamAssign = data;


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
        public JsonResult SaveUpdate(List<TMS_TeamAssign> TeamAssign)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();

                foreach (var Data in TeamAssign)
                {
                    // For existing records (TeamAssignId > 0)
                    if (Data.TeamAssignId > 0)
                    {
                        // Deactivate the existing record
                        strBuilder.Append($@"
                            UPDATE TMS_TeamAssign 
                            SET Deactivate = 1,
                                ModifiedDate = GETDATE(),
                                ModifiedBy = '{Session["EmployeeName"]}'
                            WHERE TeamAssignId = {Data.TeamAssignId};
                        ");

                        // Check if similar active record exists before inserting new one
                        strBuilder.Append($@"
                            IF NOT EXISTS (
                                SELECT 1 FROM TMS_TeamAssign 
                                WHERE TeamManagerId = {Data.TeamManagerId} 
                                AND TeamEmployeeId = {Data.TeamEmployeeId}
                                AND ClientId = {Data.ClientID} 
                                AND ProjectID = {Data.ProjectID}
                                AND Deactivate = 0
                            )
                            BEGIN
                                INSERT INTO TMS_TeamAssign (
                                    Deactivate,
                                    CreatedBy,
                                    CreatedDate,
                                    MachineName,
                                    TeamManagerId,
                                    TeamEmployeeId,
                                    ProjectID,
                                    ClientID,
                                    IsActive
                                ) VALUES (
                                    0,
                                    '{Session["EmployeeName"]}',
                                    GETDATE(),
                                    '{Dns.GetHostName()}',
                                    {Data.TeamManagerId},
                                    {Data.TeamEmployeeId},
                                    {Data.ProjectID},
                                    {Data.ClientID},
                                    '1'
                                )
                            END
                        ");
                    }
                    else // For new records
                    {
                        // Check if similar active record exists
                        strBuilder.Append($@"
                            IF EXISTS (
                                SELECT 1 FROM TMS_TeamAssign 
                                WHERE TeamManagerId = {Data.TeamManagerId} 
                                AND TeamEmployeeId = {Data.TeamEmployeeId}
                                AND ClientId = {Data.ClientID} 
                                AND ProjectID = {Data.ProjectID}
                                AND Deactivate = 0
                            )
                            BEGIN
                                -- Deactivate existing similar record
                                UPDATE TMS_TeamAssign  
                                SET Deactivate = 1,
                                    ModifiedDate = GETDATE(),
                                    ModifiedBy = '{Session["EmployeeName"]}'
                                WHERE TeamManagerId = {Data.TeamManagerId} 
                                AND TeamEmployeeId = {Data.TeamEmployeeId}
                                AND ClientId = {Data.ClientID} 
                                AND ProjectID = {Data.ProjectID}
                                AND Deactivate = 0
                            END

                            -- Insert new record
                            INSERT INTO TMS_TeamAssign (
                                Deactivate,
                                CreatedBy,
                                CreatedDate,
                                MachineName,
                                TeamManagerId,
                                TeamEmployeeId,
                                ProjectID,
                                ClientID,
                                IsActive
                            ) VALUES (
                                0,
                                '{Session["EmployeeName"]}',
                                GETDATE(),
                                '{Dns.GetHostName()}',
                                {Data.TeamManagerId},
                                {Data.TeamEmployeeId},
                                {Data.ProjectID},
                                {Data.ClientID},
                                '1'
                            )
                        ");
                    }
                }

                string errorMessage = "";
                if (objcon.SaveStringBuilder(strBuilder, out errorMessage))
                {
                    TempData["Message"] = "Records saved successfully";
                    TempData["Icon"] = "success";
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    DapperORM.DynamicQuerySingle($@"
                        INSERT INTO Tool_ErrorLog (
                            Error_Desc,
                            Error_FormName,
                            Error_MachinceName,
                            Error_Date,
                            Error_UserID,
                            Error_UserName
                        ) VALUES (
                            '{strBuilder}',
                            'TeamAssignUpdate',
                            '{Dns.GetHostName()}',
                            GETDATE(),
                            '{Session["EmployeeId"]}',
                            '{Session["EmployeeName"]}'
                        )");

                    TempData["Message"] = errorMessage;
                    TempData["Icon"] = "error";
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region GetProject
        [HttpGet]
        public ActionResult GetProject(int Clientd)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_ClientId", Clientd);

                var Project = DapperORM.ReturnList<AllDropDownBind>("sp_Get_TMS_ProjectDropdown", param).ToList();
                return Json(new { Project = Project }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetTeamEmployee
        [HttpGet]
        public ActionResult GetTeamEmployee(int TeamManagerId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                bool IsAdmin = (bool)Session["IsAdmin"];
                List<AllDropDownBind> TeamEmployee;

                if (IsAdmin == true)
                {
                    DynamicParameters paramMNG = new DynamicParameters();
                    paramMNG.Add("@query", "SELECT EmployeeId as Id, CONCAT(EmployeeName, ' - ', EmployeeNo) as Name FROM Mas_Employee WHERE Deactivate=0 AND employeeLeft=0 AND EmployeeBranchId IN (SELECT BranchID FROM UserBranchMapping WHERE EmployeeID=" + TeamManagerId + " AND IsActive=1) AND ContractorId=1 AND EmployeeId<>1 ORDER BY Name");
                    TeamEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG).ToList();
                }
                else
                {
                    DynamicParameters paramMNG2 = new DynamicParameters();
                    paramMNG2.Add("@query", "SELECT EmployeeId as Id, CONCAT(EmployeeName, ' - ', EmployeeNo) as Name FROM Mas_Employee WHERE Deactivate=0 AND EmployeeId=" + TeamManagerId + "");
                    TeamEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramMNG2).ToList();
                }

                return Json(new { TeamEmployee = TeamEmployee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Employee details
        [HttpGet]
        public ActionResult ShowEmployeeList(int? TeamManagerId, int? ClientID, int? ProjectID)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_TeamManagerId", TeamManagerId);
            param.Add("@p_ClientID", ClientID);
            param.Add("@p_ProjectID", ProjectID);
            var data = DapperORM.ReturnList<dynamic>("sp_List_TMS_TeamEmployeeList", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete
        public ActionResult Delete(double? TeamManagerId, int ClientId,int ProjectId,int TeamEmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                var machineName = Dns.GetHostName().Replace("'", "''");
                strBuilder.Append("UPDATE dbo.TMS_TeamAssign " +
                    "SET Deactivate = 1, " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE TeamManagerId = '" + TeamManagerId + "' and ClientID='"+ ClientId + "' and ProjectID='"+ ProjectId + "' and TeamEmployeeId='"+ TeamEmployeeId + "' ;");
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Deleted successfully.";
                    TempData["Icon"] = "success";
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
    }
}