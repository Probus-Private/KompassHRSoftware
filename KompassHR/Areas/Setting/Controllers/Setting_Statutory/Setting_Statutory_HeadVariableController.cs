using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Areas.Setting.Models.Setting_Statutory;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_HeadVariableController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Statutory_HeadVariable
        public ActionResult Setting_Statutory_HeadVariable(string PayrollHeadVariableId_Encrypted,  int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 453;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_Head_Variable Head_Variable = new Payroll_Head_Variable();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyname = GetComapnyName;

                if (PayrollHeadVariableId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_EncryptedId", PayrollHeadVariableId_Encrypted);
                    param.Add("@p_CompanyId", CmpId);
                    Head_Variable = DapperORM.ReturnList<Payroll_Head_Variable>("sp_List_Payroll_HeadVariable", param).FirstOrDefault();
                    return View(Head_Variable);
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(TypePayroll_Head_Variable Obj) // Replace with your actual ViewModel
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                List<TypePayroll_Head_Variable> updatedList = new List<TypePayroll_Head_Variable>();
                updatedList.Add(Obj);
                DataTable dt = ConvertToDataTable(updatedList);
                if (Obj.PayrollHeadVariableId_Encrypted != null)
                {
                    param.Add("@p_process", "Update");
                }
                else
                {
                    var result = DapperORM.DynamicQuerySingle(@"SELECT COUNT(1) as Counts FROM Payroll_Head_Variable WHERE Deactivate=0 AND CmpID="+ Obj.CmpID + "");
                    int count = Convert.ToInt32(result?.Counts);
                    if (count > 0)
                    {
                        TempData["Message"] = "This company already created Head";
                        TempData["Icon"] = "error";
                        return RedirectToAction("Setting_Statutory_HeadVariable", "Setting_Statutory_HeadVariable");
                    }
                    param.Add("@p_process", "Save");

                }
                //param.Add("@p_process", string.IsNullOrEmpty(Obj.EncryptedId) ? "Save" : "Update");
                param.Add("@tbl_dt", dt.AsTableValuedParameter("tbl_TypePayrollHeadVariable"));
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadVariable", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Statutory_HeadVariable", "Setting_Statutory_HeadVariable");
              
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
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_EncryptedId", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_HeadVariable", param).ToList();
                ViewBag.HeadVariable = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete Record
        public ActionResult Delete(string PayrollHeadVariableId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EncryptedId", PayrollHeadVariableId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadVariable", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_HeadVariable");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

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