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
    public class ESS_TimeOffice_MultipleEmployeeWiseRegularizationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: ESS/ESS_TimeOffice_MultipleEmployeeWiseRegularization
        #region Main View
        public ActionResult ESS_TimeOffice_MultipleEmployeeWiseRegularization()
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

        #region GetEmployeeLogAndShift
        [HttpGet]
        public ActionResult GetEmployeeLogAndShift(string EmployeeNo, DateTime LogFromDate, DateTime LogToDate, int CmpId , int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var LogFromDates = LogFromDate.ToString("yyyy-MM-dd");
                var LogToDates = LogToDate.ToString("yyyy-MM-dd");
                var GetEmployeeCardNo = DapperORM.DynamicQuerySingle(@"Select EmployeeCardNo,EmployeeName,EmployeeNo from Mas_Employee where EmployeeNo='" + EmployeeNo + "' and EmployeeBranchId="+ BranchId + " and CmpID="+ CmpId + " and Deactivate=0").FirstOrDefault();
                if(GetEmployeeCardNo==null)
                {
                    return Json(new { data = false }, JsonRequestBehavior.AllowGet);
                }
                var EmployeeCardNo = GetEmployeeCardNo.EmployeeCardNo;
                DynamicParameters paramLogTime = new DynamicParameters();
                paramLogTime.Add("@query", @"Select LogDate,Direction  from DeviceLogs where deactivate=0 
                                            and UserId='" + GetEmployeeCardNo.EmployeeCardNo + "' and convert(date,LogDate) between '" + LogFromDates + "' and '" + LogToDates + "' Order By LogDate ");
                var Logdata = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramLogTime).ToList();

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeID", Session["EmployeeId"]);
                var Shiftdata = DapperORM.ExecuteSP<dynamic>("sp_GetShift", param).ToList();

                var EmployeeName = GetEmployeeCardNo.EmployeeName +" - " + GetEmployeeCardNo.EmployeeNo;

                return Json(new { Logdata = Logdata, Shiftdata = Shiftdata , EmployeeName= EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Final Submit
        public ActionResult FinalSubmit(List<GetAttendanceMultiEmp> tbldata , int CmpId , int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //for (int i = 0; i < tbldata.Count; i++)
                //{
                //    var AttenLockCount = DapperORM.DynamicQuerySingle("Select Count(AtdLockIDBranchId) as LockCount from Atten_Lock where Deactivate=0  and Month(AtdLockIDMonth)='" + tbldata[1].InOut_AttendanceDate.ToString("MM") + "'  and Year(AtdLockIDMonth) ='" + tbldata[1].InOut_AttendanceDate.ToString("yyyy") + "'  and AtdLockIDBranchId=" + BranchId + " and AtdLock=1");
                //    if (AttenLockCount.LockCount != 0)
                //    {
                //        var message1 = "Record can''t be saved because the month/year is already locked.";
                //        var Icon2 = "error";
                //        return Json(new { Message = message1, Icon = Icon2 }, JsonRequestBehavior.AllowGet);
                //    }
                //}

                //DataTable dt = new DataTable();
                //DapperORM dprObj = new DapperORM();
                //dt = dprObj.ConvertToDataTable(tbldata);
                string msg = "";
                DataTable dt = ConvertToDataTable(tbldata);
                var duplicates = dt.AsEnumerable()
                          .GroupBy(row => new { Col1 = row["InOut_AttendanceDate"], Col2 = row["InOut_EmployeeNo"] })
                          .Where(group => group.Count() > 1)
                          .Any();
                if(duplicates==true)
                {
                    msg = $"Duplicate entry not allow";
                    return Json(new { Message = msg, Icon = "error" }, JsonRequestBehavior.AllowGet);
                }
                if (dt.Rows.Count > 0)
                {

                    DateTime DateIn = new DateTime();
                    DateTime DateIn_Next = new DateTime();

                    DateTime DateOut = new DateTime();
                    DateTime DateOut_Next = new DateTime();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DateIn = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 6, 0, 0);
                        DateIn_Next = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 5, 59, 59);
                        DateIn_Next = DateIn_Next.AddDays(1);


                        DateOut = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 6, 0, 0);
                        DateOut_Next = new DateTime(Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Year, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Month, Convert.ToDateTime(dt.Rows[i]["InOut_AttendanceDate"].ToString()).Day, 9, 0, 0);
                        DateOut_Next = DateOut_Next.AddDays(1);

                        var SrNo = i + 1;
                        if (dt.Rows[i]["InOut_InTime"].ToString() != "")
                        {
                            if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) < DateIn)
                            {
                                msg = $"Invalid In Time (Before 6:00 AM) - Sr.No. {SrNo}";
                                break;
                            }

                            if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) > DateIn_Next)
                            {
                                msg = $"Invalid In Time (After 6:00 AM) - Sr.No. {SrNo}";
                                break;
                            }
                        }

                        if (dt.Rows[i]["InOut_OutTime"].ToString() != "")
                        {
                            if (Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()) < DateOut)
                            {
                                msg = $"Invalid Out Time (Before 6:00 AM) - Sr.No. {SrNo}";
                                break;
                            }

                            if (Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()) > DateOut_Next)
                            {
                                msg = $"Invalid Out Time (After 9:00 AM) - Sr.No. {SrNo}";
                                break;
                            }
                        }
                        if (dt.Rows[i]["InOut_InTime"].ToString() != "" && dt.Rows[i]["InOut_OutTime"].ToString() != "")
                        {
                            if (Convert.ToDateTime(dt.Rows[i]["InOut_InTime"].ToString()) > Convert.ToDateTime(dt.Rows[i]["InOut_OutTime"].ToString()))
                            {
                                msg = $"Invalid Date Range (In Time after Out Time) - Sr.No. {SrNo}";
                                break;
                            }
                        }
                    }
                }
                if (msg != "")
                {
                    return Json(new { Message = msg, Icon = "error" }, JsonRequestBehavior.AllowGet);
                }

                DynamicParameters ARparam = new DynamicParameters();
                ARparam.Add("@tbl_dt", dt.AsTableValuedParameter("tbl_Log_Multiple"));
                ARparam.Add("@p_Origin", "MultipleEmployeeWise");
                ARparam.Add("@p_CmpId", CmpId);
                ARparam.Add("@p_BranchId", BranchId);
                ARparam.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                ARparam.Add("@p_MachineName", Dns.GetHostName().ToString());
                ARparam.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                ARparam.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter  
                //ARparam.Add("@tbl_dt", dt, DbType.Object, ParameterDirection.Input);
                var GetData = DapperORM.ExecuteSP<dynamic>("Sp_Attendance_Regularization_MultipleEmployee_FromDate_ToDate", ARparam).ToList();
                var message = ARparam.Get<string>("@p_msg");
                var Icon = ARparam.Get<string>("@p_Icon");

                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
    }
}