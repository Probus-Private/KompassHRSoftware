using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_OTApprovalController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_TimeOffice_OTApproval
        #region Main View
        public ActionResult ESS_TimeOffice_OTApproval(DailyAttendanceReportFilter MasReport , DateTime? GetDetailsDate, int? GetDetailsBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 360;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var results = DapperORM.DynamicQueryMultiple(@"Select ContractorId as Id,ContractorName As Name  from Contractor_MAster where Deactivate=0  order by Name;
                                                     select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");
                
                ViewBag.GetContractorName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DepartmentName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.DesignationName = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GradeName = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                
                if(GetComapnyName.Count>0)
                {
                    var CmpID = GetComapnyName[0].Id;
                    ViewBag.CompanyName = GetComapnyName;


                    DynamicParameters paramBranchList = new DynamicParameters();
                    paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList.Add("@p_CmpId", CmpID);
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                    ViewBag.BranchName = data;
                    var BranchId = data[0].Id;


                    DynamicParameters paramLine1 = new DynamicParameters();
                    paramLine1.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpID + " and Mas_LineMaster.BranchId=" + BranchId + " ");
                    var List_LineMaster1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine1).ToList();
                    ViewBag.LineName = List_LineMaster1;

                    //DynamicParameters paramEMP = new DynamicParameters();
                    //paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpID + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                    //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                    //ViewBag.EmployeeName = GetEmployeeName;

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramUnit.Add("@query", "Select UnitId as Id,UnitName As Name  from Mas_Unit where Deactivate=0  and CmpId=" + CmpID + " and UnitBranchId=" + BranchId + "");
                    var GetUnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();
                    ViewBag.SubUnitName = GetUnitName;
                }
                else
                {
                    ViewBag.CompanyName = "";
                    ViewBag.SubUnitName = "";
                    ViewBag.BranchName = "";
                    ViewBag.LineName = "";
                }
                

                if (GetDetailsDate != null)
                {
                    DateTime date = DateTime.Parse(GetDetailsDate.ToString());
                    string dateOnly = date.ToString("yyyy-MM-dd");

                    var Query1 = "";
                    Query1 = " and inoutBranchId=" + GetDetailsBranchId + "";

                    if (GetDetailsDate != null)
                    {
                        Query1 = Query1 + " and  Atten_InOut.InOutDate BETWEEN '" + dateOnly + "' AND '" + dateOnly + "' and AcutalOT IS NULL";
                    }

                    DynamicParameters paramdetailsList = new DynamicParameters();
                    paramdetailsList.Add("@P_Qry", Query1);
                    var GetOTPendingList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeOTlist", paramdetailsList).ToList();
                    ViewBag.OTApprovalList = GetOTPendingList;

                    ViewBag.SubDepartmentName = "";
                    ViewBag.LineName = "";
                    ViewBag.EmployeeName = "";
                    ViewBag.SubUnitName = "";
                    return View();
                }

                var Query = "";
                if (MasReport.CmpId != null)
                {
                    if (MasReport.BranchId != null)
                    {
                        Query = "and inoutBranchId=" + MasReport.BranchId + "";
                        DynamicParameters paramLine2 = new DynamicParameters();
                        paramLine2.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                        var List_LineMaster2 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine2).ToList();
                        ViewBag.LineName = List_LineMaster2;

                        //DynamicParameters paramEMP1 = new DynamicParameters();
                        //paramEMP1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                        //var GetEmployeeName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP1).ToList();
                        //ViewBag.EmployeeName = GetEmployeeName1;

                        DynamicParameters paramUnit1 = new DynamicParameters();
                        paramUnit1.Add("@query", "Select UnitId as Id,UnitName As Name  from Mas_Unit where Deactivate=0  and CmpId=" + MasReport.CmpId + " and UnitBranchId=" + MasReport.BranchId + "");
                        var GetUnitName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit1).ToList();
                        ViewBag.SubUnitName = GetUnitName1;
                    }
                    else
                    {
                        Query = "and mas_branch.BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                        ViewBag.LineName = "";
                        ViewBag.EmployeeName = "";
                        ViewBag.SubUnitName = "";
                    }

                    if (MasReport.DepartmentId != null)
                    {
                        Query = Query + " and Mas_Department.DepartmentId=" + MasReport.DepartmentId + "";

                        DynamicParameters paramSub = new DynamicParameters();
                        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                        ViewBag.SubDepartmentName = DataDept;

                    }
                    else
                    {
                        ViewBag.SubDepartmentName = "";
                    }
                    if (MasReport.SubDepartmentId != null)
                    {
                        Query = Query + " and Mas_SubDepartment.SubDepartmentId=" + MasReport.SubDepartmentId + "";
                    }
                    if (MasReport.DesignationId != null)
                    {
                        Query = Query + " and Mas_Designation.DesignationId=" + MasReport.DesignationId + "";
                    }


                    if (MasReport.GradeId != null)
                    {
                        Query = Query + " and Mas_Grade.GradeId=" + MasReport.GradeId + "";
                    }


                    if (MasReport.LineId != null)
                    {
                        Query = Query + " and Mas_LineMaster.LineId=" + MasReport.LineId + "";
                    }
                    if (MasReport.ContractorId != null)
                    {
                        Query = Query + " and Mas_Employee.ContractorID=" + MasReport.ContractorId + "";
                    }

                    if (MasReport.SubUnitId != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeUnitID=" + MasReport.SubUnitId + "";
                    }

                    var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                    var GetToDate = MasReport.ToDate.ToString("yyyy-MM-dd");

                    if (MasReport.FromDate != null && MasReport.ToDate != null)
                    {
                        Query = Query + " and  Atten_InOut.InOutDate BETWEEN '" + GetFromDate + "' AND '" + GetToDate + "' and AcutalOT IS NULL";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetOTApprovalList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeOTlist", paramList).ToList();
                    ViewBag.OTApprovalList = GetOTApprovalList;

                    if (GetOTApprovalList.Count == 0 || GetOTApprovalList == null)
                    {
                        TempData["Message"] = "Records Not Found !";
                        TempData["Icon"] = "error";
                    }

                    DynamicParameters paramBranchList1 = new DynamicParameters();
                    paramBranchList1.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList1.Add("@p_CmpId", MasReport.CmpId);
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList1).ToList();
                    ViewBag.BranchName = data1;
                    TempData["ShowData"] = true;
                }
                else
                {
                    ViewBag.OTApprovalList = "";
                    ViewBag.SubDepartmentName = "";
                    ViewBag.LineName = "";
                    ViewBag.EmployeeName = "";
                    ViewBag.SubUnitName = "";
                }
                return View(MasReport);

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

        #region GetUnit
        [HttpGet]
        public ActionResult GetUnit(int? CmpId, int? UnitBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters paramLine = new DynamicParameters();
                    paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " and Mas_LineMaster.BranchId=" + UnitBranchId + " ");
                    var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();

                    DynamicParameters paramUnit = new DynamicParameters();
                    paramUnit.Add("@query", "Select UnitId as Id,UnitName As Name  from Mas_Unit where Deactivate=0  and CmpId=" + CmpId + " and UnitBranchId=" + UnitBranchId + "");
                    var GetUnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();

                    return Json(new { List_LineMaster = List_LineMaster, GetUnitName = GetUnitName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetSubDepartment
        [HttpGet]
        public ActionResult GetSubDepartment(int? DepartmentId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (DepartmentId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + DepartmentId + "");
                    var Data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(Data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region  Multuple Multiple Approve Reject Request function
        [HttpPost]
        public ActionResult MultipleApproveRejectRequest(List<OTApprovalList> RecordList, DateTime GetDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 360;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();

                foreach (var Data in RecordList)
                {
                    string Employee_Admin = " Update Atten_InOut set AcutalOT= '" + Data.ActualOTHrs + "'," +
                                                        "OTApprovedBy='" + Session["EmployeeId"] + "'," +
                                                        "OTRemark='" + Data.Remark + "'," +
                                                        "OTApproveDate=Getdate()," +
                                                         "ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                         "ModifiedDate=Getdate()," +
                                                         "MachineName='" + Dns.GetHostName().ToString() + "' " +
                                                         " Where  InOutID=" + Data.InOutID + "";
                    strBuilder.Append(Employee_Admin);

                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {

                    TempData["Message"] = "Record Update successfully";
                    TempData["Icon"] = "success";
                }
                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                }


                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //for (var i = 0; i < RecordList.Count; i++)
                //{
                //    param.Add("@p_process", "Update");
                //    param.Add("@p_EmployeeID", RecordList[i].EmployeeID);
                //    param.Add("@p_InOutID", RecordList[i].InOutID);
                //    param.Add("@p_ActualOTHrs", RecordList[i].ActualOTHrs);
                //    param.Add("@p_Date", GetDate);
                //    param.Add("@p_Remark", RecordList[i].Remark);
                //    param.Add("@p_OTApprovedBy", Session["EmployeeId"]);
                //    param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                //    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //    var data = DapperORM.ExecuteReturn("sp_SUD_OTApproval", param);
                //    var message = param.Get<string>("@p_msg");
                //    var Icon = param.Get<string>("@p_Icon");
                //    TempData["Message"] = message;
                //    TempData["Icon"] = Icon;
                //}
                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region OTApprovedList
        public ActionResult OTApproved_List(DailyAttendanceReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 360;
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
                var CmpID = GetComapnyName[0].Id;

                DynamicParameters paramBranchList = new DynamicParameters();
                paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranchList.Add("@p_CmpId", CmpID);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                ViewBag.BranchName = data;
                var BranchId = data[0].Id;

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpID + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                ViewBag.EmployeeName = GetEmployeeName;

                var Query = "";
                if (MasReport.CmpId != null)
                {
                    if (MasReport.BranchId != null)
                    {
                        Query = "and inoutBranchId=" + MasReport.BranchId + "";
                        DynamicParameters paramEMP1 = new DynamicParameters();
                        paramEMP1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                        var GetEmployeeName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP1).ToList();
                        ViewBag.EmployeeName = GetEmployeeName1;
                    }
                    else
                    {
                        Query = "and mas_branch.BranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                    }
                    if (MasReport.EmployeeID != null)
                    {
                        Query = Query + " and Mas_Employee.EmployeeId=" + MasReport.EmployeeID + "";
                    }

                    // var GetMonth = MasReport.FromDate.ToString("MM");
                    var GetDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                    if (MasReport.FromDate != null && MasReport.ToDate != null)
                    {
                        Query = Query + " and Atten_InOut.InOutDate='" + GetDate + "' and AcutalOT IS not NULL";
                    }

                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetOTApprovedList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeOTlist_Approved", paramList).ToList();
                    ViewBag.OTApprovedList = GetOTApprovedList;

                    if (GetOTApprovedList.Count == 0 || GetOTApprovedList == null)
                    {
                        TempData["Message"] = "Records Not Found !";
                        TempData["Icon"] = "error";
                    }

                    DynamicParameters paramBranchList1 = new DynamicParameters();
                    paramBranchList1.Add("@p_employeeid", Session["EmployeeId"]);
                    paramBranchList1.Add("@p_CmpId", MasReport.CmpId);
                    var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList1).ToList();
                    ViewBag.BranchName = data1;
                    TempData["ShowData"] = true;
                }
                else
                {
                    ViewBag.OTApprovedList = "";

                }
                return View(MasReport);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeList
        [HttpGet]
        public ActionResult GetEmployeeList(int BranchId, int CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpID + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetBusinessApprovedUnit
        [HttpGet]
        public ActionResult GetBusinessApprovedUnit(int CmpId)
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
                var BranchList = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BranchId = BranchList[0].Id;

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                return Json(new { BranchList = BranchList, GetEmployeeName = GetEmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region OTRejected
        public ActionResult TimeOffice_OTRejected(int? InOutID, string Remark)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var a = DapperORM.Execute("Update Atten_InOut set AcutalOT=NULL ,OTApprovedBy=" + Session["EmployeeId"] + ",OTApproveDate=GETDATE(),InOutAdjRemark=CONCAT(InOutAdjRemark,'," + Remark + "')  where Atten_InOut.InOutID=" + InOutID + "");
                var Message = "Rejected Succesfully";
                var Icon = "success";
                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("OTApproved_List", "ESS_TimeOffice_OTApproval");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region PendingOTList
        public ActionResult Pending_OTList(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
               
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CmpID = GetComapnyName[0].Id;

                DynamicParameters paramBranchList = new DynamicParameters();
                paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                paramBranchList.Add("@p_CmpId", CmpID);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                ViewBag.BranchName = data;

                if(Obj.BranchId !=0)
                {
                    string dateString = Convert.ToString(Obj.Month);
                    DateTime date = DateTime.Parse(dateString);
                    int Month = date.Month;
                    int Year = date.Year;

                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_Barnchid", Obj.BranchId);
                    param.Add("@p_Month", Month);
                    param.Add("@p_year", Year);
                    var GetList = DapperORM.ExecuteSP<dynamic>("SP_List_PendingOT", param).ToList();
                    ViewBag.OTPendingList = GetList;
                    if(GetList.Count==0)
                    {
                        TempData["Message"] = "No Record Fount";
                        TempData["Icon"] = "error";
                    }
                   
                }
                else
                {
                    ViewBag.OTPendingList = "";
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
    }
}