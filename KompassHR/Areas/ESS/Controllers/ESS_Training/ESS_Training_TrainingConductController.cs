using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Training;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Training
{
    public class ESS_Training_TrainingConductController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        private object parsedDate;

        //[HttpGet]
        #region Training Conduct View 
        public ActionResult ESS_Training_TrainingConduct(int? TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 559;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Training_TrainingPlan TrainingConduct = new Training_TrainingPlan();
                ViewBag.AddUpdateTitle = "Add";
                ViewBag.GetCalenderAssignedEmpolyeeList = "";
                ViewBag.Batch = "";
                DynamicParameters paramtrainingCalender = new DynamicParameters();
                var GetTrainingCalender = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingCalenderDropdown", paramtrainingCalender).ToList();
                var CID = GetTrainingCalender.FirstOrDefault()?.Id;
                ViewBag.TrainingCalender = GetTrainingCalender;
                ViewBag.GetTrainingPlan = "";
                return View(TrainingConduct);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Training Plans
        [HttpGet]
        public ActionResult GetTrainingPlan(int? CalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramtrainingPlan = new DynamicParameters();
                paramtrainingPlan.Add("@P_TrainingCalenderId", CalenderId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetTrainingPlanDropdown", paramtrainingPlan).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Batch
        [HttpGet]
        public ActionResult GetBatch(int? PlanId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramtrainingBatch = new DynamicParameters();
                paramtrainingBatch.Add("@P_TrainingPlan_MasterId", PlanId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBatchDropdown", paramtrainingBatch).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get Employee
        [HttpGet]
        public ActionResult GetEmployeeDetails(int? TrainingCalenderId, string TrainingPlanId, string Batch)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }

            List<Training_TrainingPlan> data = new List<Training_TrainingPlan>();

            if (TrainingCalenderId != null)
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TrainingCalenderId", TrainingCalenderId);
                param.Add("@p_TrainingPlanId", TrainingPlanId);
                param.Add("@p_Batch", Batch);
                data = DapperORM.ReturnList<Training_TrainingPlan>("sp_TrainingConduct_GetEmployeeList", param).ToList();
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
      
        #region SaveUpadte
        [HttpPost]
        public ActionResult SaveUpadte(List<Training_TrainingPlan> TrainingConductEmployeeList,int TrainingPlanId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 558;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var param = new DynamicParameters();
                 StringBuilder strBuilder = new StringBuilder();
                string qry = "";
                for (var i = 0; i < TrainingConductEmployeeList.Count; i++)
                {
                    var employeeId = TrainingConductEmployeeList[i].EmployeeId;
                    bool IsTrainingConduct = TrainingConductEmployeeList[i].IsTrainingConduct;
                    var createdBy = Session["EmployeeName"]?.ToString().Replace("'", "''");
                    var machineName = Dns.GetHostName().Replace("'", "''");
                    qry = "UPDATE dbo.TrainingPlan_Details " +
                             "SET IsTrainingConduct = '" + IsTrainingConduct + "', " +  
                             "ModifiedBy = '" + createdBy + "', " +
                             "ModifiedDate = GETDATE(), " +
                             "MachineName = '" + machineName + "' " +
                             "WHERE EmployeeId = '" + employeeId + "' AND TrainingPlan_MasterId = '" + TrainingPlanId + "';";

                    strBuilder.Append(qry);
                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record save successfully";
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

        #region GetList
        [HttpGet]
        public ActionResult GetList(string TrainingCalenderId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 559;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainingCalenderId", TrainingCalenderId);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingConduct", param).ToList();
                ViewBag.GetTrainingtPlan = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetTraining Conduct List
        [HttpGet]
        public ActionResult GetTrainingConductList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 559;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainingCalenderId", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingConduct", param).ToList();
                ViewBag.GetTrainingtconduct = data;
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
        public ActionResult Delete(double? TrainingPlan_MasterId)
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
                strBuilder.AppendLine("UPDATE dbo.TrainingPlan_Master " +
                    "SET Deactivate = 1, " +
                    "ModifiedBy = '" + createdBy + "', " +
                    "ModifiedDate = GETDATE(), " +
                    "MachineName = '" + machineName + "' " +
                    "WHERE TrainingPlan_MasterId = '" + TrainingPlan_MasterId + "';");

                strBuilder.AppendLine("UPDATE dbo.TrainingPlan_Details " +
                    "SET Deactivate = 1 " +
                    "WHERE TrainingPlan_MasterId = '" + TrainingPlan_MasterId + "';");


                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Deleted successfully.";
                    TempData["Icon"] = "success";
                }
                return RedirectToAction("GetTrainingConductList", "ESS_Training_TrainingConduct");
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
        public ActionResult ShowEmployeeList(int? TrainingCalenderId, string Batch)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_TrainingCalenderId", TrainingCalenderId);
            param.Add("@p_Batch", Batch);
            var data = DapperORM.ReturnList<dynamic>("sp_ShowTrainingConductEmployeeList", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion
     
        #region GetTraining Pending List
        [HttpGet]
        public ActionResult GetTrainingPendingList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 559;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@P_TrainingCalenderId", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TrainingPending", param).ToList();
                ViewBag.GetTrainingtpending = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion


        #region pending Employee details
        [HttpGet]
        public ActionResult ShowPendingEmployeeList(int? TrainingCalenderId, string Batch)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_TrainingCalenderId", TrainingCalenderId);
            param.Add("@p_Batch", Batch);
            var data = DapperORM.ReturnList<dynamic>("sp_ShowTrainingPendingEmployeeList", param).ToList();
            string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}