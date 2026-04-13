using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_TeamGoalCreationController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_PMS_TeamGoalCreation

        [HttpGet]
        public ActionResult ESS_PMS_TeamGoalCreation(string OwnObjectiveCreationId_Encrypted)
        {
            try
            {
                DynamicParameters param1 = new DynamicParameters();
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 602;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                int employeeDepartmentId = Convert.ToInt32(Session["EmpDepartment"] ?? 0);
                Session["EssDeptId"] = employeeDepartmentId;

                bool isHRorAdmin = employeeDepartmentId == 13;
                ViewBag.AddUpdateTitle = "Add";
                DynamicParameters param7 = new DynamicParameters();
                param7.Add("@query", "select UOM as Name from PMS_UOM where Deactivate=0");
                var UOMList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param7).ToList();
                ViewBag.UOMList = UOMList;

                if (isHRorAdmin)
                {
                    param1.Add("@query", "SELECT DepartmentId as Id, DepartmentName as Name FROM Mas_Department WHERE Deactivate = 0");
                    var DepartmentList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.DepartmentList = DepartmentList;

                    var param2 = new DynamicParameters();
                    param2.Add("@query", "Select ObjectiveId as Id, ObjectiveTitle as Name from PMS_Objectives where Deactivate = 0 and origin='Team'");
                    var ObjectiveList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.ObjectiveList = ObjectiveList;
                    ViewBag.AddUpdateTitle = "Add";

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT EmployeeId AS Id, CONCAT(EmployeeName, ' - ', EmployeeNo) AS Name FROM Mas_Employee WHERE Deactivate = 0 AND ContractorId = 1 AND EmployeeLeft = 0 ORDER BY Name");
                    var EmpolyeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.GetEmpolyeeName = EmpolyeeName;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "select distinct(Category) as Name from PMS_KPIMaster where Deactivate=0");
                    var CategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    ViewBag.CategoryList = CategoryList;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "SELECT KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0");
                    var KPIList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                    ViewBag.KPIList = KPIList;

                    //DynamicParameters param5 = new DynamicParameters();
                    //param5.Add("@query", "select TargetTypeId AS Id,TargetType AS Name from PMS_TargetType where Deactivate=0");
                    //var TragetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    //ViewBag.TragetTypeList = TragetTypeList;

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@query", "select TargetTypeId AS Id,TargetType AS Name from PMS_TargetType where Deactivate=0");
                    var TragetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                    ViewBag.TragetTypeList = TragetTypeList;
                }
                else
                {
                    var DId = employeeDepartmentId;
                    param1.Add("@query", "SELECT DepartmentId as Id, DepartmentName as Name FROM Mas_Department WHERE Deactivate = 0");
                    var DepartmentList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.DepartmentList = DepartmentList;

                    var param2 = new DynamicParameters();
                    param2.Add("@query", "Select ObjectiveId as Id, ObjectiveTitle as Name from PMS_Objectives where Deactivate = 0 and origin!='Individual'");
                    var ObjectiveList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.ObjectiveList = ObjectiveList;
                    ViewBag.AddUpdateTitle = "Add";

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "SELECT EmployeeId AS Id, CONCAT(EmployeeName, ' - ', EmployeeNo) AS Name FROM Mas_Employee WHERE Deactivate = 0 AND ContractorId = 1 AND EmployeeLeft = 0 and EmployeeDepartmentID='" + DId + "' ORDER BY Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.GetEmpolyeeName = EmployeeName;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@query", "select distinct(Category) as Name from PMS_KPIMaster where Deactivate=0");
                    var CategoryList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    ViewBag.CategoryList = CategoryList;

                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "SELECT KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0");
                    var KPIList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                    ViewBag.KPIList = KPIList;

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@query", "select TargetTypeId AS Id,TargetType AS Name from PMS_TargetType where Deactivate=0");
                    var TragetTypeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param6).ToList();
                    ViewBag.TragetTypeList = TragetTypeList;
                }
                PMS_OwnObjectiveCreation PMS_OwnObjectiveCreation = new PMS_OwnObjectiveCreation();

                if (OwnObjectiveCreationId_Encrypted != null)
                {
                    DynamicParameters paramup = new DynamicParameters();

                    ViewBag.AddUpdateTitle = "Update";
                    paramup.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                    PMS_OwnObjectiveCreation = DapperORM.ReturnList<PMS_OwnObjectiveCreation>("sp_List_PMS_TeamObjectiveCreation", paramup).FirstOrDefault();

                    TempData["Employeeid"] = PMS_OwnObjectiveCreation.EmployeeId;
                    TempData["p_Id"] = PMS_OwnObjectiveCreation.OwnObjectiveCreationId;
                }
                return View(PMS_OwnObjectiveCreation);
            }
            catch (Exception ex)

            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUpdate(PMS_OwnObjectiveCreation teamObjective, string SmartPrinciple, string EmployeeId)
        {
            if (Session["EmployeeId"] == null)
            {
                TempData["Message"] = "Session expired. Please log in again.";
                TempData["Icon"] = "error";
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            try
            {
                string process = string.IsNullOrEmpty(teamObjective.OwnObjectiveCreationId_Encrypted) ? "Save" : "Update";              
                DynamicParameters param12 = new DynamicParameters();
                param12.Add("@p_process", process);
                param12.Add("@p_OwnObjectiveCreationId", teamObjective.OwnObjectiveCreationId);
                param12.Add("@p_OwnObjectiveCreationId_Encrypted", teamObjective.OwnObjectiveCreationId_Encrypted);
                param12.Add("@p_DepartmentId", teamObjective.DepartmentId);
                string employeeCsv = teamObjective.EmployeeIds != null
                    ? string.Join(",", teamObjective.EmployeeIds)
                    : "";
                TempData["Message2"] = employeeCsv;
                param12.Add("@p_EmployeeId", employeeCsv);
                param12.Add("@p_IsAlign", teamObjective.IsAlign);
                if (teamObjective.IsAlign == true && teamObjective.AlignWithType == "Company")
                {
                    param12.Add("@p_AlignWithType", "Company");
                    param12.Add("@p_AlignWith", teamObjective.ObjectiveId);

                    var query = @"SELECT ObjectiveId FROM PMS_CompanyObjectiveCreation WHERE CompanyObjectiveCreationId = '"+ teamObjective.ObjectiveId + "'";
                    var ObjId = DapperORM.DynamicQuerySingle(query);
                    int ObjId1 = Convert.ToInt32(ObjId.ObjectiveId);
                    param12.Add("@p_ObjectiveId",ObjId1);
                }
                else
                {
                    param12.Add("@p_AlignWithType", null);
                    param12.Add("@p_AlignWith", null);
                    param12.Add("@p_ObjectiveId", teamObjective.ObjectiveId);
                }
                param12.Add("@p_KRA", teamObjective.KRA);
                param12.Add("@p_SmartPrinciple", SmartPrinciple);
                param12.Add("@p_UOM", teamObjective.UOM);
                param12.Add("@p_Target", teamObjective.Target);
                param12.Add("@p_TargetType", teamObjective.TargetType);
                param12.Add("@p_StartDate", teamObjective.StartDate);
                param12.Add("@p_EndDate", teamObjective.EndDate);
                param12.Add("@p_KPICategory", teamObjective.KPICategory);
                param12.Add("@p_KPINameId", teamObjective.KPINameId);
                param12.Add("@p_Weightage", teamObjective.Weightage);
                param12.Add("@p_Visibility", teamObjective.Visibility);
                param12.Add("@p_Description", teamObjective.Description);
                param12.Add("@p_Attachment", teamObjective.Attachment);
                param12.Add("@p_Origin", "Team");
                param12.Add("@p_CmpId", Session["CompanyId"]);
                param12.Add("@p_BranchId", Session["BranchId"]);
                param12.Add("@p_Status", null);
                param12.Add("@p_ProgressPercentage", null);
                param12.Add("@p_ProgressStatus", null);
                param12.Add("@p_PMS_YearId", null);
                param12.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param12.Add("@p_MachineName", Dns.GetHostName());
                param12.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                param12.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);
                param12.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_OwnObjectiveCreation", param12);
                TempData["Message"] = param12.Get<string>("@p_msg");
                TempData["Icon"] = param12.Get<string>("@p_Icon");
                TempData["P_Id"] = param12.Get<string>("@p_Id");
                return RedirectToAction("ESS_PMS_TeamGoalCreation", "ESS_PMS_TeamGoalCreation");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error";
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_TeamGoalCreation", new { area = "ESS" });
            }
        }

        //public ActionResult OriginWiseObjective(string Origin)
        //{

        //    if (Session["EmployeeId"] == null)
        //    {
        //        return RedirectToAction("Login", "Login", new { Area = "" });
        //    }
        //    if(Origin!="")
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "Select ObjectiveId as Id,ObjectiveTitle as Name from PMS_Objectives where Deactivate=0 and origin='" + Origin + "'");
        //        var ObjectiveList = DapperORM.ReturnList<PMS_Objectives>("sp_QueryExcution", param).ToList();
        //        return Json(ObjectiveList, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        var param2 = new DynamicParameters();
        //        param2.Add("@query", "Select ObjectiveId as Id, ObjectiveTitle as Name from PMS_Objectives where Deactivate = 0 and origin!='Individual'");
        //        var ObjectiveList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
        //        return Json(ObjectiveList, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult OriginWiseObjective(string Origin)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            DynamicParameters param = new DynamicParameters();
            string query = "";

            if (Origin == "Company")
            {
                query = @"
        SELECT 
            C.CompanyObjectiveCreationId AS Id,   -- 🔥 THIS IS ALIGNWITH VALUE
            O.ObjectiveTitle AS Name,
            C.ObjectiveId                   -- 🔥 THIS WILL BE SAVED IN OBJECTIVEID
        FROM PMS_CompanyObjectiveCreation C
        INNER JOIN PMS_Objectives O 
            ON C.ObjectiveId = O.ObjectiveId
        WHERE C.Deactivate = 0";
            }
            else
            {
                query = @"
        SELECT ObjectiveId as Id, ObjectiveTitle as Name
        FROM PMS_Objectives
        WHERE Deactivate = 0
        AND Origin = 'Team'";
            }

            param.Add("@query", query);

            var list = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult IsObjectiveExist(string ObjectiveTitle, string Origin)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_ObjectiveTitle", ObjectiveTitle);
                    param.Add("@p_Origin", Origin);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Objectives", param);
                    var Message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult CategoryWiseKPI(string Category)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select KPIId as Id,KPIName as Name FROM PMS_KPIMaster WHERE Deactivate=0 and Category='" + Category + "'");
            var KPINameList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
            return Json(KPINameList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KPIWiseDescription(int KPIID)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@query", "Select Description FROM PMS_KPIMaster WHERE Deactivate=0 and KPIId='" + KPIID + "'");
            var KPINameList = DapperORM.ReturnList<PMS_KPIMaster>("sp_QueryExcution", param).FirstOrDefault();
            return Json(KPINameList.Description, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveObjective(PMS_Objectives PMS_Objectives)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    TempData["Message"] = "Your session has expired. Please log in again.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    string process = string.IsNullOrEmpty(PMS_Objectives.ObjectiveId_Encrypted) ? "Save" : "Update";
                    var param = new DynamicParameters();
                    param.Add("@p_process", process);
                    param.Add("@p_ObjectiveId_Encrypted", PMS_Objectives.ObjectiveId_Encrypted);
                    param.Add("@p_ObjectiveTitle", PMS_Objectives.ObjectiveTitle);
                    param.Add("@p_Origin", PMS_Objectives.Origin);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    param.Add("@p_CmpId", Session["CmpId"]?.ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]?.ToString());
                    param.Add("@p_MachineName", System.Net.Dns.GetHostName());
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_Objectives", param);

                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    TempData["P_Id"] = param.Get<string>("@p_Id");

                    return RedirectToAction("ESS_PMS_TeamGoalCreation", new { area = "ESS" });
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while saving the objective: " + ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_TeamGoalCreation", new { area = "ESS" });
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
                DynamicParameters paramlist = new DynamicParameters();

                paramlist.Add("@p_OwnObjectiveCreationId_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_PMS_TeamObjectiveCreation", paramlist);
                ViewBag.GetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult DepartmentWiseEmployee(int ddlDepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "Select EmployeeId as Id,EmployeeName as Name from MAS_EMPLOYEE where Deactivate=0 and EmployeeLeft=0 and EmployeeDepartmentID='" + ddlDepartmentId + "'");
                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                return Json(EmployeeList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "An error occurred: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete(string OwnObjectiveCreationId_Encrypted)
        {
            try
            {
                DynamicParameters paramdel = new DynamicParameters();

                paramdel.Add("@p_process", "Delete");
                paramdel.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
                paramdel.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                paramdel.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_PMS_OwnObjectiveCreation", paramdel);
                TempData["Message"] = paramdel.Get<string>("@p_msg");
                TempData["Icon"] = paramdel.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_PMS_TeamGoalCreation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetStandardKRA(int? DepartmentId)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_OwnObjectiveCreationId_Encrypted", "StandardKRAList");
            param.Add("@p_DepartmentId", DepartmentId);
            //var data = DapperORM.ReturnList<PMS_OwnObjectiveCreation>("sp_List_PMS_OwnObjectiveCreation", param);
            var data=DapperORM.DynamicList("sp_List_PMS_OwnObjectiveCreation", param);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStandardKRADetails(string OwnObjectiveCreationId_Encrypted)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_OwnObjectiveCreationId_Encrypted", OwnObjectiveCreationId_Encrypted);
            var data = DapperORM.DynamicList("sp_List_PMS_TeamObjectiveCreation", param);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        


    }
}