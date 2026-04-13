using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_EmployeeWiseRegularizationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_TimeOffice_EmployeeWiseRegularization
        #region Main View
        public ActionResult ESS_TimeOffice_EmployeeWiseRegularization()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 382;
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
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var GetBranchId = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                ViewBag.GetBusinessUnit = GetBranchId;
                var BranchId = GetBranchId[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BranchId + " and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //DynamicParameters paramEmpName = new DynamicParameters();
                ////var GetCurrentMonth = DateTime.Now.Date.ToString("MM");
                ////var  GetCurrentYear  = DateTime.Now.Date.ToString("yyyy");
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) order by Name");
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId=" + BranchId + " and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                //ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                ViewBag.EmployeeLog = "";
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ESS_TimeOffice_EmployeeWiseRegularization
        [HttpPost]
        public ActionResult ESS_TimeOffice_EmployeeWiseRegularization(EmployeeWiseAttendance EmpAttObj)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;

                var CmpId = GetComapnyName[0].Id;
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                ViewBag.GetBusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + EmpAttObj.CmpId + " and EmployeeBranchId=" + EmpAttObj.BranchId + " and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                if (EmpAttObj.EmployeeID != null)
                {
                    TempData["GetSelectedEmpId"] = EmpAttObj.EmployeeID;
                    DynamicParameters GetLogparam = new DynamicParameters();
                    GetLogparam.Add("@p_CmpId", EmpAttObj.CmpId);
                    GetLogparam.Add("@p_BranchId", EmpAttObj.BranchId);
                    GetLogparam.Add("@p_EmployeeId", EmpAttObj.EmployeeID);
                    GetLogparam.Add("@p_FromDate", EmpAttObj.FromDate);
                    GetLogparam.Add("@p_ToDate", EmpAttObj.ToDate);
                    ViewBag.EmployeeLog = DapperORM.ExecuteSP<GetAttendance>("sp_CalReg_EmployeeWise_FromDate_ToDate", GetLogparam).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@p_employeeid", Session["EmployeeId"]);
                    param2.Add("@p_CmpId", EmpAttObj.CmpId);
                    ViewBag.GetBusinessUnit = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param2).ToList();

                    DynamicParameters paramEmpName3 = new DynamicParameters();
                    paramEmpName3.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + EmpAttObj.CmpId + " and EmployeeBranchId=" + EmpAttObj.BranchId + " and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName3).ToList();
                    Session["SetFromDate"] = EmpAttObj.FromDate.ToString("yyyy-MM-dd");
                    Session["SetToDate"] = EmpAttObj.ToDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    TempData["GetSelectedEmpId"] = "";
                    ViewBag.EmployeeLog = "";
                    Session["SetFromDate"] = null;
                    Session["SetToDate"] = null;
                }

                return View(EmpAttObj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeLogAndShift
        [HttpGet]
        public ActionResult GetEmployeeLogAndShift(int EmployeeId, DateTime LogFromDate, DateTime LogToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var LogFromDates = LogFromDate.ToString("yyyy-MM-dd");
                var LogToDates = LogToDate.ToString("yyyy-MM-dd");
                var GetEmployeeCardNo = DapperORM.DynamicQuerySingle(@"Select EmployeeCardNo from Mas_Employee where EmployeeId=" + EmployeeId + " and Deactivate=0");
                var EmployeeCardNo = GetEmployeeCardNo.EmployeeCardNo;
                DynamicParameters paramLogTime = new DynamicParameters();
                paramLogTime.Add("@query", @"Select LogDate,Direction  from DeviceLogs where deactivate=0 
                                            and UserId='" + EmployeeCardNo + "' and convert(date,LogDate) between '" + LogFromDates + "' and '" + LogToDates + "' Order By LogDate ");
                var Logdata = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramLogTime).ToList();
                //paramEMP.Add("@query", @"SELECT 
                //                        FORMAT(CONVERT(DATETIME, LogDate), 'hh:mm tt') AS LogTime, 
                //                        Direction FROM DeviceLogs WHERE  
                //                        DeviceLogEmployeeID = " + EmployeeId + "  AND LogDate >= '" + LogFromDates + "' AND LogDate <= '" + LogToDates + "';");
                //var Logdata = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramEMP).ToList();

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                var Shiftdata = DapperORM.ExecuteSP<dynamic>("sp_GetShift", param).ToList();

                return Json(new { Logdata = Logdata, Shiftdata = Shiftdata }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        public class AttendanceTimeRule
        {
            public int RuleId { get; set; }
            public string RuleName { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int MaxDurationMinutes { get; set; }
        }

        #region Final Submit
        public ActionResult FinalSubmit(List<GetAttendance> tbldata, int EmployeeId, int BranchId, DateTime FromDate, DateTime ToDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 382;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + FromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + FromDate.ToString("yyyy") + "'  and AtdLockIDBranchId='" + BranchId + "' and AtdLock=1");
                // var AttenLockCount = DapperORM.DynamicQuerySingleOrDefault("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + FromDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + FromDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + BranchId + " and AtdLock=1");
                DynamicParameters paramLock = new DynamicParameters();
                paramLock.Add("@p_FromDate", FromDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_ToDate", ToDate.ToString("yyyy-MM-dd"));
                paramLock.Add("@p_AtdLockIDBranchId", BranchId);
                var AttenLockCount = DapperORM.ReturnList<dynamic>("sp_Check_AttenLock", paramLock).FirstOrDefault();


                if (AttenLockCount.LockCount != 0)
                {
                    var message1 = "Record can''t be saved because the month/year is already locked.";
                    var Icon2 = "error";
                    return Json(new { Message = message1, Icon = Icon2 }, JsonRequestBehavior.AllowGet);
                }

                //DataTable dt = new DataTable();
                //DapperORM dprObj = new DapperORM();
                //dt = dprObj.ConvertToDataTable(tbldata);


                DataTable dt = ConvertToDataTable(tbldata);
                string msg = "";
                if (dt.Rows.Count > 0)
                {
                    var rules = DapperORM.ListOfDynamicQueryList<AttendanceTimeRule>("SELECT * FROM Atten_AttendanceTimeRules");
                    var inRule = rules.FirstOrDefault(r => r.RuleName == "InTime");
                    var outRule = rules.FirstOrDefault(r => r.RuleName == "OutTime");

                    DateTime DateIn, DateIn_Next, DateOut, DateOut_Next;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var SrNo = i + 1;
                        DateTime attDate = Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"]);

                        // InTime boundaries
                        DateIn = attDate.Date + inRule.StartTime;
                        DateIn_Next = attDate.Date.AddDays(1) + inRule.EndTime;

                        // OutTime boundaries
                        DateOut = attDate.Date + outRule.StartTime;
                        DateOut_Next = attDate.Date.AddDays(1) + outRule.EndTime;

                        // Validate In Time
                        if (dt.Rows[i]["InOut_InTime"].ToString() != "")
                        {
                            DateTime inTime = Convert.ToDateTime(dt.Rows[i]["InOut_InTime"]);

                            if (inTime < DateIn)
                            {
                                msg = $"Invalid In Time (Before {inRule.StartTime}) - Sr.No. {SrNo}";
                                break;
                            }
                            if (inTime > DateIn_Next)
                            {
                                msg = $"Invalid In Time (After {inRule.EndTime}) - Sr.No. {SrNo}";
                                break;
                            }
                        }

                        // Validate Out Time
                        if (dt.Rows[i]["InOut_OutTime"].ToString() != "")
                        {
                            DateTime outTime = Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"]);

                            if (outTime < DateOut)
                            {
                                msg = $"Invalid Out Time (Before {outRule.StartTime}) - Sr.No. {SrNo}";
                                break;
                            }
                            if (outTime > DateOut_Next)
                            {
                                msg = $"Invalid Out Time (After {outRule.EndTime}) - Sr.No. {SrNo}";
                                break;
                            }
                        }

                        // Validate Duration
                        if (dt.Rows[i]["InOut_InTime"].ToString() != "" && dt.Rows[i]["InOut_OutTime"].ToString() != "")
                        {
                            DateTime inTime = Convert.ToDateTime(dt.Rows[i]["InOut_InTime"]);
                            DateTime outTime = Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"]);

                            if (inTime > outTime)
                            {
                                msg = $"Invalid Date Range (In Time after Out Time) - Sr.No. {SrNo}";
                                break;
                            }

                            TimeSpan tm = outTime - inTime;
                            if (tm.TotalMinutes > inRule.MaxDurationMinutes)
                            {
                                msg = $"Invalid date duration can't be more than 24Hr - Sr.No. {SrNo}";
                                break;
                            }
                        }
                    }


                    //DateTime DateIn = new DateTime();
                    //DateTime DateIn_Next = new DateTime();

                    //DateTime DateOut = new DateTime();
                    //DateTime DateOut_Next = new DateTime();

                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    DateIn = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 6, 0, 0);
                    //    DateIn_Next = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 5, 59, 59);
                    //    DateIn_Next = DateIn_Next.AddDays(1);


                    //    DateOut = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 6, 0, 0);
                    //    DateOut_Next = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 9, 0, 0);
                    //    DateOut_Next = DateOut_Next.AddDays(1);

                    //    var SrNo = i + 1;
                    //if (dt.Rows[i]["InOut_InTime"].ToString() != "")
                    //{
                    //    if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) < DateIn)
                    //    {
                    //        msg = $"Invalid In Time (Before 6:00 AM) - Sr.No. {SrNo}";
                    //        break;
                    //    }

                    //    if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) > DateIn_Next)
                    //    {
                    //        msg = $"Invalid In Time (After 6:00 AM) - Sr.No. {SrNo}";
                    //        break;
                    //    }
                    //}

                    //    if (dt.Rows[i]["InOut_OutTime"].ToString() != "")
                    //    {
                    //        if (Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()) < DateOut)
                    //        {
                    //            msg = $"Invalid Out Time (Before 6:00 AM) - Sr.No. {SrNo}";
                    //            break;
                    //        }

                    //        if (Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()) > DateOut_Next)
                    //        {
                    //            msg = $"Invalid Out Time (After 9:00 AM) - Sr.No. {SrNo}";
                    //            break;
                    //        }
                    //    }
                    //    if (dt.Rows[i]["InOut_InTime"].ToString() != "" && dt.Rows[i]["InOut_OutTime"].ToString() != "")
                    //    {
                    //        if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) > Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()))
                    //        {
                    //            msg = $"Invalid Date Range (In Time after Out Time) - Sr.No. {SrNo}";
                    //            break;
                    //        }
                    //        TimeSpan tm = Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()) - Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString());
                    //        double totalMin = tm.TotalMinutes;
                    //        if (totalMin > 1439)
                    //        {
                    //            msg = $"Invalid date duration can't more than 24Hr - Sr.No. {SrNo}";
                    //            break;
                    //        }
                    //    }
                    //}
                }
                if (msg != "")
                {
                    return Json(new { Message = msg, Icon = "error" }, JsonRequestBehavior.AllowGet);
                }
                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_dt", dt.AsTableValuedParameter("tbl_Log"));
                ARparam.Add("@p_Origin", "EmployeeWisePunchAdjustment");
                ARparam.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_FromDate", FromDate.ToString("yyyy-MM-dd"));
                ARparam.Add("@p_ToDate", ToDate.ToString("yyyy-MM-dd"));
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                //ARparam.Add("@tbl_dt", dt, DbType.Object, ParameterDirection.Input);
                var GetData = DapperORM.ExecuteSP<dynamic>("Sp_Attendance_Regularization_EmployeeWise_FromDate_ToDate", ARparam).ToList();
                var message = ARparam.Get<string>("@p_msg");
                var Icon = ARparam.Get<string>("@p_Icon");

                return Json(new { Message = message, Icon = Icon }, JsonRequestBehavior.AllowGet);
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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                var BranchId = Branch[0].Id;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetUnit(int? UnitBranchId, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + UnitBranchId + "and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeBranchId in (select BranchID as Id from UserBranchMapping where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and UserBranchMapping.CmpID = " + CmpId + " and UserBranchMapping.IsActive = 1) and Mas_Employee.EmployeeLeft=0 Order By EmployeeName");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        [HttpGet]
        public ActionResult GetEmployee(int? CmpId, int? UnitBranchId, DateTime GetFromDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (UnitBranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", $@"select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name 
                        from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeId 
                    in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  
                    where Atten_InOut.deactivate=0 and Atten_InOut.InOutBranchId=" + UnitBranchId + " and month( Atten_InOut.InOutDate )='" + GetFromDate.ToString("MM") +
                    "' and year( Atten_InOut.InOutDate )='" + GetFromDate.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + UnitBranchId + " and( month(mas_employee.JoiningDate)<='" + GetFromDate.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + GetFromDate.ToString("yyyy") + "') and (month(LeavingDate)='" + GetFromDate.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + GetFromDate.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Mas_Employee.Deactivate=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where Atten_InOut.deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  and month( Atten_InOut.InOutDate )='" + GetFromDate.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + GetFromDate.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid  in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  and( month(mas_employee.JoiningDate)<='" + GetFromDate.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + GetFromDate.ToString("yyyy") + "') and (month(LeavingDate)='" + GetFromDate.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + GetFromDate.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();

                    return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        [HttpGet]
        public ActionResult GetEmployeeNameDateWise(int CmpId, int? BranchId, DateTime GetFromDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if (BranchId != null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and month( Atten_InOut.InOutDate )='" + GetFromDate.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + GetFromDate.ToString("yyyy") + "' and EmployeeId<>1 and InOutBranchId=" + BranchId + " ) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid=" + BranchId + " and( month(mas_employee.JoiningDate)<='" + GetFromDate.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + GetFromDate.ToString("yyyy") + "') and (month(LeavingDate)='" + GetFromDate.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + GetFromDate.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ") and month( Atten_InOut.InOutDate )='" + GetFromDate.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + GetFromDate.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and employeebranchid in (Select BranchID from UserBranchMapping where UserBranchMapping.EmployeeID=" + Session["EmployeeId"] + " and UserBranchMapping.IsActive=1 and UserBranchMapping.CmpID=" + CmpId + ")  and ( month(mas_employee.JoiningDate)<='" + GetFromDate.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + GetFromDate.ToString("yyyy") + "') and (month(LeavingDate)='" + GetFromDate.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + GetFromDate.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region ConvertToDataTable
        public static DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            DataTable table = new DataTable();
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
        #endregion
    }
}