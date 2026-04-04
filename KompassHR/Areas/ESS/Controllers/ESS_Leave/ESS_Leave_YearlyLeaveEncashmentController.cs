using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using KompassHR.Areas.ESS.Models.ESS_Leave;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_Leave_YearlyLeaveEncashmentController : Controller
    {
        #region Main View 
        // GET: ESS/ESS_Leave_YearlyLeaveEncashment
        public ActionResult ESS_Leave_YearlyLeaveEncashment(YearlyLeaveEncashment YearlyLeaveEncashment)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 872;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;

                //ViewBag.GetLeaveGroup = new List<AllDropDownBind>();
                ViewBag.GetLeaveType = new List<AllDropDownBind>();
                //ViewBag.GetLeaveYear =  new List<AllDropDownBind>();

                if (YearlyLeaveEncashment.CompanyId != 0)
                {

                    DynamicParameters paramEmp = new DynamicParameters();
                    paramEmp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + YearlyLeaveEncashment.CompanyId + "' and EmployeeId<>1 and EmployeeLeft=0 order by Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    ViewBag.GetEmployeeList = EmployeeList;

                    DynamicParameters paramLeaveYear = new DynamicParameters();
                    paramLeaveYear.Add("@query", "select  LeaveYearId as Id, CONCAT(YEAR(FromDate), '-', YEAR(ToDate)) AS name from Leave_Year where Deactivate=0 AND CmpID='" + YearlyLeaveEncashment.CompanyId + "' order by Name ");
                    var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                    ViewBag.GetLeaveYear = LeaveYear;
                }
                
                else
                {
                    ViewBag.GetEmployeeList = new List<AllDropDownBind>();

                    ViewBag.GetLeaveYear = new List<AllDropDownBind>();
                }

                if (YearlyLeaveEncashment.CompanyId != 0)
                {
                    DynamicParameters paramLeaveGroup = new DynamicParameters();
                    paramLeaveGroup.Add("@query", "Select LeaveGroupId as Id, LeaveGroupName As Name From Leave_Group where Deactivate = 0  and CmpId= '" + YearlyLeaveEncashment.CompanyId + "' order by Name ");
                    var LeaveGroup = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveGroup).ToList();
                    ViewBag.GetLeaveGroup = LeaveGroup;
                }
                else
                {
                    ViewBag.GetLeaveGroup = new List<AllDropDownBind>();
                }
                
                if (YearlyLeaveEncashment.CompanyId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", YearlyLeaveEncashment.EmployeeId);
                    paramList.Add("@p_CompanyId", YearlyLeaveEncashment.CompanyId);
                    paramList.Add("@p_LeaveYear", YearlyLeaveEncashment.LeaveYear);
                    paramList.Add("@p_Leave", YearlyLeaveEncashment.Leave);
                    paramList.Add("@p_LeaveGroupId", YearlyLeaveEncashment.LeaveGroupId);

                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Leave_YearlyLeaveEncashment", paramList).ToList();
                    ViewBag.GetLeaveEncashmentList = GetData;
                    ViewBag.DynamicTable = ToDataTable(GetData);

                    if (GetData.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                    }
                    //if (GetData.Count > 0)
                    //{
                    //    ViewBag.GetAssetList = GetData;
                    //}
                    //else
                    //{
                    //    TempData["Message"] = "Record Not Found";
                    //    TempData["Icon"] = "error";
                    //    //return View(EmployeeAsset);

                    //}

                }
                else
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_EmployeeId", YearlyLeaveEncashment.EmployeeId);
                    paramList.Add("@p_CompanyId", YearlyLeaveEncashment.CompanyId);
                    paramList.Add("@p_LeaveYear", YearlyLeaveEncashment.LeaveYear);
                    paramList.Add("@p_Leave", YearlyLeaveEncashment.Leave);
                    paramList.Add("@p_LeaveGroupId", YearlyLeaveEncashment.LeaveGroupId);
                    
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Leave_YearlyLeaveEncashment", paramList).ToList();
                    ViewBag.GetLeaveEncashmentList = GetData;
                }
                return View(YearlyLeaveEncashment);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion




        #region GetLeaveGroup
        [HttpGet]
        public ActionResult GetLeaveGroup(int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();

                DynamicParameters paramLeaveGroup = new DynamicParameters();
                paramLeaveGroup.Add("@query", "Select LeaveGroupId as Id, LeaveGroupName As Name From Leave_Group where Deactivate = 0  and CmpId= '"+ CompanyId + "' order by Name ");
                var LeaveGroup = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveGroup).ToList();
                ViewBag.GetLeaveGroup = LeaveGroup;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                ViewBag.GetEmployeeList = EmployeeName;


                DynamicParameters paramLeaveYear = new DynamicParameters();
                paramLeaveYear.Add("@query", "select  LeaveYearId as Id, CONCAT(YEAR(FromDate), '-', YEAR(ToDate)) AS name from Leave_Year where Deactivate=0 AND CmpID='" + CompanyId + "' order by Name ");
                var LeaveYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveYear).ToList();
                ViewBag.GetLeaveYear = LeaveYear;


                return Json(new { LeaveGroup = LeaveGroup, EmployeeName = EmployeeName , LeaveYear = LeaveYear }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLeaveType
        [HttpGet]
        public ActionResult GetLeaveType(string Leave,int? LeaveGroupId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                //DynamicParameters param = new DynamicParameters();
                List<AllDropDownBind> Leave1 = new List<AllDropDownBind>();

                if (Leave == "CarryForward")
                {
                    DynamicParameters paramLeaveType = new DynamicParameters();
                    paramLeaveType.Add("@query", "select Distinct LeaveTypeId As Id, LeaveTypeName As Name from Leave_Setting Left Join Leave_Type On Leave_Setting.LeaveSettingLeaveTypeId = Leave_Type.LeaveTypeId where IsCarryforward = 1 AND LeaveSettingLeaveGroupId='"+ LeaveGroupId + "' order by Name");
                    Leave1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveType).ToList();
                }
                if(Leave== "Encashment")
                {
                    DynamicParameters paramLeaveType = new DynamicParameters();
                    paramLeaveType.Add("@query", "select Distinct LeaveTypeId As Id,LeaveTypeName As Name from Leave_Setting Left Join Leave_Type On Leave_Setting.LeaveSettingLeaveTypeId = Leave_Type.LeaveTypeId where IsEncashment = 1 AND LeaveSettingLeaveGroupId='" + LeaveGroupId + "' order by Name ");
                    Leave1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLeaveType).ToList();
                }
                ViewBag.GetLeaveType = Leave1;
                
                return Json(new { Leave1 = Leave1}, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Datatable

        public System.Data.DataTable ToDataTable(IEnumerable<dynamic> items)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            if (items == null || !items.Any())
                return dt;

            // First row (DapperRow)
            var first = (IDictionary<string, object>)items.First();

            // Add columns
            foreach (var key in first.Keys)
            {
                dt.Columns.Add(key.ToString());
            }

            // Add rows
            foreach (var item in items)
            {
                var dict = (IDictionary<string, object>)item;
                DataRow row = dt.NewRow();

                foreach (var key in first.Keys)
                {
                    row[key.ToString()] = dict[key] ?? DBNull.Value;
                }

                dt.Rows.Add(row);
            }

            return dt;
        }
        #endregion



    }
}