using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TimeOffice;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TimeOffice
{
    public class ESS_TimeOffice_EmployeeWiseAttendanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();

        DataTable dt_Shift = new DataTable();
        DataTable dt_ShiftrotationalWeekOff = new DataTable();
        DataTable dt_COff_Generation = new DataTable();
        DataTable dt_PH_Check = new DataTable();
        DataTable dt_WO_Check = new DataTable();
        DataTable dt_ShiftRule = new DataTable();

        DateTime currentDate = DateTime.Now;
        // GET: ESS/ESS_TimeOffice_EmployeeWiseAttendance
        #region ESS_TimeOffice_EmployeeWiseAttendance
        public ActionResult ESS_TimeOffice_EmployeeWiseAttendance(EmployeeWiseAttendance OBJ)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AttendanceList = null;
                //param.Add("@query", "  select CompanyId as Id ,CompanyName as Name from Mas_CompanyProfile where Deactivate= 0");
                //var GetCompanyName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                //ViewBag.CompanyName = GetCompanyName;

                DynamicParameters paramEMP = new DynamicParameters();
                paramEMP.Add("@query", "Select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeLeft=0  and CmpID in (select distinct CmpID from UserBranchMapping where  isactive=1  and employeeid=" + Session["EmployeeId"] + ") order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEMP).ToList();
                ViewBag.EmployeeName = EmployeeName;


                if (OBJ.EmployeeID != null)
                {
                    if (Session["EmployeeList"] != "")
                    {
                        ViewBag.AttendanceListNew = Session["EmployeeList"];
                        //When Calculate data get Atten_EmployeeWiseSummery
                        DynamicParameters paramList1 = new DynamicParameters();
                        paramList1.Add("@p_EmployeeId", OBJ.EmployeeID);
                        paramList1.Add("@p_FromDate", OBJ.FromDate);
                        paramList1.Add("@p_ToDate", OBJ.ToDate);
                        var EmployeeListSummery = DapperORM.ReturnList<dynamic>("sp_List_Atten_EmployeeWiseSummery", paramList1).ToList();
                        //var EmployeeList = DapperORM.DynamicQuerySingle("Select InOutEmployeeId,  InOutDate, InOutIntime, InOutOutTime,  InOutDuration,   InOutLateBy,  InOutEarlyBy, InOutOT_End,  InOutCoff, InOutShift, InOutStatus, InOut_WO from Atten_InOut where Atten_InOut.InOutDate  between '" + GetFromDate + "' and '" + GetToDate + "' and Atten_InOut.InOutEmployeeId=" + OBJ.EmployeeID + " and Atten_InOut.Deactivate=0");
                        ViewBag.AttendanceListSummury = EmployeeListSummery;

                        Session["EmployeeList"] = "";
                    }
                    else
                    {
                        DynamicParameters paramList = new DynamicParameters();
                        var GetFromDate = OBJ.FromDate.ToString("yyyy-MM-dd");
                        var GetToDate = OBJ.ToDate.ToString("yyyy-MM-dd");
                        paramList.Add("@P_EMPLOYEEEID", OBJ.EmployeeID);
                        paramList.Add("@P_FROMDATE", GetFromDate);
                        paramList.Add("@P_TODATE", GetToDate);
                        var EmployeeList = DapperORM.ExecuteSP<dynamic>("SP_Atten_EmployeeWiseRegularization", paramList).ToList();
                        //var EmployeeList = DapperORM.DynamicQuerySingle("Select InOutEmployeeId,  InOutDate, InOutIntime, InOutOutTime,  InOutDuration,   InOutLateBy,  InOutEarlyBy, InOutOT_End,  InOutCoff, InOutShift, InOutStatus, InOut_WO from Atten_InOut where Atten_InOut.InOutDate  between '" + GetFromDate + "' and '" + GetToDate + "' and Atten_InOut.InOutEmployeeId=" + OBJ.EmployeeID + " and Atten_InOut.Deactivate=0");
                        ViewBag.AttendanceList = EmployeeList;

                        //ViewBag.AttendanceList1 = "14:20";
                        DynamicParameters paramList1 = new DynamicParameters();
                        paramList1.Add("@p_EmployeeId", OBJ.EmployeeID);
                        paramList1.Add("@p_FromDate", OBJ.FromDate);
                        paramList1.Add("@p_ToDate", OBJ.ToDate);
                        var EmployeeListSummery = DapperORM.ReturnList<dynamic>("sp_List_Atten_EmployeeWiseSummery", paramList1).ToList();
                        //var EmployeeList = DapperORM.DynamicQuerySingle("Select InOutEmployeeId,  InOutDate, InOutIntime, InOutOutTime,  InOutDuration,   InOutLateBy,  InOutEarlyBy, InOutOT_End,  InOutCoff, InOutShift, InOutStatus, InOut_WO from Atten_InOut where Atten_InOut.InOutDate  between '" + GetFromDate + "' and '" + GetToDate + "' and Atten_InOut.InOutEmployeeId=" + OBJ.EmployeeID + " and Atten_InOut.Deactivate=0");
                        ViewBag.AttendanceListSummury = EmployeeListSummery;
                    }

                    var GetEmployeeCardNo = DapperORM.DynamicQuerySingle(@"Select EmployeeCardNo from Mas_Employee where EmployeeId=" + OBJ.EmployeeID + " and Deactivate=0").FirstOrDefault();
                    var EmployeeCardNo = GetEmployeeCardNo.EmployeeCardNo;
                    DynamicParameters paramLogTime = new DynamicParameters();
                    paramLogTime.Add("@query", @"Select convert(date,LogDate)as LogDate,Direction  from DeviceLogs where deactivate=0 
                                                and UserId='" + EmployeeCardNo + "' and convert(date,LogDate) between CONVERT(date, '" + OBJ.FromDate + "', 103)  and CONVERT(date, '" + OBJ.ToDate + "', 103)");
                    var Logdata = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramLogTime).ToList();
                    ViewBag.EmployeeLog = Logdata;

                    //DynamicParameters param = new DynamicParameters();
                    //param.Add("@p_EmployeeID", OBJ.EmployeeID);
                    //var Shiftdata = DapperORM.ExecuteSP<dynamic>("sp_GetShift", param).ToList();
                    //ViewBag.EmployeeShifts = Shiftdata;

                    var EmpName = DapperORM.DynamicQuerySingle(@"Select EmployeeName from Mas_Employee where EmployeeId=" + OBJ.EmployeeID + "").FirstOrDefault();
                    ViewBag.EmpName = EmpName.EmployeeName;
                }
                else
                {
                    if (Session["EmployeeList"] != "")
                    {
                        ViewBag.AttendanceListNew = Session["EmployeeList"];
                        //ViewBag.AttendanceList = null;
                        Session["EmployeeList"] = "";
                    }
                    else
                    {
                        ViewBag.AttendanceListNew = null;
                    }
                    ViewBag.AttendanceListSummury = null;
                    ViewBag.AttendanceList = null;
                    ViewBag.EmployeeLog = "";
                    ViewBag.EmployeeShifts = "";
                    ViewBag.EmpName = "";
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

        public ActionResult SaveUpdate(List<EmployeeWiseAttendanceList> tbldata)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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
                var GetEmployeeCardNo = DapperORM.DynamicQuerySingle(@"Select EmployeeCardNo from Mas_Employee where EmployeeId=" + EmployeeId + " and Deactivate=0").FirstOrDefault();
                var EmployeeCardNo = GetEmployeeCardNo.EmployeeCardNo;
                DynamicParameters paramLogTime = new DynamicParameters();
                paramLogTime.Add("@query", @"Select LogDate,Direction  from DeviceLogs where deactivate=0 
                                            and UserId='" + EmployeeCardNo + "' and convert(date,LogDate) between '" + LogFromDates + "' and '" + LogToDates + "'");
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
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and EmployeeBranchId= " + UnitBranchId + " and Mas_Employee.EmployeeLeft=0 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
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

        public ActionResult CalculateData(List<EmployeeWiseAttendanceReg> tbldata, int EmployeeId) // EmployeeWiseAttendanceReg
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                //USING SP GET CALCULATION PASS dt AS PARAMETER USING TYPE IN STORE PROCEDURE
                DataTable dt = ConvertToDataTable(tbldata);
                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@AttendanceRegularization", dt, DbType.Object, ParameterDirection.Input);
                var GetData = DapperORM.ExecuteSP<dynamic>("InsertAttendanceRegularization", ARparam).ToList();

                Session["EmployeeList"] = GetData;
                return Json(new { data = GetData }, JsonRequestBehavior.AllowGet);

                //MY OLD CALCULATION CODE



                //var EmployeeId = 3178;
                dt_Shift = objcon.GetDataTable(@"Select SName,ShiftName,BeginTime,EndTime,Duration,ShiftDurationForOTCalculation 
                                            from Atten_Shifts where ShiftId in (select Atten_ShiftGroupShifts_ShiftId from Atten_ShiftGroupShifts
                                            where Atten_ShiftGroupShifts_ShiftGroupId in 
                                            (Select EM_Atten_ShiftGroupId from Mas_Employee_Attendance where AttendanceEmployeeId = " + EmployeeId + "))");
                dt_ShiftrotationalWeekOff = objcon.GetDataTable(@"Select RotationalWeekOffId from Atten_RotationalWeekOff where RotationalWeekOffEmployeeId=" + EmployeeId + "");

                dt_COff_Generation = objcon.GetDataTable(@"Select CoffRegularDay,CoffRegularCoff0_5Day_Min,CoffRegularCoff0_5Day_Max,CoffRegularCoff1Day_Min
                                                        , CoffRegularCoff1Day_Max, CoffRegularCoff1_5Day_Min, CoffRegularCoff1_5Day_Max, CoffRegularCoff2Day_Min
                                                        , CoffRegularCoff2Day_Max, CoffWOPHCoff0_5day_Min, CoffWOPHCoff0_5day_Max, CoffWOPHCoff1day_Min
                                                        , CoffWOPHCoff1day_Max, CoffWOPHCoff1_5day_Min, CoffWOPHCoff1_5day_Max, CoffWOPHCoff2day_Min, CoffWOPHCoff2day_Max
                                                        from Atten_CoffSetting where Deactivate = 0 and
                                                        CoffId = (Select EM_Atten_CoffSettingId from Mas_Employee_Attendance where EM_Atten_CoffApplicable = 1 and Deactivate = 0 and AttendanceEmployeeId = " + EmployeeId + ")");
                //OutSide Function
                dt_PH_Check = objcon.GetDataTable(@"Select PublicHolidayDate from Atten_PublicHoliday
                                                    where CmpId=(Select CmpID from Mas_Employee where EmployeeId=" + EmployeeId + ") and PublicHolidayBranchId=(Select EmployeeBranchId from Mas_Employee where EmployeeId=" + EmployeeId + ")");

                //OutSide Function Check WeekOff Day and Get ShiftRuleId and EM_Atten_OT_Applicable Check
                dt_WO_Check = objcon.GetDataTable(@"Select EM_Atten_WOFF1,EM_Atten_CoffApplicable,EM_Atten_ShiftRuleId,EM_Atten_OT_Applicable from Mas_Employee_Attendance where Deactivate=0 and AttendanceEmployeeId=" + EmployeeId + "");
                var dt_WO = (from row in dt_WO_Check.AsEnumerable()
                             select row).Last();
                var GetShiftRuleId = dt_WO.ItemArray[2];
                var OT_Applicable = dt_WO.ItemArray[3];//true/false

                dt_ShiftRule = objcon.GetDataTable(@"Select MinOT from Atten_ShiftRule where ShiftRuleId=" + GetShiftRuleId + "");
                var dtMinOT = (from row in dt_ShiftRule.AsEnumerable()
                               select row).Last();
                var dtMinOTCheck = dtMinOT.ItemArray[0];



                //var vardt_WO_Check = (from row in dt_WO_Check.AsEnumerable()
                //                      where row.Field<string>("MinOT") == GetdayOfDate
                //                      select EM_Atten_ShiftRuleId).ToList();

                //var vardt_WO_Check = (from row in dt_ShiftRule.AsEnumerable()
                //                      where row.Field<string>("MinOT") == GetdayOfDate
                //                      select row).ToList();

                List<EmployeeWiseAttendanceReg> ListEmp = new List<EmployeeWiseAttendanceReg>();

                for (int i = 0; i < tbldata.Count; i++)
                {
                    var ShiftName = "";
                    var List_ShiftName = "";
                    GetShift_Name(tbldata[i].Shift.ToString(), out ShiftName);
                    List_ShiftName = ShiftName;
                    EmployeeWiseAttendanceReg ObjListEmp = new EmployeeWiseAttendanceReg();

                    string Mydate = tbldata[i].Date;
                    DateTime date = DateTime.Parse(Mydate);
                    string SetDay = date.ToString("ddd");

                    if (tbldata[i].InTime == null && tbldata[i].OutTime == null)
                    {

                        ObjListEmp.Date = Convert.ToString(tbldata[i].Date);
                        ObjListEmp.Day = Convert.ToString(SetDay);
                        if (tbldata[i].InTime == null)
                        {
                            ObjListEmp.InTime = Convert.ToString("2999-01-01T00:00");
                        }
                        if (tbldata[i].InTime == null)
                        {
                            ObjListEmp.OutTime = Convert.ToString("2999-01-01T00:00");
                        }
                        else
                        {
                            ObjListEmp.InTime = Convert.ToString(tbldata[i].InTime);
                            ObjListEmp.OutTime = Convert.ToString(tbldata[i].OutTime);
                        }

                        ObjListEmp.Status = Convert.ToString(tbldata[i].Status);
                        ObjListEmp.Shift = Convert.ToString(tbldata[i].Shift);

                        ObjListEmp.TotalDuration = Convert.ToString(tbldata[i].TotalDuration);
                        ObjListEmp.OverTime = Convert.ToString(tbldata[i].OverTime);
                        ObjListEmp.LateComing = Convert.ToString(tbldata[i].LateComing);
                        ObjListEmp.EarlyGoing = Convert.ToString(tbldata[i].EarlyGoing);

                        ObjListEmp.NoOffCOFF = Convert.ToString(tbldata[i].NoOffCOFF);
                        ObjListEmp.Remark = Convert.ToString(tbldata[i].Remark);
                        ListEmp.Add(ObjListEmp);
                        continue;
                        //return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                    }

                    //CHECK SHIFT AND SET SHIFT
                    DateTime dateTimeIn = DateTime.Parse(tbldata[i].InTime);
                    DateTime dateTimeOut = DateTime.Parse(tbldata[i].OutTime);
                    string SetShiftName = "";
                    string dtShiftName = "";
                    string dtShiftFullName = "";
                    TimeSpan dtShiftBeginTime = TimeSpan.Zero;
                    TimeSpan dtShiftOutTime = TimeSpan.Zero;
                    int dtShiftDuration = 0;
                    int dtShiftOTDuration = 0;

                    if (tbldata[i].Shift == "-" && tbldata[i].InTime != null && tbldata[i].OutTime != null)
                    {
                        var GetShiftName = (from row in dt_Shift.AsEnumerable()
                                            let beginTime = row.Field<TimeSpan>("BeginTime")
                                            let endTime = row.Field<TimeSpan>("EndTime")
                                            where dateTimeIn.TimeOfDay >= beginTime && dateTimeIn.TimeOfDay <= endTime
                                            select row).LastOrDefault();

                        if (GetShiftName != null)
                        {
                            SetShiftName = GetShiftName.Field<string>("SName");

                            var dtGetDetailShiftWise = (from row in dt_Shift.AsEnumerable()
                                                        where row.Field<string>("SName") == SetShiftName
                                                        select row).LastOrDefault();

                            if (dtGetDetailShiftWise != null)
                            {
                                dtShiftName = dtGetDetailShiftWise.Field<string>(0);
                                dtShiftFullName = dtGetDetailShiftWise.Field<string>(1);
                                dtShiftBeginTime = dtGetDetailShiftWise.Field<TimeSpan>(2);
                                dtShiftOutTime = dtGetDetailShiftWise.Field<TimeSpan>(3);
                                dtShiftDuration = dtGetDetailShiftWise.Field<int>(4);
                                dtShiftOTDuration = dtGetDetailShiftWise.Field<int>(5);
                            }
                        }
                    }
                    else
                    {
                        SetShiftName = tbldata[i].Shift;

                        var dtGetDetailShiftWise = (from row in dt_Shift.AsEnumerable()
                                                    where row.Field<string>("SName") == SetShiftName
                                                    select row).LastOrDefault();

                        if (dtGetDetailShiftWise != null)
                        {
                            dtShiftName = dtGetDetailShiftWise.Field<string>(0);
                            dtShiftFullName = dtGetDetailShiftWise.Field<string>(1);
                            dtShiftBeginTime = dtGetDetailShiftWise.Field<TimeSpan>(2);
                            dtShiftOutTime = dtGetDetailShiftWise.Field<TimeSpan>(3);
                            dtShiftDuration = dtGetDetailShiftWise.Field<int>(4);
                            dtShiftOTDuration = dtGetDetailShiftWise.Field<int>(5);
                        }
                    }


                    //if (tbldata[i].Shift == "-" && (tbldata[i].InTime != null && tbldata[i].OutTime != null))
                    //{
                    //    var GetShiftName = (from row in dt_Shift.AsEnumerable()
                    //                        let beginTime = row.Field<TimeSpan>("BeginTime")
                    //                        let endTime = row.Field<TimeSpan>("EndTime")
                    //                        where (dateTimeIn.TimeOfDay >= beginTime && dateTimeIn.TimeOfDay <= endTime)
                    //                        select row).LastOrDefault();
                    //    SetShiftName = GetShiftName.ItemArray[0];

                    //    //GET TOTAL DURATION BASED ON SHIFT NAME VALIDATION
                    //    var dtGetDetailShiftWise = (from row in dt_Shift.AsEnumerable()
                    //                                where row.Field<string>("SName") == SetShiftName
                    //                                select row).Last();
                    //    dtShiftName = dtGetDetailShiftWise.ItemArray[0];
                    //    dtShiftFullName = dtGetDetailShiftWise.ItemArray[1];
                    //    dtShiftBeginTime = dtGetDetailShiftWise.ItemArray[2];
                    //    dtShiftOutTime = dtGetDetailShiftWise.ItemArray[3];
                    //    dtShiftDuration = dtGetDetailShiftWise.ItemArray[4];
                    //    dtShiftOTDuration = dtGetDetailShiftWise.ItemArray[5];
                    //}
                    //else
                    //{
                    //    SetShiftName = tbldata[i].Shift;
                    //    //GET TOTAL DURATION BASED ON SHIFT NAME VALIDATION
                    //    var dtGetDetailShiftWise = (from row in dt_Shift.AsEnumerable()
                    //                                where row.Field<string>("SName") == SetShiftName
                    //                                select row).Last();
                    //    dtShiftName = dtGetDetailShiftWise.ItemArray[0];
                    //    dtShiftFullName = dtGetDetailShiftWise.ItemArray[1];
                    //    dtShiftBeginTime = dtGetDetailShiftWise.ItemArray[2];
                    //    dtShiftOutTime = dtGetDetailShiftWise.ItemArray[3];
                    //    dtShiftDuration = dtGetDetailShiftWise.ItemArray[4];
                    //    dtShiftOTDuration = dtGetDetailShiftWise.ItemArray[5];
                    //}


                    // Define variables with appropriate types and initial values
                    Object dtCoffRegularDay = null;
                    Object dtCoffRegularCoff0_5Day_Min = null;
                    Object dtCoffRegularCoff0_5Day_Max = null;
                    Object dtCoffRegularCoff1Day_Min = null;
                    Object CoffRegularCoff1Day_Max = null;
                    Object CoffRegularCoff1_5Day_Min = null;
                    Object CoffRegularCoff1_5Day_Max = null;
                    Object CoffRegularCoff2Day_Min = null;
                    Object CoffRegularCoff2Day_Max = null;

                    Object CoffWOPHCoff0_5day_Min = null;
                    Object CoffWOPHCoff0_5day_Max = null;
                    Object CoffWOPHCoff1day_Min = null;
                    Object CoffWOPHCoff1day_Max = null;
                    Object CoffWOPHCoff1_5day_Min = null;
                    Object CoffWOPHCoff1_5day_Max = null;
                    Object CoffWOPHCoff2day_Min = null;
                    Object CoffWOPHCoff2day_Max = null;
                    if (dt_COff_Generation.Rows.Count > 0)
                    {
                        var dtCOffGeneration = (from row in dt_COff_Generation.AsEnumerable()
                                                select row).Last();
                        if (dtCOffGeneration != null)
                        {
                            dtCoffRegularDay = dtCOffGeneration.ItemArray[0];
                        }
                        dtCoffRegularCoff0_5Day_Min = dtCOffGeneration.ItemArray[1];
                        dtCoffRegularCoff0_5Day_Max = dtCOffGeneration.ItemArray[2];
                        dtCoffRegularCoff1Day_Min = dtCOffGeneration.ItemArray[3];
                        CoffRegularCoff1Day_Max = dtCOffGeneration.ItemArray[4];
                        CoffRegularCoff1_5Day_Min = dtCOffGeneration.ItemArray[5];
                        CoffRegularCoff1_5Day_Max = dtCOffGeneration.ItemArray[6];
                        CoffRegularCoff2Day_Min = dtCOffGeneration.ItemArray[7];
                        CoffRegularCoff2Day_Max = dtCOffGeneration.ItemArray[8];

                        CoffWOPHCoff0_5day_Min = dtCOffGeneration.ItemArray[9];
                        CoffWOPHCoff0_5day_Max = dtCOffGeneration.ItemArray[10];
                        CoffWOPHCoff1day_Min = dtCOffGeneration.ItemArray[11];
                        CoffWOPHCoff1day_Max = dtCOffGeneration.ItemArray[12];
                        CoffWOPHCoff1_5day_Min = dtCOffGeneration.ItemArray[13];
                        CoffWOPHCoff1_5day_Max = dtCOffGeneration.ItemArray[14];
                        CoffWOPHCoff2day_Min = dtCOffGeneration.ItemArray[15];
                        CoffWOPHCoff2day_Max = dtCOffGeneration.ItemArray[16];
                    }


                    string fromDateString = tbldata[i].InTime;
                    string toDateString = tbldata[i].OutTime;

                    // Parse the date and time strings into DateTime objects
                    DateTime tblfromDate = DateTime.Parse(fromDateString);
                    DateTime tbltoDate = DateTime.Parse(toDateString);
                    TimeSpan tblfromTime = tblfromDate.TimeOfDay; // GetInTime-InTable
                    TimeSpan tbltoTime = tbltoDate.TimeOfDay; // GetOutTime-InTable

                    //THIS CODE FOR TOTAL DURATION CALCULATION 
                    TimeSpan timeDifference = tbltoDate - tblfromDate;
                    int GetDurationInMin = Convert.ToInt32(timeDifference.TotalMinutes);

                    int minutes = GetDurationInMin;
                    int hours = minutes / 60;
                    int remainingMinutes = minutes % 60;
                    string GetDurationHRForSet1 = $"{hours}:{remainingMinutes:D2}";

                    int GetDurationForSet = GetDurationInMin;
                    string GetDurationHRForSet = "";
                    GetDurationHRForSet = GetDurationHRForSet1;
                    if (Convert.ToUInt32(GetDurationInMin) < 0)
                    {
                        GetDurationHRForSet = tbldata[i].TotalDuration;
                    }
                    else
                    {
                        GetDurationHRForSet = GetDurationHRForSet1;
                    }

                    //THIS CODE FOR OVER TIME (OT) CALCULATION
                    int GetOTMin = (Int32)GetDurationForSet - (Int32)dtShiftOTDuration;
                    TimeSpan GetOTHR = TimeSpan.FromMinutes(GetOTMin);
                    string GetOTForSet1 = GetOTHR.ToString(@"hh\:mm");
                    string GetOTForSet = "";
                    if (Convert.ToBoolean(OT_Applicable) == true) //OT Applicable true then go inside
                    {
                        if (GetOTMin < 0)
                        {
                            GetOTForSet = tbldata[i].OverTime;
                        }
                        else
                        {
                            GetOTForSet = GetOTForSet1;
                        }
                    }
                    else
                    {
                        GetOTForSet = tbldata[i].OverTime;
                    }

                    //THIS CODE FOR MIN OT CHECK AND THEN APPLY OT
                    var GetOTTimeInMin = Convert.ToInt32(dtMinOTCheck);
                    if (GetOTTimeInMin > GetOTMin)
                    {
                        TimeSpan CheckOT = TimeSpan.FromMinutes(0);
                        GetOTForSet = CheckOT.ToString(@"hh\:mm");
                    }

                    //THIS CODE FOR GET LATE COMING MINUTES 
                    TimeSpan dtShiftBeginTimeSpan;
                    string GetLCForSet1 = "";
                    double totalLCMinutesDifference = 0;
                    if (TimeSpan.TryParse(dtShiftBeginTime.ToString(), out dtShiftBeginTimeSpan))
                    {
                        TimeSpan LCdifference = tblfromTime - dtShiftBeginTimeSpan;
                        totalLCMinutesDifference = LCdifference.TotalMinutes;
                        TimeSpan GetLCMin = TimeSpan.FromMinutes(totalLCMinutesDifference);
                        GetLCForSet1 = GetLCMin.ToString(@"hh\:mm");
                    }
                    string GetLCForSet = "";
                    var CheckConvertLC = Convert.ToInt32(totalLCMinutesDifference);
                    if (CheckConvertLC < 0)
                    {
                        GetLCForSet = tbldata[i].LateComing;
                    }
                    else
                    {
                        GetLCForSet = GetLCForSet1;
                    }

                    //THIS CODE FOR GET EARLY GOING MINUTES 
                    TimeSpan dtShiftBegoutTimeSpan;
                    string GetEGForSet1 = "";
                    double totalEGMinutesDifference = 0;
                    if (TimeSpan.TryParse(dtShiftOutTime.ToString(), out dtShiftBegoutTimeSpan))
                    {
                        TimeSpan EGdifference = dtShiftBegoutTimeSpan - tbltoTime;
                        totalEGMinutesDifference = EGdifference.TotalMinutes;
                        TimeSpan GetEGMin = TimeSpan.FromMinutes(totalEGMinutesDifference);
                        GetEGForSet1 = GetEGMin.ToString(@"hh\:mm");
                    }
                    string GetEGForSet = "";
                    var CheckConvertEG = Convert.ToInt32(totalEGMinutesDifference);
                    if (CheckConvertEG < 0)
                    {
                        GetEGForSet = tbldata[i].EarlyGoing;
                    }
                    else
                    {
                        GetEGForSet = GetEGForSet1;
                    }

                    //THIS CODE FOR PUBLIC WEEKOFF CHECK VALIDATION
                    DayOfWeek dayOfWeek = tblfromDate.DayOfWeek;
                    string GetdayOfDate = dayOfWeek.ToString();
                    var vardt_WO_Check = (from row in dt_WO_Check.AsEnumerable()
                                          where row.Field<string>("EM_Atten_WOFF1") == GetdayOfDate
                                          select row).ToList();

                    //THIS CODE FOR PUBLIC HOLIDAY CHECK VALIDATION
                    var vardt_PH_Check = (from row in dt_PH_Check.AsEnumerable()
                                          where row.Field<DateTime>("PublicHolidayDate") == tblfromDate
                                          select row).ToList(); // Convert to List

                    //THIS CODE FOR CHECK PH
                    double SetNoOffCOFF = 0;
                    if (vardt_PH_Check.Count() != 0)
                    {
                        SetNoOffCOFF = Convert.ToDouble(tbldata[i].NoOffCOFF);
                        if (GetDurationInMin > Convert.ToInt32(CoffWOPHCoff0_5day_Min) && GetDurationInMin < Convert.ToInt32(CoffWOPHCoff0_5day_Max))
                        {
                            SetNoOffCOFF = 0.5;
                        }
                        if (GetDurationInMin > Convert.ToInt32(CoffWOPHCoff1day_Min) && GetDurationInMin < Convert.ToInt32(CoffWOPHCoff1day_Max))
                        {
                            SetNoOffCOFF = 1;
                        }
                        if (GetDurationInMin > Convert.ToInt32(CoffWOPHCoff1_5day_Min) && GetDurationInMin < Convert.ToInt32(CoffWOPHCoff1_5day_Max))
                        {
                            SetNoOffCOFF = 1.5;
                        }
                        if (GetDurationInMin > Convert.ToInt32(CoffWOPHCoff2day_Min) && GetDurationInMin < Convert.ToInt32(CoffWOPHCoff2day_Max))
                        {
                            SetNoOffCOFF = 2;
                        }
                    }
                    else if (Convert.ToBoolean(dtCoffRegularDay) == true) // WHEN COFF ALLOW FOR EMPLOYEE
                    {
                        SetNoOffCOFF = Convert.ToDouble(tbldata[i].NoOffCOFF);
                        if (GetDurationInMin > Convert.ToInt32(dtCoffRegularCoff0_5Day_Min) && GetDurationInMin < Convert.ToInt32(dtCoffRegularCoff0_5Day_Max))
                        {
                            SetNoOffCOFF = 0.5;
                        }
                        if (GetDurationInMin > Convert.ToInt32(dtCoffRegularCoff1Day_Min) && GetDurationInMin < Convert.ToInt32(CoffRegularCoff1Day_Max))
                        {
                            SetNoOffCOFF = 1;
                        }
                        if (GetDurationInMin > Convert.ToInt32(CoffRegularCoff1_5Day_Min) && GetDurationInMin < Convert.ToInt32(CoffRegularCoff1_5Day_Max))
                        {
                            SetNoOffCOFF = 1.5;
                        }
                        if (GetDurationInMin > Convert.ToInt32(CoffRegularCoff2Day_Min) && GetDurationInMin < Convert.ToInt32(CoffRegularCoff2Day_Max))
                        {
                            SetNoOffCOFF = 2;
                        }
                    }

                    // THIS IS FINAL CALCULATION AND SET TABLE
                    ObjListEmp.Date = Convert.ToString(tbldata[i].Date);//Done
                    ObjListEmp.Day = Convert.ToString(SetDay);//Done
                    ObjListEmp.InTime = Convert.ToString(tbldata[i].InTime);//Done
                    ObjListEmp.OutTime = Convert.ToString(tbldata[i].OutTime);//Done
                    ObjListEmp.Status = Convert.ToString(tbldata[i].Status);//Done
                    ObjListEmp.Shift = Convert.ToString(SetShiftName);//Done

                    ObjListEmp.TotalDuration = Convert.ToString(GetDurationHRForSet);//Check Done
                    ObjListEmp.OverTime = Convert.ToString(GetOTForSet);//Check Done
                    ObjListEmp.LateComing = Convert.ToString(GetLCForSet);//Check Done
                    ObjListEmp.EarlyGoing = Convert.ToString(GetEGForSet);//Check Done

                    ObjListEmp.NoOffCOFF = Convert.ToString(SetNoOffCOFF);//Pending
                    ObjListEmp.Remark = Convert.ToString(tbldata[i].Remark); //Done
                    ListEmp.Add(ObjListEmp);
                }

                //Session["EmployeeList"] = ListEmp;
                //return Json(new { data = ListEmp }, JsonRequestBehavior.AllowGet);

                //return RedirectToAction("ESS_TimeOffice_EmployeeWiseAttendance", "ESS_TimeOffice_EmployeeWiseAttendance");
                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        static int TimeToMinutes(string time)
        {
            string[] parts = time.Split(':');
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            return hours * 60 + minutes;
        }

        public ActionResult FinalSubmit(List<EmployeeWiseAttendanceReg> tbldata, int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                using (var connection = new SqlConnection(DapperORM.connectionString))
                {

                    for (int i = 0; i < tbldata.Count; i++)
                    {
                        // Parse the time string and Get the total minutes
                        string TotalDurationtime = tbldata[i].TotalDuration;
                        int TotalDurationMinutes = TimeToMinutes(TotalDurationtime);
                        int TotalDuration = TotalDurationMinutes;

                        string OverTimetime = tbldata[i].OverTime;
                        int OverTimeMinutes = TimeToMinutes(OverTimetime);
                        int OverTime = OverTimeMinutes;

                        string EarlyGoingtime = tbldata[i].EarlyGoing;
                        int EarlyGoingMinutes = TimeToMinutes(EarlyGoingtime);
                        int EarlyGoing = EarlyGoingMinutes;

                        string LateComingtime = tbldata[i].LateComing;
                        int LateComingMinutes = TimeToMinutes(LateComingtime);
                        int LateComing = LateComingMinutes;

                        DateTime parsedDate = DateTime.ParseExact(tbldata[i].Date, "dd-MM-yyyy", null);
                        string PassformattedDate = parsedDate.ToString("yyyy-MM-dd");

                        List<AttendanceRegu_Atten_InOut> EmployeeWiseAtten = new List<AttendanceRegu_Atten_InOut>();
                        var CheckIsExist = DapperORM.DynamicQuerySingle(@"Select Count(InOutID) as GetCount from Atten_InOut_Test where InOutEmployeeId=" + EmployeeId + " and InOutDate='" + PassformattedDate + "'").FirstOrDefault();
                        var CheckIsExists = CheckIsExist.GetCount();
                        if (CheckIsExists > 0)
                        {
                            if (tbldata[i].InTime != null & tbldata[i].OutTime != null)
                            {
                                var sql1 = @"Update Atten_InOut_Test Set
                                        Deactivate=@Deactivate , CreatedBy=@CreatedBy, CreatedDate=@CreatedDate, ModifiedBy=@ModifiedBy, 
                                        ModifiedDate=@ModifiedDate, MachineName=@MachineName, CmpId=@CmpId, 
                                        InOutBranchId=@InOutBranchId, InOutDate=@InOutDate, InOutIntime=@InOutIntime,
                                        InOutOutTime=@InOutOutTime, InOutStatus=@InOutStatus, InOutShift=@InOutShift,
                                        InOutDuration=@InOutDuration, InOutOT_End=@InOutOT_End, InOutLateMark=@InOutLateMark,
                                        InOutLateBy=@InOutLateBy, InOutEarlyMark=@InOutEarlyMark, InOutEarlyBy=@InOutEarlyBy ,
                                        InOutCoff=@InOutCoff , InOutAdjRemark=@InOutAdjRemark 
                                        Where InOutEmployeeId = @InOutEmployeeId and InOutDate= @InOutDate";
                                EmployeeWiseAtten.Add(new AttendanceRegu_Atten_InOut()
                                {
                                    Deactivate = false,
                                    CreatedBy = Convert.ToString(Session["EmployeeName"]),
                                    CreatedDate = currentDate,
                                    ModifiedBy = Convert.ToString(Session["EmployeeName"]),
                                    ModifiedDate = currentDate,
                                    MachineName = System.Net.Dns.GetHostName().ToString(),
                                    CmpId = 1,
                                    InOutBranchId = 1,
                                    InOutIntime = Convert.ToDateTime(tbldata[i].InTime),
                                    InOutOutTime = Convert.ToDateTime(tbldata[i].OutTime),
                                    InOutStatus = tbldata[i].Status,
                                    InOutShift = tbldata[i].Shift,
                                    InOutDuration = Convert.ToDouble(TotalDuration),
                                    InOutOT_End = Convert.ToDouble(OverTime),
                                    InOutLateMark = 0,
                                    InOutLateBy = Convert.ToDouble(LateComing),
                                    InOutEarlyMark = 1,
                                    //LateComing = tbldata[i].LateComing, //This Column not in DB
                                    InOutEarlyBy = Convert.ToDouble(EarlyGoing),
                                    InOutCoff = Convert.ToDouble(tbldata[i].NoOffCOFF),
                                    InOutAdjRemark = tbldata[i].Remark,

                                    InOutEmployeeId = EmployeeId,
                                    InOutDate = Convert.ToDateTime(tbldata[i].Date),
                                });
                                var rowsAffected1 = connection.Execute(sql1, EmployeeWiseAtten);
                            }
                        }
                        else
                        {
                            if (tbldata[i].InTime != null & tbldata[i].OutTime != null)
                            {
                                var sql = @"INSERT INTO Atten_InOut_Test
                                     (Deactivate,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,MachineName,
                                      CmpId,InOutBranchId,InOutDate,InOutEmployeeId,InOutIntime , InOutOutTime ,
                                      InOutStatus , InOutShift , InOutDuration , InOutOT_End , InOutLateMark , 
                                      InOutLateBy ,InOutEarlyMark,InOutEarlyBy)
                              VALUES (@Deactivate,@CreatedBy,@CreatedDate,@ModifiedBy,@ModifiedDate,@MachineName, 
                                      @CmpId,@InOutBranchId,@InOutDate , @InOutEmployeeId , @InOutIntime , @InOutOutTime , 
                                      @InOutStatus , @InOutShift , @InOutDuration , @InOutOT_End , @InOutLateMark ,
                                      @InOutLateBy, @InOutEarlyMark, @InOutEarlyBy)";
                                EmployeeWiseAtten.Add(new AttendanceRegu_Atten_InOut()
                                {
                                    Deactivate = false,
                                    CreatedBy = Convert.ToString(Session["EmployeeName"]),
                                    CreatedDate = currentDate,
                                    ModifiedBy = Convert.ToString(Session["EmployeeName"]),
                                    ModifiedDate = currentDate,
                                    MachineName = System.Net.Dns.GetHostName().ToString(),
                                    CmpId = 1,
                                    InOutBranchId = 1,
                                    InOutEmployeeId = EmployeeId,
                                    InOutDate = Convert.ToDateTime(tbldata[i].Date),
                                    InOutIntime = Convert.ToDateTime(tbldata[i].InTime),
                                    InOutOutTime = Convert.ToDateTime(tbldata[i].OutTime),
                                    InOutStatus = tbldata[i].Status,
                                    InOutShift = tbldata[i].Shift,
                                    InOutDuration = Convert.ToDouble(TotalDuration),
                                    InOutOT_End = Convert.ToDouble(OverTime),
                                    InOutLateMark = 0,
                                    InOutLateBy = Convert.ToDouble(LateComing),
                                    InOutEarlyMark = 1,
                                    //LateComing = tbldata[i].LateComing, //This Column not in DB
                                    InOutEarlyBy = Convert.ToDouble(EarlyGoing),
                                    InOutCoff = Convert.ToDouble(tbldata[i].NoOffCOFF),
                                    InOutAdjRemark = tbldata[i].Remark

                                });
                                var rowsAffected = connection.Execute(sql, EmployeeWiseAtten);
                            }
                            else
                            {
                                var sql = @"INSERT INTO Atten_InOut_Test
                                     (Deactivate,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,MachineName,
                                      CmpId,InOutBranchId,InOutDate,InOutEmployeeId,
                                      InOutStatus , InOutShift , InOutDuration , InOutOT_End , InOutLateMark , 
                                      InOutLateBy ,InOutEarlyMark,InOutEarlyBy)
                              VALUES (@Deactivate,@CreatedBy,@CreatedDate,@ModifiedBy,@ModifiedDate,@MachineName, 
                                      @CmpId,@InOutBranchId,@InOutDate , @InOutEmployeeId , 
                                      @InOutStatus , @InOutShift , @InOutDuration , @InOutOT_End , @InOutLateMark ,
                                      @InOutLateBy, @InOutEarlyMark, @InOutEarlyBy)";
                                EmployeeWiseAtten.Add(new AttendanceRegu_Atten_InOut()
                                {
                                    Deactivate = false,
                                    CreatedBy = Convert.ToString(Session["EmployeeName"]),
                                    CreatedDate = currentDate,
                                    ModifiedBy = Convert.ToString(Session["EmployeeName"]),
                                    ModifiedDate = currentDate,
                                    MachineName = System.Net.Dns.GetHostName().ToString(),
                                    CmpId = 1,
                                    InOutBranchId = 1,
                                    InOutEmployeeId = EmployeeId,
                                    InOutDate = Convert.ToDateTime(tbldata[i].Date),
                                    //InOutIntime = Convert.ToDateTime(tbldata[i].InTime),
                                    //InOutOutTime = Convert.ToDateTime(tbldata[i].OutTime),
                                    InOutStatus = tbldata[i].Status,
                                    InOutShift = tbldata[i].Shift,
                                    InOutDuration = Convert.ToDouble(TotalDuration),
                                    InOutOT_End = Convert.ToDouble(OverTime),
                                    InOutLateMark = 0,
                                    InOutLateBy = Convert.ToDouble(LateComing),
                                    InOutEarlyMark = 1,
                                    //LateComing = tbldata[i].LateComing, //This Column not in DB
                                    InOutEarlyBy = Convert.ToDouble(EarlyGoing),
                                    InOutCoff = Convert.ToDouble(tbldata[i].NoOffCOFF),
                                    InOutAdjRemark = tbldata[i].Remark

                                });
                                var rowsAffected = connection.Execute(sql, EmployeeWiseAtten);
                            }
                        }
                    }
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

        void GetShift_Name(string Name, out string ShiftName)
        {
            try
            {
                var varShiftName = (from row in dt_Shift.AsEnumerable()
                                    where row.Field<string>("SName") == Name
                                    select row).Last();
                if (varShiftName == null)
                {
                    ShiftName = "";
                }
                else
                {
                    ShiftName = Convert.ToString(varShiftName["SName"]);
                }
            }
            catch
            {
                ShiftName = "";
            }
        }
        
    }
}