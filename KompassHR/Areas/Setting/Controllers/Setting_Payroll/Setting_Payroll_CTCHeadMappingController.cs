using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_CTCHeadMappingController : Controller
    {
        // GET: Setting/Setting_Payroll_CTCHeadMapping
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region Main View
        public ActionResult Setting_Payroll_CTCHeadMapping(string PayrollHeadId_Encrypted, string CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 427;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                BulkAccessClass Obj = new BulkAccessClass();
                ViewBag.CompanyName = Obj.GetCompanyName();
                CTCHeadMapping ObjMapHead = new CTCHeadMapping();

                ViewBag.AddUpdateTitle = "Add";
                
                if (PayrollHeadId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_EncryptedId", PayrollHeadId_Encrypted);
                    param.Add("@p_CompanyId", CompanyId);
                    ObjMapHead = DapperORM.ReturnList<CTCHeadMapping>("sp_List_Payroll_HeadMappings", param).FirstOrDefault();
                    return View(ObjMapHead);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<CTCHeadMapping> tbldata, TypeCTCHeadMapping Obj) // Replace with your actual ViewModel
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                List<TypeCTCHeadMapping> updatedList = new List<TypeCTCHeadMapping>();
                updatedList.Add(Obj);
                DataTable dt = ConvertToDataTable(updatedList);

                
                if (Obj.PayrollHeadId_Encrypted != null)
                {
                    param.Add("@p_process", "Update");
                }
                else
                {
                    var GetCount = DapperORM.DynamicQueryList($@"SELECT COUNT(1) as GetCount FROM Payroll_Head_CTC WHERE Deactivate=0 AND PayrollMap_CompanyId={Obj.PayrollMap_CompanyId}").FirstOrDefault();
                    var count = GetCount.GetCount;
                    if (count > 0)
                    {
                        TempData["Message"] = "This company already created CTC";
                        TempData["Icon"] = "error";
                        return RedirectToAction("Setting_Payroll_CTCHeadMapping", "Setting_Payroll_CTCHeadMapping");
                    }
                    param.Add("@p_process", "Save");
                   
                }
                //param.Add("@p_process", string.IsNullOrEmpty(Obj.EncryptedId) ? "Save" : "Update");
                param.Add("@tbl_dt", dt.AsTableValuedParameter("tbl_TypePayrollHead"));
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadMappings", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("Setting_Payroll_CTCHeadMapping", "Setting_Payroll_CTCHeadMapping");
                //return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_HeadMappings", param).ToList();
                ViewBag.HeadMapping = data;
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
        public ActionResult Delete(string PayrollHeadId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EncryptedId", PayrollHeadId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadMappings", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_CTCHeadMapping");
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