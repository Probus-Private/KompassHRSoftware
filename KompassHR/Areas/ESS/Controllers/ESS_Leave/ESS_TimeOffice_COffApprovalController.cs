using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Leave
{
    public class ESS_TimeOffice_COffApprovalController : Controller
    {
        // GET: ESS/ESS_TimeOffice_COffApproval

        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_TimeOffice_COffApproval
        #region ESS_TimeOffice_COffApproval
        public ActionResult ESS_TimeOffice_COffApproval(ReportFilter MasReport, DateTime? GetDetailsDate, int? GetDetailsBranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 380;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //var a = DapperORM.DynamicQuerySingle("Select CoffVaild from Atten_CoffSetting").FirstOrDefault();
                //var Coff = a.CoffVaild;
                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                //var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                //ViewBag.CompanyName = GetComapnyName;
                //var CmpID = GetComapnyName[0].Id;

                //var results = DapperORM.DynamicQuerySingleMultiple(@"Select ContractorId as Id,ContractorName As Name  from Contractor_MAster where Deactivate=0  order by Name;
                //                                     select DepartmentId as Id, DepartmentName as Name from Mas_Department where Deactivate=0 order by DepartmentName; 
                //                                  select DesignationId as Id,DesignationName as Name from Mas_Designation where Deactivate=0 order by DesignationName
                //                                  select GradeId as Id,GradeName as Name from Mas_Grade where Deactivate=0 order by Name");

                //ViewBag.GetContractorName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DepartmentName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.DesignationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GradeName = results.Read<AllDropDownClass>().ToList();

                //DynamicParameters paramBranchList = new DynamicParameters();
                //paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                //paramBranchList.Add("@p_CmpId", CmpID);
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                //ViewBag.BranchName = data;
                //var BranchId = data[0].Id;


                //DynamicParameters paramLine1 = new DynamicParameters();
                //paramLine1.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpID + " and Mas_LineMaster.BranchId=" + BranchId + " ");
                //var List_LineMaster1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine1).ToList();
                //ViewBag.LineName = List_LineMaster1;

                ////DynamicParameters paramEMP = new DynamicParameters();
                ////paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpID + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                ////var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                ////ViewBag.EmployeeName = GetEmployeeName;

                //DynamicParameters paramUnit = new DynamicParameters();
                //paramUnit.Add("@query", "Select UnitId as Id,UnitName As Name  from Mas_Unit where Deactivate=0  and CmpId=" + CmpID + " and UnitBranchId=" + BranchId + "");
                //var GetUnitName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();
                //ViewBag.SubUnitName = GetUnitName;

                if (GetDetailsDate != null)
                {
                    DateTime date = DateTime.Parse(GetDetailsDate.ToString());
                    string dateOnly = date.ToString("yyyy-MM-dd");

                    var Query1 = "";
                    //Query1 = " and Atten_InOut.InoutBranchId=" + GetDetailsBranchId + "";

                    if (GetDetailsDate != null)
                    {
                        Query1 = Query1 + " and  Atten_InOut.InOutDate BETWEEN '" + dateOnly + "' AND '" + dateOnly + "'";

                        Query1 = Query1 + " and atten_inout.InOutEmployeeId in (select distinct  ReportingEmployeeID from mas_employee_reporting where Deactivate=0 and ReportingModuleID=8 and ReportingManager1=" + Session["EmployeeId"] + " )";
                    }

                    DynamicParameters paramdetailsList = new DynamicParameters();
                    paramdetailsList.Add("@P_Qry", Query1);
                    var GetCOffPendingList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeCOffList", paramdetailsList).ToList(); /* sp_GetEmployeeOTlist*/
                    ViewBag.COffApprovalList = GetCOffPendingList;

                    ViewBag.SubDepartmentName = "";
                    ViewBag.LineName = "";
                    ViewBag.EmployeeName = "";
                    ViewBag.SubUnitName = "";
                    TempData["GetDate"] = GetDetailsDate;
                    return View();
                }

                var Query = "";
                //if (MasReport.CmpId != null)
                //{
                //    if (MasReport.BranchId != null)
                //    {
                //        Query = "and Atten_InOut.InoutBranchId=" + MasReport.BranchId + "";
                //        DynamicParameters paramLine2 = new DynamicParameters();
                //        paramLine2.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + MasReport.CmpId + " and Mas_LineMaster.BranchId=" + MasReport.BranchId + " ");
                //        var List_LineMaster2 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine2).ToList();
                //        ViewBag.LineName = List_LineMaster2;

                //        //DynamicParameters paramEMP1 = new DynamicParameters();
                //        //paramEMP1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                //        //var GetEmployeeName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP1).ToList();
                //        //ViewBag.EmployeeName = GetEmployeeName1;

                //        DynamicParameters paramUnit1 = new DynamicParameters();
                //        paramUnit1.Add("@query", "Select UnitId as Id,UnitName As Name  from Mas_Unit where Deactivate=0  and CmpId=" + MasReport.CmpId + " and UnitBranchId=" + MasReport.BranchId + "");
                //        var GetUnitName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit1).ToList();
                //        ViewBag.SubUnitName = GetUnitName1;
                //    }
                //    else
                //    {
                //        Query = " and Atten_InOut.InoutBranchId in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                //        ViewBag.LineName = "";
                //        ViewBag.EmployeeName = "";
                //        ViewBag.SubUnitName = "";
                //    }

                //    if (MasReport.DepartmentId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpDepartmentId=" + MasReport.DepartmentId + "";

                //        DynamicParameters paramSub = new DynamicParameters();
                //        paramSub.Add("@query", "Select SubDepartmentId As Id,SubDepartmentName as Name from Mas_SubDepartment where Mas_SubDepartment.Deactivate=0 and Mas_SubDepartment.DepartmentId=" + MasReport.DepartmentId + "");
                //        var DataDept = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramSub).ToList();
                //        ViewBag.SubDepartmentName = DataDept;

                //    }
                //    else
                //    {
                //        ViewBag.SubDepartmentName = "";
                //    }

                //    if (MasReport.SubDepartmentId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpSubDepartmentId=" + MasReport.SubDepartmentId + "";
                //    }
                //    if (MasReport.DesignationId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpDesignationId=" + MasReport.DesignationId + "";
                //    }


                //    if (MasReport.GradeId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpGradeId=" + MasReport.GradeId + "";
                //    }


                //    if (MasReport.LineId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpLineId=" + MasReport.LineId + "";
                //    }
                //    if (MasReport.ContractorId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpContractorId=" + MasReport.ContractorId + "";
                //    }

                //    if (MasReport.SubUnitId != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOut_EmpUnitId=" + MasReport.SubUnitId + "";
                //    }
                //}
                ///var GetFromDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                // var GetFromDate = Convert.ToDateTime(MasReport.FromDate).ToString("yyyy-MM-dd");/* MasReport.FromDate.ToString("yyyy-MM-dd");*/
                ViewBag.COffApprovalList = "";

                if ((MasReport.FromDate != DateTime.MinValue) && (MasReport.FromDate != null))
                {
                    Query = Query + " and  Atten_InOut.InOutDate BETWEEN '" + Convert.ToDateTime(MasReport.FromDate).ToString("yyyy-MM-dd") + "' AND '" + Convert.ToDateTime(MasReport.ToDate).ToString("yyyy-MM-dd") + "' and CoffApproved IS NULL";

                    Query = Query + " and atten_inout.InOutEmployeeId in (select distinct  ReportingEmployeeID from mas_employee_reporting where Deactivate=0 and ReportingModuleID=8 and ReportingManager1=" + Session["EmployeeId"] + " )";


                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetCoffApprovalList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeCOffList", paramList).ToList();
                    ViewBag.COffApprovalList = GetCoffApprovalList;

                    if (GetCoffApprovalList.Count == 0 || GetCoffApprovalList == null)
                    {
                        TempData["Message"] = "Records Not Found Of Selected Date!";
                        TempData["Icon"] = "error";
                    }
                    TempData["ShowData"] = true;
                }

                //else
                //{
                //    var GetDateTo = DateTime.Now.Date.ToString("yyyy-MM-dd");
                //    var GetDateFrom = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                //    Query = Query + " and  Atten_InOut.InOutDate BETWEEN '" + GetDateFrom + "' AND '" + GetDateTo + "' and CoffApproved IS NULL";

                //    Query = Query + " and atten_inout.InOutEmployeeId in (select distinct  ReportingEmployeeID from mas_employee_reporting where Deactivate=0 and ReportingModuleID=8 and ReportingManager1=" + Session["EmployeeId"] + " )";


                //    DynamicParameters paramList = new DynamicParameters();
                //    paramList.Add("@P_Qry", Query);
                //    var GetCoffApprovalList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeCOffList", paramList).ToList();
                //    ViewBag.COffApprovalList = GetCoffApprovalList;

                //    if (GetCoffApprovalList.Count == 0 || GetCoffApprovalList == null)
                //    {
                //        TempData["Message"] = "Records Not Found Of Selected Date!";
                //        TempData["Icon"] = "error";
                //    }
                //    TempData["ShowData"] = true;
                //    //ViewBag.COffApprovalList = "";
                //}
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
        public ActionResult MultipleApproveRejectRequest(List<COffApprovalList> RecordList, DateTime GetDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 380;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                var a = DapperORM.DynamicQuerySingle("Select CoffVaild from Atten_CoffSetting");
                var Coff = a.CoffVaild;
                foreach (var Data in RecordList)
                {
                    string Employee_Admin = " Update Atten_InOut set CoffApproved= " + Data.ApprovedCoffDays + "," +
                                                  "CoffApproveBy='" + Session["EmployeeId"] + "'," +
                                                  "CoffApproveRemark='" + Data.Remark + "'," +
                                                  "CoffApproveDate=Getdate()," +
                                                   "ModifiedBy='" + Session["EmployeeName"] + "'," +
                                                   "ModifiedDate=Getdate()," +
                                                   "MachineName='" + Dns.GetHostName().ToString() + "' " +
                                                   " Where  Atten_InOut.InOutID='" + Data.InOutID + "'     ";
                    strBuilder.Append(Employee_Admin);

                    if (Data.ApprovedCoffDays > 0)
                    {
                        string SetoffGeneration = (@"Insert Into Atten_CoffGeneration(CmpID " +
                                       "   , CoffGenerationBranchID " +
                                       "   , Deactivate " +
                                       "   , CreatedBy " +
                                       "   , CreatedDate " +
                                       "   , MachineName " +
                                       "   , DocNo " +
                                       "   , DocDate " +
                                       "   , CoffGenerationEmployeeID " +
                                       "   , CoffGenerationManually   " +
                                       "   , CoffExpriredDate         " +
                                       "   , CoffGenerationDate       " +
                                       "   , Actualnoofcoff           " +
                                       "   , Approvenoofcoff          " +
                                       "   , Reason                   " +
                                       "   , CoffApprovedStatus       " +
                                       "   , ApproveRejectBy          " +
                                       "   , ApproveRejectRemark" +
                                       "   , ApproveRejectDate  " +
                                       "   , CoffAttenInOutId   " +
                                       "   , AvailedCoff        " +
                                       "   , RequestFrom        " +
                                       "   , CoffStatus)        " +
                                       "    select " +
                                       "    CmpID " +
                                       "   , InOutBranchId " +
                                       "   ,'0' " +
                                       "   ,CreatedBy " +
                                       "   ,getdate() " +
                                       "   ,MachineName " +
                                       "   ,(select isnull(max(DocNo), 0) + 1 from Atten_CoffGeneration ) " +
                                       "   ,getdate() " +
                                       "   ,InOutEmployeeId " +
                                       "   ,'0' " +
                                       "   ,Cast(DATEADD(day, " + Coff + ", CoffApproveDate) as date) " +
                                       "   ,InOutDate " +
                                       "   ,InOutCoff " +
                                       "   ,CoffApproved " +
                                       "   ,'' " +
                                       "   ,'Approved' " +
                                       "   ,CoffApproveBy    " +
                                       "   ,CoffApproveRemark" +
                                       "   ,CoffApproveDate  " +
                                       "   ,InOutID " +
                                       "   ,'' " +
                                       "   ,'CoffApproval' " +
                                       "   ,'Pending' " +
                                       "   from Atten_InOut " +
                                       "   where Deactivate = 0  and InOutID ='" + Data.InOutID + "'    ");
                        strBuilder.Append(SetoffGeneration);
                    }
                }

                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Coff approved successfully";
                    TempData["Icon"] = "success";
                    DapperORM.Execute("Update Atten_CoffGeneration Set CoffGenerationID_Encrypted=(master.dbo.fn_varbintohexstr(HashBytes('SHA2_256 ', convert(nvarchar(70),CoffGenerationID)))) where CoffGenerationID_Encrypted is null");
                }
                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
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

        #region OTApprovedList
        public ActionResult COffApproved_List(ReportFilter MasReport)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 380;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                //var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                //ViewBag.CompanyName = GetComapnyName;
                //var CmpID = GetComapnyName[0].Id;

                //DynamicParameters paramBranchList = new DynamicParameters();
                //paramBranchList.Add("@p_employeeid", Session["EmployeeId"]);
                //paramBranchList.Add("@p_CmpId", CmpID);
                //var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList).ToList();
                //ViewBag.BranchName = data;
                //var BranchId = data[0].Id;

                //DynamicParameters paramEMP = new DynamicParameters();
                //paramEMP.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpID + " and EmployeeBranchId= " + BranchId + "and Mas_Employee.EmployeeLeft=0");
                //var GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                //ViewBag.EmployeeName = GetEmployeeName;


                //if (MasReport.CmpId != null)
                //{
                //    if (MasReport.BranchId != null)
                //    {
                //        Query = " and Atten_InOut.InoutBranchId=" + MasReport.BranchId + "";

                //        DynamicParameters paramEMP1 = new DynamicParameters();
                //        paramEMP1.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + MasReport.CmpId + " and EmployeeBranchId= " + MasReport.BranchId + "and Mas_Employee.EmployeeLeft=0");
                //        var GetEmployeeName1 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP1).ToList();
                //        ViewBag.EmployeeName = GetEmployeeName1;
                //    }
                //    else
                //    {
                //        Query = " and Atten_InOut.InoutBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + MasReport.CmpId + " and UserBranchMapping.IsActive = 1)";
                //    }
                //    if (MasReport.EmployeeID != null)
                //    {
                //        Query = Query + " and Atten_InOut.InOutEmployeeId=" + MasReport.EmployeeID + "";
                //    }
                //}

                // var GetDate = MasReport.FromDate.ToString("yyyy-MM-dd");
                var Query = "";
                if (MasReport.FromDate != null)
                {
                    Query = Query + " and Atten_InOut.InOutDate  BETWEEN '" + Convert.ToDateTime(MasReport.FromDate).ToString("yyyy-MM-dd") + "' and '" + Convert.ToDateTime(MasReport.ToDate).ToString("yyyy-MM-dd") + "'";
                    Query = Query + " and atten_inout.InOutEmployeeId in (select distinct  ReportingEmployeeID from mas_employee_reporting where Deactivate=0 and ReportingModuleID=8 and ReportingManager1=" + Session["EmployeeId"] + " )";


                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@P_Qry", Query);
                    var GetCOffApprovedList = DapperORM.ReturnList<dynamic>("sp_GetEmployeeCOffList_Approved", paramList).ToList();
                    ViewBag.COffApprovedList = GetCOffApprovedList;

                    if (GetCOffApprovedList.Count == 0 || GetCOffApprovedList == null)
                    {
                        TempData["Message"] = "Records Not Found !";
                        TempData["Icon"] = "error";
                    }

                    //DynamicParameters paramBranchList1 = new DynamicParameters();
                    //paramBranchList1.Add("@p_employeeid", Session["EmployeeId"]);
                    //paramBranchList1.Add("@p_CmpId", MasReport.CmpId);
                    //var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", paramBranchList1).ToList();
                    //ViewBag.BranchName = data1;
                    TempData["ShowData"] = true;
                }
                else
                {
                    ViewBag.COffApprovedList = "";

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
        public ActionResult TimeOffice_COffRejected(int? InOutID, string Remark, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 380;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetAvailedCoff = DapperORM.DynamicQuerySingle("Select AvailedCoff from Atten_CoffGeneration where Atten_CoffGeneration.Deactivate=0 and  CoffGenerationEmployeeID=" + EmployeeId + " and CoffAttenInOutId=" + InOutID + "");
                if (GetAvailedCoff != null)
                {
                    var z = GetAvailedCoff.AvailedCoff;
                    if (z > 0)
                    {
                        var Message = "Can't reject Coff is already use ";
                        var Icon = "error";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var a = DapperORM.DynamicQuerySingle("update Atten_InOut set CoffApproved=NULL ,CoffApproveBy=" + Session["EmployeeId"] + ",CoffApproveDate=GETDATE(),CoffApproveRemark=CONCAT(CoffApproveRemark,'," + Remark + "')  where Atten_InOut.InOutID=" + InOutID + "");
                        var b = DapperORM.DynamicQuerySingle("update Atten_CoffGeneration set Deactivate=1,CoffApprovedStatus='Rejected',ModifiedDate=GETDATE(),ModifiedBy=" + Session["EmployeeId"] + ",ApproveRejectDate=GETDATE(),ApproveRejectBy=" + Session["EmployeeId"] + ",ApproveRejectRemark=CONCAT(ApproveRejectRemark,'," + Remark + "') where CoffAttenInOutId=" + InOutID + "  ");
                        var Message = "Coff rejected succesfully";
                        var Icon = "success";
                        return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var a = DapperORM.DynamicQuerySingle("update Atten_InOut set CoffApproved=NULL ,CoffApproveBy=" + Session["EmployeeId"] + ",CoffApproveDate=GETDATE(),CoffApproveRemark=CONCAT(CoffApproveRemark,'," + Remark + "')  where Atten_InOut.InOutID=" + InOutID + "");
                    var Message = "Coff rejected succesfully";
                    var Icon = "success";
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
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
        public ActionResult Pending_COffList(MonthWiseFilter Obj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 380;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (Obj.Month != null) 
                {
                    string dateString = Convert.ToString(Obj.Month);
                    DateTime date = DateTime.Parse(dateString);
                    int Month = date.Month;
                    int Year = date.Year;

                    DynamicParameters param = new DynamicParameters();
                    // param.Add("@p_BranchId", Obj.BranchId);
                    param.Add("@p_Month", Month);
                    param.Add("@p_year", Year);
                    param.Add("@p_Employeeid", Session["EmployeeId"]);
                    var GetList = DapperORM.ExecuteSP<dynamic>("SP_List_PendingCOff", param).ToList();
                    ViewBag.COffPendingList = GetList;
                    //TempData["GetMonth"] = Obj.Month.ToString("MMM/yyyy");
                    if (Obj.Month.HasValue)
                    {
                        TempData["GetMonth"] = Obj.Month.Value.ToString("MMM/yyyy");
                    }
                    if (GetList.Count == 0)
                    {
                        TempData["Message"] = "Records Not Found !";
                        TempData["Icon"] = "error";
                    }

                }
                //else
                //{
                //    DateTime dateString = DateTime.Now.Date; // Assuming Obj.Month contains a valid date string
                //    //DateTime date = DateTime.Parse(dateString);
                //    DateTime lastMonthDate = dateString.AddMonths(-1);
                //    int Month = dateString.Month;
                //    //int Month = date.Month;
                //    int Year = dateString.Year;

                //    DynamicParameters param = new DynamicParameters();
                //    // param.Add("@p_BranchId", Obj.BranchId);
                //    param.Add("@p_Month", Month);
                //    param.Add("@p_year", Year);
                //    param.Add("@p_Employeeid", Session["EmployeeId"]);
                //    var GetList = DapperORM.ExecuteSP<dynamic>("SP_List_PendingCOff", param).ToList();
                //    ViewBag.COffPendingList = GetList;
                //    //TempData["GetMonth"] = Obj.Month.ToString("MMM/yyyy");
                //    if (Obj.Month.HasValue)
                //    {
                //        TempData["GetMonth"] = Obj.Month.Value.ToString("MMM/yyyy");
                //    }
                //    if (GetList.Count == 0)
                //    {
                //        TempData["Message"] = "Records Not Found !";
                //        TempData["Icon"] = "error";
                //    }
                //    //ViewBag.COffPendingList = "";
                //}
                return View(Obj);
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