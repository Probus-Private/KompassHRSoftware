using Dapper;
using KompassHR.Areas.ESS.Models.ESS_PMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_PMS
{
    public class ESS_PMS_HRMappingController : Controller
    {
        // GET: ESS/ESS_PMS_HRMapping
        public ActionResult ESS_PMS_HRMapping()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 866;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;

                DynamicParameters paramBUName = new DynamicParameters();
                paramBUName.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch Where Deactivate=0;");
                ViewBag.BUName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBUName).ToList();

                DynamicParameters paramDept = new DynamicParameters();
                paramDept.Add("Query", $"select DepartmentId as Id,DepartmentName as Name from mas_Department where deactivate=0");
                ViewBag.GetDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDept).ToList();

                DynamicParameters paramDesg = new DynamicParameters();
                paramDesg.Add("Query", $"select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0");
                ViewBag.GetDesg = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramDesg).ToList();

                DynamicParameters paramInfo = new DynamicParameters();
                paramInfo.Add("Query", $"SELECT EmployeeId as Id,EmployeeName as Name FROM Mas_Employee WHERE Deactivate=0 AND employeeleft=0");
                ViewBag.GetEmployeeInfo = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramInfo).ToList();


                ViewBag.AddUpdateTitle = "Add";
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }
         
        }

        public ActionResult GetList()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw;
            }     
        }

        public ActionResult GetBranchByCompany(int CmpId)
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

                throw;
            }
        }

        //public ActionResult GetDesignationByDepartment(int DeptId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@p_employeeid", Session["EmployeeId"]);
        //        param.Add("@p_DeptId", DeptId);
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        public ActionResult GetEmployees(int CmpId, int BranchId, int? DeptId, int? DesgId, long ResponsibleHR)
        {
            if (Session["EmployeeId"] == null)
                return RedirectToAction("Login", "Login", new { area = "" });

            /* 1️⃣ Get mapped employee IDs for selected HR */
            DynamicParameters mapParam = new DynamicParameters();
            mapParam.Add("@CmpID", CmpId);
            mapParam.Add("@BranchId", BranchId);
            mapParam.Add("@ResponsibleHR", ResponsibleHR);
            var mappedResult = (IEnumerable<dynamic>)DapperORM.DynamicList(
    "sp_Get_PMS_HR_MappedEmployees",
    mapParam
);
            var mappedIds = mappedResult
    .Select(x => Convert.ToInt64(x.EmployeeId))
    .ToList();

            //var mappedIds = DapperORM
            //    .DynamicList("sp_Get_PMS_HR_MappedEmployees", mapParam)
            //    .Select(x => Convert.ToInt32(x.EmployeeId))
            //    .ToList();
            /* 2️⃣ Load ALL employees condition-wise (COMMON SP) */
            string qry = "";
            qry += " AND Mas_Employee.CmpID = " + CmpId;
            qry += " AND Mas_Employee.EmployeeBranchId = " + BranchId;

            if (DeptId.HasValue)
                qry += " AND Mas_Department.DepartmentId = " + DeptId.Value;

            if (DesgId.HasValue)
                qry += " AND Mas_Employee.EmployeeDesignationID = " + DesgId.Value;

            DynamicParameters empParam = new DynamicParameters();
            empParam.Add("@p_Qry", qry);

            var employees = DapperORM
                .ReturnList<dynamic>("sp_GetEmployeeList", empParam)
                .ToList();

            /* 3️⃣ Merge → tick only mapped */
            var finalData = new List<object>();

            foreach (var item in employees)
            {
                var row = (IDictionary<string, object>)item;
                long empId = Convert.ToInt64(row["EmployeeId"]);

                finalData.Add(new
                {
                    EmployeeId = empId,
                    EmployeeName = row["EmployeeName"],
                    DepartmentName = row["DepartmentName"],
                    DesignationName = row["DesignationName"],
                    IsChecked = mappedIds.Contains(empId)
                });
            }

            return Json(finalData, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public ActionResult SaveHRMapping(PMS_HRMapping model)
        //{
        //    try
        //    {
        //        string[] empIds = model.SelectedEmployeeIds?.Split(',');

        //        if (empIds == null || empIds.Length == 0)
        //        {
        //            TempData["Message"] = "Please select at least one employee";
        //            TempData["Icon"] = "error";
        //            return RedirectToAction("GetList");
        //        }

        //        foreach (string empId in empIds)
        //        {
        //            var param = new DynamicParameters();

        //            param.Add("@p_process", string.IsNullOrEmpty(model.PmshrmappingId_Encrypted) ? "Save" : "Update");
        //            param.Add("@p_MachineName", Dns.GetHostName());
        //            param.Add("@p_CreatedBy", Session["EmployeeName"]);
        //            param.Add("@p_CmpID", model.CmpID);
        //            param.Add("@p_BranchId", model.BranchId);
        //            param.Add("@p_DepartmentId", model.DepartmentId);
        //            param.Add("@p_DesignationId", model.DesignationId);
        //            param.Add("@p_EmployeeId", Convert.ToInt32(empId));
        //            param.Add("@p_ResponsibleHR", Session["EmployeeId"]);
        //            param.Add("@p_IsActive", true);

        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);

        //            DapperORM.ExecuteReturn("sp_SUD_PMS_HR_Mapping", param);
        //        }

        //        TempData["Message"] = "HR Mapping saved successfully";
        //        TempData["Icon"] = "success";
        //        return RedirectToAction("GetList");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Message"] = ex.Message;
        //        TempData["Icon"] = "error";
        //        return RedirectToAction("GetList");
        //    }
        //}

        [HttpPost]
        public ActionResult SaveHRMapping(PMS_HRMapping model)
        {
            try
            {
                var selectedIds = string.IsNullOrEmpty(model.SelectedEmployeeIds)
                    ? new List<long>()
                    : model.SelectedEmployeeIds
                           .Split(',')
                           .Select(long.Parse)
                           .ToList();

                var paramGet = new DynamicParameters();
                paramGet.Add("@CmpID", model.CmpID);
                paramGet.Add("@BranchId", model.BranchId);
                paramGet.Add("@ResponsibleHR", model.ResponsibleHR);

                var existingIds = DapperORM
                    .ReturnList<long>("sp_Get_PMS_HR_MappedEmployees", paramGet)
                    .ToList();

                var toInsert = selectedIds.Except(existingIds).ToList();

                foreach (var empId in toInsert)
                {
                    SaveSingleMapping(model, empId);
                }
                
                var toDeactivate = existingIds.Except(selectedIds).ToList();

                foreach (var empId in toDeactivate)
                {
                    DeactivateMapping(model, empId);
                }

                TempData["Message"] = "HR Mapping saved successfully";
                TempData["Icon"] = "success";
                return RedirectToAction("ESS_PMS_HRMapping");
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Icon"] = "error";
                return RedirectToAction("ESS_PMS_HRMapping");
            }
        }


        private void SaveSingleMapping(PMS_HRMapping model, long empId)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@p_process", "Save");
                param.Add("@p_CmpID", model.CmpID);
                param.Add("@p_BranchId", model.BranchId);
                param.Add("@p_DepartmentId", model.DepartmentId);
                param.Add("@p_DesignationId", model.DesignationId);
                param.Add("@p_EmployeeId", empId);
                param.Add("@p_ResponsibleHR", model.ResponsibleHR);
                param.Add("@p_CreatedBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());

                DapperORM.ExecuteReturn("sp_SUD_PMS_HR_Mapping", param);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
            }
            
        }


        private void DeactivateMapping(PMS_HRMapping model, long empId)
        {
            try
            {
                var param = new DynamicParameters();

                param.Add("@p_process", "Deactivate");
                param.Add("@p_CmpID", model.CmpID);
                param.Add("@p_BranchId", model.BranchId);
                param.Add("@p_DepartmentId", model.DepartmentId);
                param.Add("@p_DesignationId", model.DesignationId);
                param.Add("@p_EmployeeId", empId);
                param.Add("@p_ModifiedBy", Session["EmployeeName"]);

                DapperORM.ExecuteReturn("sp_SUD_PMS_HR_Mapping", param);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
            }

        }



    }
}