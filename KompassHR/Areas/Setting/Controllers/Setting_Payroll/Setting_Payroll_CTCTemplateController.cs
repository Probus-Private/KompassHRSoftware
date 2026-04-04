using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using System.Data;
using System.Net;
using System.Reflection;

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_CTCTemplateController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Payroll_CTCTemplate
        #region Main View
        public ActionResult Setting_Payroll_CTCTemplate(int? CompanyId , string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 687;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_Employee_CTC_Template Obj = new Mas_Employee_CTC_Template();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.ComapnyName = GetComapnyName;

                int selectedCompanyId = CompanyId ?? 0;
                Obj.TemplateCompanyId = selectedCompanyId;
               
                //if (CompanyId != null)
                //{
                //    DynamicParameters Head = new DynamicParameters();
                //    Head.Add("@query", $"SELECT * FROM Mas_Employee_CTC_Template WHERE Deactivate = 0 AND TemplateCompanyId = {CompanyId}");
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head);
                //    TempData["GetAllDetail"] = data?.FirstOrDefault();
                //}
                //else
                //{
                //    TempData["GetAllDetail"] = null;
                //}
                // Get selected company ID
                //int selectedCompanyId = CompanyId ?? Convert.ToInt32(GetComapnyName.FirstOrDefault()?.Id ?? 0);
                // Get data for selected company
                //if (CompanyId != null)
                //{
                //    selectedCompanyId = CompanyId ?? 0;
                //    DynamicParameters Head = new DynamicParameters();
                //    Head.Add("@query", $"SELECT * FROM Mas_Employee_CTC_Template WHERE Deactivate = 0 AND TemplateCompanyId = {selectedCompanyId}");
                //    var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head);
                //    TempData["GetAllDetail"] = data?.FirstOrDefault();
                //}
                //else
                //{
                //    if (selectedCompanyId > 0)
                //    {
                //        DynamicParameters Head = new DynamicParameters();
                //        Head.Add("@query", $"SELECT * FROM Mas_Employee_CTC_Template WHERE Deactivate = 0 AND TemplateCompanyId = {selectedCompanyId}");
                //        var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head);
                //        TempData["GetAllDetail"] = data?.FirstOrDefault();
                //    }
                //    else
                //    {
                //        TempData["GetAllDetail"] = null;
                //    }
                //}
                //Obj.TemplateCompanyId = selectedCompanyId;

                if (EncryptedId != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_EncryptedId", EncryptedId);
                    var data1 =  DapperORM.ReturnList<dynamic>("sp_List_Payroll_Employee_CTC_Template", param);
                    var Getdata = data1?.FirstOrDefault();
                    TempData["GetAllDetail"] = Getdata;
                    Obj.TemplateCompanyId = Convert.ToDouble(Getdata.TemplateCompanyId);
                    Obj.TemplateName = Getdata.TemplateName;
                    selectedCompanyId = Convert.ToInt32(Getdata.TemplateCompanyId);
                }
                
                DynamicParameters HeadPartA = new DynamicParameters();
                HeadPartA.Add("@p_PayrollMap_CompanyId", selectedCompanyId);
                var GetHeadPartA = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartA", HeadPartA).ToList();
                ViewBag.MappedHeadPartA = GetHeadPartA;

                DynamicParameters HeadPartB = new DynamicParameters();
                HeadPartB.Add("@p_PayrollMap_CompanyId", selectedCompanyId);
                var GetHeadPartB = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartB", HeadPartB).ToList();
                ViewBag.MappedHeadPartB = GetHeadPartB;

                return View(Obj);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsUnitExists
        public ActionResult IsExists(int CompanyId, string TemplateName , string EncryptedId)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_EncryptedId", EncryptedId);
                param.Add("@p_TemplateCompanyId", CompanyId);
                param.Add("@p_TemplateName", TemplateName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Employee_CTC_Template", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Message != "")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 687;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_EncryptedId", "List");
                ViewBag.GetAllTemplate = DapperORM.DynamicList("sp_List_Payroll_Employee_CTC_Template", param);
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string EncryptedId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_EncryptedId", EncryptedId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Employee_CTC_Template", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_CTCTemplate");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region FINAL SUBMIT
        [HttpPost]
        public JsonResult SubmitData(Dictionary<string, Dictionary<string, string>> data, int CompanyId, string TemplateName, string EncriptedId, string partATotal, string partBTotal, string partABTotal, string Rate_VPF, double RatePartB_PF, double RatePartB_ESIC, double RatePartB_PT, double Rate_NetPay)
        {
            try
            {
                // Assuming TypeMas_Employee_CTC has properties that match the keys in the dictionaries
                var partAData = data.ContainsKey("partA") ? data["partA"] : new Dictionary<string, string>();
                var partBData = data.ContainsKey("partB") ? data["partB"] : new Dictionary<string, string>();

                //PART A 
                List<TypeMas_Employee_CTC_PartA> updatedList_PartA = new List<TypeMas_Employee_CTC_PartA>();
                TypeMas_Employee_CTC_PartA newItem_PartA = new TypeMas_Employee_CTC_PartA();
                foreach (var key in partAData.Keys)
                {
                    var property = typeof(TypeMas_Employee_CTC_PartA).GetProperty(key);
                    if (property != null && property.CanWrite)
                    {
                        var value = Convert.ChangeType(partAData[key], property.PropertyType);
                        property.SetValue(newItem_PartA, value);
                    }
                }
                //PART B 
                List<TypeMas_Employee_CTC_PartB> updatedList_PartB = new List<TypeMas_Employee_CTC_PartB>();
                TypeMas_Employee_CTC_PartB newItem_PartB = new TypeMas_Employee_CTC_PartB();
                foreach (var key in partBData.Keys)
                {
                    var property = typeof(TypeMas_Employee_CTC_PartB).GetProperty(key);
                    if (property != null && property.CanWrite)
                    {
                        var value = Convert.ChangeType(partBData[key], property.PropertyType);
                        property.SetValue(newItem_PartB, value);
                    }
                }
                // Add the object to the list PART A
                updatedList_PartA.Add(newItem_PartA);
                DataTable PartA_dt = new DataTable();
                PartA_dt = ConvertToDataTable(updatedList_PartA);

                // Add the object to the list PART B
                updatedList_PartB.Add(newItem_PartB);
                DataTable PartB_dt = new DataTable();
                PartB_dt = ConvertToDataTable(updatedList_PartB);
                if (EncriptedId == "")
                {
                    param.Add("@p_process", "Save");
                }
                else
                {
                    param.Add("@p_process", "Update");
                }
                //param.Add("@p_process", EncriptedId == null ? "Update" : "Save");
                param.Add("@tbl_dt_PartA", PartA_dt.AsTableValuedParameter("tbl_TypeParollCTC_PartA"));
                param.Add("@tbl_dt_PartB", PartB_dt.AsTableValuedParameter("tbl_TypeParollCTC_PartB"));
                param.Add("@p_RateTotalPartA", partATotal);
                param.Add("@p_RateTotalPartB", partBTotal);
                param.Add("@p_RateTotalPartAB", partABTotal);
                param.Add("@p_TemplateName", TemplateName);
                param.Add("@p_TemplateCompanyId", CompanyId);
                //param.Add("@p_RateMakerEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_Rate_VPF", Rate_VPF);
                param.Add("@p_RatePartB_PF", RatePartB_PF);
                param.Add("@p_RatePartB_ESIC", RatePartB_ESIC);
                param.Add("@p_RatePartB_PT", RatePartB_PT);
                param.Add("@p_Rate_NetPay", Rate_NetPay);
                //param.Add("@p_RatePartB_AttendanceBonus_IsApplicable", RatePartB_AttendanceBonus_IsApplicable);

                param.Add("@p_EncryptedId", EncriptedId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_ModifiedBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data1 = DapperORM.ExecuteReturn("sp_SUD_Payroll_Employee_CTC_Template", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                Session["p_process"] = null;
                return Json(new { success = true, Message = TempData["Message"], Icon = TempData["Icon"] });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region Calculate Data This is Pending SP not generated
        public JsonResult CalculateDatas(Dictionary<string, Dictionary<string, string>> data1, string FromDate, string DailyMonthly, string partATotal)
        {
            try
            {
                // Extract partAData from data
                var partAData = data1.ContainsKey("partA") ? data1["partA"] : new Dictionary<string, string>();

                // Prepare PART A data
                List<TypeMas_Employee_CTC_PartA> updatedList_PartA = new List<TypeMas_Employee_CTC_PartA>();
                TypeMas_Employee_CTC_PartA newItem_PartA = new TypeMas_Employee_CTC_PartA();

                foreach (var key in partAData.Keys)
                {
                    var property = typeof(TypeMas_Employee_CTC_PartA).GetProperty(key);
                    if (property != null && property.CanWrite)
                    {
                        var value = Convert.ChangeType(partAData[key], property.PropertyType);
                        property.SetValue(newItem_PartA, value);
                    }
                }

                // Add the object to the list
                updatedList_PartA.Add(newItem_PartA);

                // Convert to DataTable
                DataTable PartA_dt = ConvertToDataTable(updatedList_PartA);

                DynamicParameters param = new DynamicParameters();
                param.Add("@tbl_dt_PartA", PartA_dt.AsTableValuedParameter("tbl_TypeParollCTC_PartA"));
                param.Add("@p_DailyMonthly", DailyMonthly);
                param.Add("@p_CTCEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_RateTotalPartA", partATotal);
                param.Add("@p_FromDate", Convert.ToDateTime(FromDate).Date);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500); // Output parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500); // Output parameter

                var GetCalculte = DapperORM.ExecuteSP<dynamic>("sp_CTC_Employee_PF_ESIC", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
                var SendCalculate = Newtonsoft.Json.JsonConvert.SerializeObject(GetCalculte);
                var deserializedDynamic = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(SendCalculate);
                return Json(new { GetCalculte }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
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