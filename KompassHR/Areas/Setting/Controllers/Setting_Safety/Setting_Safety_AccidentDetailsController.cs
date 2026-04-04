using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Safety
{
    public class Setting_Safety_AccidentDetailsController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Safety_AccidentDetails
        #region AccidentDetails
        public ActionResult Setting_Safety_AccidentDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Safety_AccidentDetails";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.GetBranchName = "";
                ViewBag.SubUnitList = "";
                ViewBag.GetShiftList = "";


                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }


        }
        #endregion

        #region GetList
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
            return View();
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

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + CmpId + " and Mas_Employee.EmployeeLeft=0");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                //ViewBag.EmployeeName = EmployeeName;

                DynamicParameters paramLine = new DynamicParameters();
                paramLine.Add("@query", "Select LineId as Id,LineName as Name from Mas_LineMaster where Mas_LineMaster.Deactivate=0 and Mas_LineMaster.CmpId=" + CmpId + " ");
                var List_LineMaster = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramLine).ToList();
                //ViewBag.LineName = GetList_LineMaster;

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(new { EmployeeName = EmployeeName, List_LineMaster = List_LineMaster, Branch = Branch }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetUnit(int? CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                DynamicParameters paramShift = new DynamicParameters();
                paramShift.Add("@query", "select Atten_Shifts.ShiftId As Id,Atten_Shifts.ShiftName+' ( '+RIGHT(CONVERT(VARCHAR, Atten_Shifts.BeginTime, 100), 7)+'-'+  RIGHT(CONVERT(VARCHAR, Atten_Shifts.EndTime, 100), 7) +' )'	as Name from Atten_Shifts where Atten_Shifts.Deactivate=0 and CmpId="+ CmpId + " and Atten_Shifts.ShiftBranchId="+ BranchId + " order by Name");
                var ShiftNameGet = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramShift).ToList();

                DynamicParameters paramUnit = new DynamicParameters();
                paramUnit.Add("@query", "select UnitId as Id,UnitName as Name from Mas_Unit where Deactivate=0 and CmpId=" + CmpId + " and UnitBranchId=" + BranchId + " ");
                var List_SubUnit = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramUnit).ToList();
                return Json(new { ShiftNameGet = ShiftNameGet, List_SubUnit = List_SubUnit }, JsonRequestBehavior.AllowGet);
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