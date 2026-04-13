using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_SingleEmployeeLeftController : Controller
    {
        // GET: Module/Module_Employee_SingleEmployeeLeft
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        BulkAccessClass Obj = new BulkAccessClass();
        clsCommonFunction objcon = new clsCommonFunction();
        #region MainView
        public ActionResult Module_Employee_SingleEmployeeLeft()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 445;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetComapnyName = Obj.GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                int CmpId = Convert.ToInt32(GetComapnyName[0].Id);
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                var GetBranchName = Obj.GetBusinessUnit(CmpId, EmpId);
                ViewBag.BranchName = GetBranchName;
                int BranchId = Convert.ToInt32(GetBranchName[0].Id);

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0 and ContractorId<>1 order by Name");
                //paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and Mas_Employee.EmployeeId in (Select distinct Atten_InOut.Inoutemployeeid from Atten_InOut  where deactivate=0 and Atten_InOut.InOutBranchId =" + BranchId + " and month( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("MM") + "' and year( Atten_InOut.InOutDate )='" + DateTime.Now.Date.ToString("yyyy") + "' and EmployeeId<>1) union select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from mas_employee where deactivate=0 and EmployeeLeft=0 and employeebranchid=" + BranchId + " and( month(mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("MM") + "' and year (mas_employee.JoiningDate)<='" + DateTime.Now.Date.ToString("yyyy") + "') and (month(LeavingDate)='" + DateTime.Now.Date.ToString("MM") + "' and year(mas_employee.LeavingDate)='" + DateTime.Now.Date.ToString("yyyy") + "' or mas_employee.LeavingDate is null) order by Name");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
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
                int EmpId = Convert.ToInt32(Session["EmployeeId"]);
                var Branch = Obj.GetBusinessUnit(CmpId, EmpId);

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + Branch[0].Id + " and Mas_Employee.EmployeeLeft=0");
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

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int CmpId, int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select  EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0 and ContractorId<>1 order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                return Json(new { EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SaveUpdate
        public ActionResult SaveUpdate(SingleEmployeeLeft ObjEmployeeLeft)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }


                StringBuilder strBuilder = new StringBuilder();

                // if (ObjEmployeeLeft.EmployeeId!=0)
                //{
                //var b = q.employeeid;
                //if (b > 0)
                //{
                //    TempData["Message"] = "Leaving date should not less than joining date";
                //    TempData["Icon"] = "error";
                //    return View();
                //}
                ////}

                string Employee_Admin = " Insert into Log_Mas_Employee  (EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy)    " +
                                        " Select EmployeeId, EmployeeId_Encrypted, Deactivate, UseBy, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, MachineName, CmpID, PreboardingFid, EmployeeBranchId, EmployeeOrigin, EmployeeSeries, EmployeeNo, EmployeeCardNo, LocalExpat, IsNRI, Salutation, EmployeeName, EmployeeLevelID, EmployeeWageID, EmployeeDepartmentID, EmployeeSubDepartmentName, EmployeeDesignationID, EmployeeGradeID, EmployeeGroupID, EmployeeTypeID, EmployeeCostCenterID, EmployeeZoneID, EmployeeUnitID, ContractorID, IsCriticalStageApplicable, EmployeeCriticalStageID, EmployeeLineID, JoiningDate, IsJoiningSpecial, JoiningStatus, TraineeDueDate, ProbationDueDate, ConfirmationDate, IsConfirmation, ConfirmationBy, CompanyMobileNo, CompanyMailID, ReportingHR, ReportingManager1, ReportingManager2, ReportingAccount, NoOfBranchTransfer, IsReasigned, ResignationDate, NoticePeriodDays, IsExit, ExitDate, EmployeeLeft, LeavingDate, LeavingReason, LeavingReasonPF, LeavingReasonESI, EM_PayrollLastDate, VMSApplicable, IsVMSMultipleLocation, LeftEmployeeApprovedBy from Mas_Employee where Mas_Employee.Deactivate = 0 and Mas_Employee.EmployeeId ='" + ObjEmployeeLeft.EmployeeId + "'    " +
                                        " " +
                                        " " +
                                        " Update Mas_Employee set EmployeeLeft=1 ,LeavingDate='" + Convert.ToDateTime(ObjEmployeeLeft.LeftDate).ToString("yyyy-MM-dd") + "',LeftEmployeeApprovedBy='" + Session["EmployeeName"] + "' where EmployeeId='" + ObjEmployeeLeft.EmployeeId + "'" +
                                        " " +
                                        " " +
                                        "Update Atten_InOut set Atten_InOut.Deactivate=1,Atten_InOut.InOutRegNote=concat(InOutRegNote,', EmployeeLeft') where Atten_InOut.InOutEmployeeId='" + ObjEmployeeLeft.EmployeeId + "' and InOutDate >'" + Convert.ToDateTime(ObjEmployeeLeft.LeftDate).ToString("yyyy-MM-dd") + "' " +
                                        " ";

                strBuilder.Append(Employee_Admin);
                string abc = "";
                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {

                    TempData["Message"] = "Record save successfully";
                    TempData["Icon"] = "success";
                }
                if (abc != "")
                {
                    DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                                                                        "   Error_Desc " +
                                                                        " , Error_FormName " +
                                                                        " , Error_MachinceName " +
                                                                        " , Error_Date " +
                                                                        " , Error_UserID " +
                                                                        " , Error_UserName " + ") values (" +
                                                                        "'" + strBuilder + "'," +
                                                                        "'SingleEmployeeLeft'," +
                                                                        "'" + Dns.GetHostName().ToString() + "'," +
                                                                        "GetDate()," +
                                                                        "'" + Session["EmployeeId"] + "'," +
                                                                        "'" + Session["EmployeeName"] + "'");
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                }
                return RedirectToAction("Module_Employee_SingleEmployeeLeft", "Module_Employee_SingleEmployeeLeft");
                // Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        public ActionResult CheckJoiningDate(int? EmployeeId, string LeftDate)
        {
            try
            {
                var q = DapperORM.DynamicQuerySingle("Select replace(convert(nvarchar(12),JoiningDate,106),' ','/') as JoiningDate from Mas_Employee where employeeid=" + EmployeeId + " and JoiningDate>='" + LeftDate + "'").FirstOrDefault();
                var b = q.JoiningDate;
                if (b != null)
                {
                    TempData["Message"] = "Leaving date should not less than " + b + "";
                    TempData["Icon"] = "error";
                    //return View();
                }
                //}
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }


        }
    }
}