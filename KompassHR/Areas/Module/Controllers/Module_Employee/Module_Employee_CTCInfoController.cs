using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
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

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_CTCInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_CTCInfo
        #region CTCInfo
        public ActionResult Module_Employee_CTCInfo(string EncryptedId, int? obEmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 419;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetHeadName = DapperORM.DynamicQuerySingle($@"Select CTC_RatePartB_Head_A,CTC_RatePartB_Head_B from Tool_CommonTable");
                TempData["CTC_RatePartB_Head_A"] = GetHeadName?.CTC_RatePartB_Head_A ?? "";
                TempData["CTC_RatePartB_Head_B"] = GetHeadName?.CTC_RatePartB_Head_B ?? "";

                var GetDailyOrMonthly = DapperORM.DynamicQuerySingle($@"Select EM_Atten_DailyMonthly From Mas_Employee where Deactivate=0 And EmployeeId={Session["OnboardEmployeeId"]}");
                TempData["GetDailyOrMonthly"] = GetDailyOrMonthly?.EM_Atten_DailyMonthly ?? "";

                var result = DapperORM.DynamicQuerySingle(@"SELECT RatePartB_AttendanceBonus_IsApplicable FROM Tool_CommonTable");
                // If result is null or the property is null, default to false
                TempData["GetAttenBonusIsApplicable"] = Convert.ToBoolean(result?.RatePartB_AttendanceBonus_IsApplicable ?? false);

                var GetStaDet = DapperORM.DynamicQueryList($@"Select ESIC_Applicable,PT_Applicable,LWF_Applicable,Gratuity_Applicable,PF_Applicable from Mas_Employee_Statutory
                                                where Deactivate=0 And StatutoryEmployeeId={Session["OnboardEmployeeId"]}").FirstOrDefault();
                TempData["GetStatutoryDetail"] = Newtonsoft.Json.JsonConvert.SerializeObject(GetStaDet);

                Session["p_process"] = null;
                if (EncryptedId != null)
                {
                    Session["p_process"] = "Update";
                    DynamicParameters Head = new DynamicParameters();
                    Head.Add("@query", $@" Select * from Mas_Employee_CTC where Deactivate=0 and EmployeeCTCId_Encrypted='{EncryptedId}'");
                    TempData["GetIncrementDetail"] = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head).FirstOrDefault();
                }
                if (obEmployeeId != null)
                {
                    Session["p_process"] = "Increment";
                    DynamicParameters Head = new DynamicParameters();
                    Head.Add("@query", $@"Select Top 1 * from Mas_Employee_CTC where Deactivate=0 and CTCEmployeeId={Session["OnboardEmployeeId"]} And Year(ToDate)=2999");
                    var GetHead = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head).FirstOrDefault();
                    if (GetHead!=null)
                    {
                        GetHead.NextIncrementDate = "";
                        TempData["GetIncrementDetail"] = GetHead;
                    }
                    else
                    {
                        TempData["GetIncrementDetail"] = null;
                    }
                    
                }
                ViewBag.AddUpdateTitle = Session["p_process"] == null ? "Add" : Session["p_process"];

                DynamicParameters Category = new DynamicParameters();
                Category.Add("@query", "Select CategoryId Id , CategoryName Name from Payroll_CTC_Category Where Deactivate=0");
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Category).ToList();
                ViewBag.CategoryName = GetCategoryName;

                DynamicParameters Buget = new DynamicParameters();
                Buget.Add("@query", $@"Select  BudgetId as Id,BudgetName as Name from Payroll_Budget,Payroll_BudgetMapping
                where Payroll_Budget.BudgetId = Payroll_BudgetMapping.BudgetMappingBugetId
                and Payroll_Budget.Deactivate = 0 and Payroll_BudgetMapping.Deactivate = 0
                and  BudgetMappingCmpId = (Select CmpID from Mas_Employee where EmployeeId={Session["OnboardEmployeeId"]}) and BudgetMappingBuId = (Select EmployeeBranchId from Mas_Employee where EmployeeId={Session["OnboardEmployeeId"]})
                order by BudgetName");
                var GetBudgetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Buget).ToList();
                ViewBag.BudgetName = GetBudgetName;

                var results = DapperORM.DynamicQueryMultiple($@"Select CompanyBankId As Id , BankName as Name from Payroll_CompanyBank where Deactivate=0
                                 AND CompanyBankCmpID = (SELECT CmpId FROM Mas_Employee WHERE EmployeeId = {Session["OnboardEmployeeId"]} AND Deactivate = 0)
                                 AND CompanyBankBUId = (SELECT EmployeeBranchId FROM Mas_Employee WHERE EmployeeId = {Session["OnboardEmployeeId"]} AND Deactivate = 0)                                                    
                                 Order By IsDefault Desc");
                ViewBag.CompanyBankName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();


                //var CmpId = Session["CompanyId"];
                var GetCmpId = DapperORM.DynamicQuerySingle($@"Select CmpID from Mas_Employee where Deactivate=0 and EmployeeId={Session["OnboardEmployeeId"]}");
                var CmpId = GetCmpId?.CmpID;
                DynamicParameters HeadPartA = new DynamicParameters();
                HeadPartA.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartA = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartA", HeadPartA).ToList();
                ViewBag.MappedHeadPartA = GetHeadPartA;

                DynamicParameters HeadPartB = new DynamicParameters();
                HeadPartB.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartB = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartB", HeadPartB).ToList();
                ViewBag.MappedHeadPartB = GetHeadPartB;

                var GetCommonColumn = DapperORM.ExecuteSP<dynamic>("sp_GetTool_CommonColumn").ToList();
                ViewBag.GetCommonColumn = GetCommonColumn;

                DynamicParameters Template = new DynamicParameters();
                Template.Add("@query", $@"Select TemplateCTCId as Id,TemplateName as Name from Mas_Employee_CTC_Template where Deactivate=0");
                var GetTemplateName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Template).ToList();
                ViewBag.TemplateName = GetTemplateName;

                return View();
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
        public JsonResult SubmitData(Dictionary<string, Dictionary<string, string>> data, string Status, int OBEmployeeId, string RateOTRate, int CategoryId, string DailyMonthly, string EncriptedId, string fromDate, string toDate, string NextIncrementDate, string Remark, string partATotal, string partBTotal, string partABTotal , string Rate_VPF, string RateSalaryMode, double CompanyBankId, string BudgetId, double RatePartB_PF, double RatePartB_ESIC, double RatePartB_PT, double RatePartB_MLWF, double RatePartB_Head_A, double RatePartB_Head_B, double Rate_NetPay, bool RatePartB_AttendanceBonus_IsApplicable,double RateA, double RateB, double RateC)
        {
            try
            {
                // Assuming TypeMas_Employee_CTC has properties that match the keys in the dictionaries
                var partAData = data.ContainsKey("partA") ? data["partA"] : new Dictionary<string, string>();
                var partBData = data.ContainsKey("partB") ? data["partB"] : new Dictionary<string, string>();
                // Combine partAData and partBData
                //var combinedData = partAData.Concat(partBData).ToDictionary(x => x.Key, x => x.Value);
                //combinedData["RateTotalPartA"] = partATotal;
                //combinedData["RateTotalPartB"] = partBTotal;
                //combinedData["RateTotalPartAB"] = partABTotal;

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
                //PartA_dt = ConvertToDataTable(updatedList_PartA);
                PartA_dt = ConvertToDataTable_PartA(updatedList_PartA);

                // Add the object to the list PART B
                updatedList_PartB.Add(newItem_PartB);
                DataTable PartB_dt = new DataTable();
                //PartB_dt = ConvertToDataTable(updatedList_PartB);
                PartB_dt = ConvertToDataTable_PartB(updatedList_PartB);
                if (EncriptedId == null)
                {
                    param.Add("@p_process", "Save");
                }
                else
                {
                    param.Add("@p_process", Session["p_process"]);
                }
                //param.Add("@p_process", OBEmployeeId == null ? "Update" : "Save");
                param.Add("@tbl_dt_PartA", PartA_dt.AsTableValuedParameter("tbl_TypeParollCTC_PartA"));
                param.Add("@tbl_dt_PartB", PartB_dt.AsTableValuedParameter("tbl_TypeParollCTC_PartB"));
                param.Add("@p_RateTotalPartA", partATotal);
                param.Add("@p_RateTotalPartB", partBTotal);
                param.Add("@p_RateTotalPartAB", partABTotal);
                param.Add("@p_DailyMonthly", DailyMonthly);
                param.Add("@p_FromDate", fromDate);
                param.Add("@p_ToDate", toDate);
                param.Add("@p_NextIncrementDate", NextIncrementDate);
                param.Add("@p_Remark", Remark);
                param.Add("@p_Rate_VPF", Rate_VPF);
                param.Add("@p_RateOTRate", RateOTRate);
                param.Add("@p_CategoryId", CategoryId);
                param.Add("@p_RateBudgetId", BudgetId);
                param.Add("@p_RateSalaryMode", RateSalaryMode);
                param.Add("@p_RateCmpBankId", CompanyBankId);
                param.Add("@p_RateMakerEmployeeId", Session["EmployeeId"]);

                param.Add("@p_RatePartB_PF", RatePartB_PF);
                param.Add("@p_RatePartB_ESIC", RatePartB_ESIC);
                param.Add("@p_RatePartB_PT", RatePartB_PT);
                param.Add("@p_RatePartB_MLWF", RatePartB_MLWF);
                param.Add("@p_RatePartB_Head_A", RatePartB_Head_A);
                param.Add("@p_RatePartB_Head_B", RatePartB_Head_B);
                param.Add("@p_Rate_NetPay", Rate_NetPay);
                param.Add("@p_RatePartB_AttendanceBonus_IsApplicable", RatePartB_AttendanceBonus_IsApplicable);

                param.Add("@p_RateA", RateA);
                param.Add("@p_RateB", RateB);
                param.Add("@p_RateC", RateC);

                param.Add("@p_CTCEmployeeId", OBEmployeeId);
                param.Add("@p_EncryptedId", EncriptedId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_ModifiedBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data1 = DapperORM.ExecuteReturn("sp_SUD_Payroll_Employee_CTC", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                Session["p_process"] = null;
                TempData["GetIncrementDetail"] = null;
                //return RedirectToAction("Setting_Payroll_CTCHeadMapping", "Setting_Payroll_CTCHeadMapping");
                return Json(new { success = true, message = TempData["Message"], Icon = TempData["Icon"] });
            }
            catch (Exception ex)
            {
                // Handle any errors
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region GetEmployeeName
        public ActionResult GetIncrementDetails(int EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters Head = new DynamicParameters();
                Head.Add("@query", $@"Select EmployeeName,FromDate,ToDate,IsIncrement,DailyMonthly,RateOTRate,CategoryName from Mas_Employee_CTC 
                                    inner join Mas_Employee on Mas_Employee.EmployeeId=Mas_Employee_CTC.CTCEmployeeId
                                    inner join Payroll_CTC_Category On Payroll_CTC_Category.CategoryId=Mas_Employee_CTC.CategoryId
                                    where Mas_Employee_CTC.Deactivate=0 And CTCEmployeeId={EmployeeId}");
                var GetData = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Head).FirstOrDefault();

                return Json(new { GetIncrementDetail = GetData });
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetPopUpDETAILS
        public ActionResult GetEmployeeCTCDetails(string EmployeeCTCId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetData = DapperORM.DynamicQueryList($@"Select * from Mas_Employee_CTC where Deactivate=0 and EmployeeCTCId_Encrypted='{EmployeeCTCId_Encrypted}'").FirstOrDefault();
                return Json(new { GetEmpCTC = GetData }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetList(string EmployeeCTCId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 419;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetCount = DapperORM.DynamicQuerySingle($@"Select COUNT(*) As Counts from Mas_Employee_Statutory
                                                Where Deactivate=0 And StatutoryEmployeeId={Session["OnboardEmployeeId"]}");
                var GetCounts = GetCount != null ? GetCount.Counts : null;
                TempData["CheckEmpInStatutory"] = GetCounts;
                DynamicParameters queryEx = new DynamicParameters();
                queryEx.Add("@query", $@"Select EmployeeCTCId,EmployeeCTCId_Encrypted,FromDate,ToDate,IsIncrement,DailyMonthly,CategoryName,RateTotalPartAB from Mas_Employee_CTC 
                                        inner join Payroll_CTC_Category On Payroll_CTC_Category.CategoryId=Mas_Employee_CTC.CategoryId
                                        where Mas_Employee_CTC.Deactivate=0  And CTCEmployeeId={Session["OnboardEmployeeId"]} order by todate desc");
                ViewBag.GetEmployeeCTCList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", queryEx).ToList();



                ViewBag.AddUpdateTitle = Session["p_process"] == null ? "Add" : Session["p_process"];
                DynamicParameters Category = new DynamicParameters();
                Category.Add("@query", "Select CategoryId Id , CategoryName Name from Payroll_CTC_Category Where Deactivate=0");
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Category).ToList();
                ViewBag.CategoryName = GetCategoryName;

                var CmpId = Session["CompanyId"];
                DynamicParameters HeadPartA = new DynamicParameters();
                HeadPartA.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartA = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartA", HeadPartA).ToList();
                ViewBag.MappedHeadPartA = GetHeadPartA;

                DynamicParameters HeadPartB = new DynamicParameters();
                HeadPartB.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartB = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartB", HeadPartB).ToList();
                ViewBag.MappedHeadPartB = GetHeadPartB;


                var GetHeadName = DapperORM.DynamicQuerySingle($@"Select CTC_RatePartB_Head_A,CTC_RatePartB_Head_B from Tool_CommonTable");
                TempData["CTC_RatePartB_Head_A"] = GetHeadName?.CTC_RatePartB_Head_A ?? "";
                TempData["CTC_RatePartB_Head_B"] = GetHeadName?.CTC_RatePartB_Head_B ?? "";

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region New Code Convert ToTatable For RatePart_A
        public static DataTable ConvertToDataTable_PartA(List<TypeMas_Employee_CTC_PartA> list)
        {
            DataTable dt = new DataTable();
            // Define columns in EXACT order as dbo.tbl_TypeParollCTC_PartB your sql sever Create this Type
            dt.Columns.Add("RatePartA_Basic", typeof(decimal));
            dt.Columns.Add("RatePartA_DA", typeof(decimal));
            dt.Columns.Add("RatePartA_HRA", typeof(decimal));
            dt.Columns.Add("RatePartA_A", typeof(decimal));
            dt.Columns.Add("RatePartA_B", typeof(decimal));
            dt.Columns.Add("RatePartA_C", typeof(decimal));
            dt.Columns.Add("RatePartA_D", typeof(decimal));
            dt.Columns.Add("RatePartA_E", typeof(decimal));
            dt.Columns.Add("RatePartA_F", typeof(decimal));
            dt.Columns.Add("RatePartA_G", typeof(decimal));
            dt.Columns.Add("RatePartA_H", typeof(decimal));
            dt.Columns.Add("RatePartA_I", typeof(decimal));
            dt.Columns.Add("RatePartA_J", typeof(decimal));
            dt.Columns.Add("RatePartA_K", typeof(decimal));
            dt.Columns.Add("RatePartA_L", typeof(decimal));
            dt.Columns.Add("RatePartA_M", typeof(decimal));
            dt.Columns.Add("RatePartA_N", typeof(decimal));
            dt.Columns.Add("RatePartA_O", typeof(decimal));
            dt.Columns.Add("RatePartA_P", typeof(decimal));
            dt.Columns.Add("RatePartA_Q", typeof(decimal));
            dt.Columns.Add("RatePartA_R", typeof(decimal));
            dt.Columns.Add("RatePartA_S", typeof(decimal));
            dt.Columns.Add("RatePartA_T", typeof(decimal));
            dt.Columns.Add("RateTotalPartA", typeof(decimal));

            foreach (var item in list)
            {
                dt.Rows.Add(
                    item.RatePartA_Basic,
                    item.RatePartA_DA,
                    item.RatePartA_HRA,
                    item.RatePartA_A,
                    item.RatePartA_B,
                    item.RatePartA_C,
                    item.RatePartA_D,
                    item.RatePartA_E,
                    item.RatePartA_F,
                    item.RatePartA_G,
                    item.RatePartA_H,
                    item.RatePartA_I,
                    item.RatePartA_J,
                    item.RatePartA_K,
                    item.RatePartA_L,
                    item.RatePartA_M,
                    item.RatePartA_N,
                    item.RatePartA_O,
                    item.RatePartA_P,
                    item.RatePartA_Q,
                    item.RatePartA_R,
                    item.RatePartA_S,
                    item.RatePartA_T,
                    item.RateTotalPartA
                );
            }
            return dt;
        }
        #endregion

        #region New Code Convert ToTatable For RatePart_B
        public static DataTable ConvertToDataTable_PartB(List<TypeMas_Employee_CTC_PartB> list)
        {
            DataTable dt = new DataTable();
            // Define columns in EXACT order as dbo.tbl_TypeParollCTC_PartB your sql sever Create this Type
            dt.Columns.Add("RatePartB_PFEmployer", typeof(double));
            dt.Columns.Add("RatePartB_ESICEmployer", typeof(double));
            dt.Columns.Add("RatePartB_LWFEmployer", typeof(double));
            dt.Columns.Add("RatePartB_Bonus", typeof(double));
            dt.Columns.Add("RatePartB_Gratuity", typeof(double));
            dt.Columns.Add("RatePartB_A", typeof(double));
            dt.Columns.Add("RatePartB_B", typeof(double));
            dt.Columns.Add("RatePartB_C", typeof(double));
            dt.Columns.Add("RatePartB_D", typeof(double));
            dt.Columns.Add("RatePartB_E", typeof(double));
            dt.Columns.Add("RateTotalPartB", typeof(double));
            dt.Columns.Add("RateTotalPartAB", typeof(double));

            foreach (var item in list)
            {
                dt.Rows.Add(
                    item.RatePartB_PFEmployer,
                    item.RatePartB_ESICEmployer,
                    item.RatePartB_LWFEmployer,
                    item.RatePartB_Bonus,
                    item.RatePartB_Gratuity,
                    item.RatePartB_A,
                    item.RatePartB_B,
                    item.RatePartB_C,
                    item.RatePartB_D,
                    item.RatePartB_E,
                    item.RateTotalPartB,
                    item.RateTotalPartAB
                );
            }
            return dt;
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

        #region Calculate Data
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

        #region GetTemplate Details
        [HttpGet]
        public JsonResult GetTemplateDetails(int TemplateId)
        {
            try
            {
                DynamicParameters Template = new DynamicParameters();
                Template.Add("@query", $@"Select * from Mas_Employee_CTC_Template where Deactivate=0 and TemplateCTCId={TemplateId}");
                var data = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", Template).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}