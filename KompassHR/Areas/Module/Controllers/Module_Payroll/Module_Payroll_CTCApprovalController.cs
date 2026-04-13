using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Module.Models.Module_Payroll;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
namespace KompassHR.Areas.Module.Controllers.Module_Payroll
{
    public class Module_Payroll_CTCApprovalController : Controller
    {
        // GET: Module/Module_Payroll_CTCApproval
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        public ActionResult Module_Payroll_CTCApproval(Payroll_CTCApproval OBjCTC)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 493;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var results = DapperORM.DynamicQueryMultiple(
               "SELECT DISTINCT CompanyId AS Id, CompanyName AS Name FROM Mas_CompanyProfile INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerCmpId = Mas_CompanyProfile.CompanyId WHERE Payroll_MakerChecker.Deactivate = 0 AND Mas_CompanyProfile.Deactivate = 0 " +
               "AND Payroll_MakerChecker.IncrementCheckerEmpId = '" + Session["EmployeeId"] + "'");

                var Companylist = results[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();
                ViewBag.GetCompanyName = Companylist;
                var CompanyId = Companylist.FirstOrDefault()?.Id;
                if (CompanyId == null)
                {
                    CompanyId = 0;
                }

                var branch = DapperORM.DynamicQueryMultiple("SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                 "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.IncrementCheckerEmpId = '" + Session["EmployeeId"] + "' " +
                 "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + CompanyId + "'");
                ViewBag.GetBranchName = branch[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();


                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                //var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                //ViewBag.GetCompanyName = GetComapnyName;



                //// START THIS BELOW ARE USE FOR IN MODAL DROPDOWN DIND
                //DynamicParameters Category = new DynamicParameters();
                //Category.Add("@query", "Select CategoryId Id , CategoryName Name from Payroll_CTC_Category Where Deactivate=0");
                //var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Category).ToList();
                //ViewBag.CategoryName = GetCategoryName;

                //DynamicParameters Buget = new DynamicParameters();
                //Buget.Add("@query", $@"Select  BudgetId as Id,BudgetName as Name from Payroll_Budget,Payroll_BudgetMapping
                //where Payroll_Budget.BudgetId = Payroll_BudgetMapping.BudgetMappingBugetId
                //and Payroll_Budget.Deactivate = 0 and Payroll_BudgetMapping.Deactivate = 0
                //and  BudgetMappingCmpId = (Select CmpID from Mas_Employee where EmployeeId={Session["OnboardEmployeeId"]}) and BudgetMappingBuId = (Select EmployeeBranchId from Mas_Employee where EmployeeId={Session["OnboardEmployeeId"]})
                //order by BudgetName");
                //var GetBudgetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Buget).ToList();
                //ViewBag.BudgetName = GetBudgetName;

                //var results = DapperORM.DynamicQueryMultiple($@"Select CompanyBankId As Id , BankName as Name from Payroll_CompanyBank where Deactivate=0
                //                 AND CompanyBankCmpID = (SELECT CmpId FROM Mas_Employee WHERE EmployeeId = {Session["OnboardEmployeeId"]} AND Deactivate = 0)
                //                 AND CompanyBankBUId = (SELECT EmployeeBranchId FROM Mas_Employee WHERE EmployeeId = {Session["OnboardEmployeeId"]} AND Deactivate = 0)                                                    
                //                 Order By IsDefault Desc");
                //ViewBag.CompanyBankName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                //// END THIS BELOW ARE USE FOR IN MODAL DROPDOWN DIND


                var CmpId = Session["CompanyId"];
                DynamicParameters HeadPartA = new DynamicParameters();
                HeadPartA.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartA = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartA", HeadPartA).ToList();
                ViewBag.MappedHeadPartA = GetHeadPartA;
                DynamicParameters HeadPartB = new DynamicParameters();
                HeadPartB.Add("@p_PayrollMap_CompanyId", CmpId);
                var GetHeadPartB = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_EmployeeCTC_PartB", HeadPartB).ToList();
                ViewBag.MappedHeadPartB = GetHeadPartB;

                ViewBag.GetBranchName = "";
                ViewBag.CTCApproval = null;
                if (OBjCTC.CTCCmpId != 0)
                {
                    var branch1 = DapperORM.DynamicQueryMultiple("SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                   "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                   "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                   "AND Mas_Branch.Deactivate = 0 " +
                   "AND Payroll_MakerChecker.IncrementCheckerEmpId = '" + Session["EmployeeId"] + "' " +
                   "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + OBjCTC.CTCCmpId + "'");
                    ViewBag.GetBranchName = branch1[0].Select(x => new AllDropDownBind { Id = Convert.ToDouble(x.Id), Name = (string)x.Name }).ToList();

                    //DynamicParameters param = new DynamicParameters();
                    //param.Add("@p_employeeid", Session["EmployeeId"]);
                    //param.Add("@p_CmpId", OBjCTC.CTCCmpId);
                    //var data1 = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    //ViewBag.GetBranchName = data1;


                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_Process", "GetCTC");
                    paramList.Add("@p_BranchId", OBjCTC.CTCBranchId);
                    paramList.Add("@p_CmpId", OBjCTC.CTCCmpId);
                    paramList.Add("@p_CTCEmployeeId", Session["EmployeeId"]);
                    var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_CTCApproval", paramList);
                    ViewBag.CTCApproval = data;

                }
                return View(OBjCTC);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetList

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 493;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_Process", "GetList");
                param.Add("@p_CTCEmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_CTCApproval", param).ToList();
                ViewBag.GetCTCApprovalList = data;
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

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "SELECT DISTINCT BranchId AS Id, BranchName AS Name FROM Mas_Branch " +
                 "INNER JOIN Payroll_MakerChecker ON Payroll_MakerChecker.MakerCheckerBranchId = Mas_Branch.BranchId " +
                 "WHERE Payroll_MakerChecker.Deactivate = 0 " +
                 "AND Mas_Branch.Deactivate = 0 " +
                 "AND Payroll_MakerChecker.IncrementCheckerEmpId = '" + Session["EmployeeId"] + "' " +
                 "AND Payroll_MakerChecker.MakerCheckerCmpId = '" + CmpId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetPopUpDETAILS
        public ActionResult GetEmployeeCTCDetails(string EmployeeCTCId_Encrypted, int? EmployeeId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return Json(new { IsSessionExpired = true }, JsonRequestBehavior.AllowGet);
                }

                var GetData = DapperORM.DynamicQueryList($@"Select * from Mas_Employee_CTC where Deactivate=0 and EmployeeCTCId_Encrypted='{EmployeeCTCId_Encrypted}'").FirstOrDefault();

                DynamicParameters Category = new DynamicParameters();
                Category.Add("@query", "Select CategoryId Id , CategoryName Name from Payroll_CTC_Category Where Deactivate=0");
                var GetCategoryName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Category).ToList();

                DynamicParameters Buget = new DynamicParameters();
                Buget.Add("@query", $@"Select  BudgetId as Id,BudgetName as Name from Payroll_Budget,Payroll_BudgetMapping
            where Payroll_Budget.BudgetId = Payroll_BudgetMapping.BudgetMappingBugetId
            and Payroll_Budget.Deactivate = 0 and Payroll_BudgetMapping.Deactivate = 0
            and  BudgetMappingCmpId = (Select CmpID from Mas_Employee where EmployeeId={EmployeeId}) 
            and BudgetMappingBuId = (Select EmployeeBranchId from Mas_Employee where EmployeeId={EmployeeId})
            order by BudgetName");
                var GetBudgetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Buget).ToList();

                var results = DapperORM.DynamicQueryMultiple($@"Select CompanyBankId As Id , BankName as Name from Payroll_CompanyBank where Deactivate=0
            AND CompanyBankCmpID = (SELECT CmpId FROM Mas_Employee WHERE EmployeeId = {EmployeeId} AND Deactivate = 0)
            AND CompanyBankBUId = (SELECT EmployeeBranchId FROM Mas_Employee WHERE EmployeeId = {EmployeeId} AND Deactivate = 0)                                                    
            Order By IsDefault Desc");
                var CompanyBankName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                return Json(new
                {
                    GetEmpCTC = GetData,
                    GetCategoryName = GetCategoryName,
                    GetBudgetName = GetBudgetName,
                    CompanyBankName = CompanyBankName
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { IsError = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(List<Payroll_CTCApproval> ObjCTCApproval)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                if (ObjCTCApproval != null)
                {
                    foreach (var Data in ObjCTCApproval)
                    {
                        //param.Add("@query", "update Mas_Employee_CTC set RateCheckerEmployeeId='"+Session["EmployeeId"] +"', RateCheckerDateAndTime='"+DateTime.Now+"' where EmployeeCTCId=1");
                        //var list_NoDuesName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                        string StringMonthlyAttendance = " update Mas_Employee_CTC set RateCheckerEmployeeId='" + Session["EmployeeId"] + "', RateCheckerDateAndTime='" + DateTime.Now + "' where EmployeeCTCId=" + Data.EmployeeCTCId + "";
                        strBuilder.Append(StringMonthlyAttendance);

                    }
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
                                                                            "'BuklInsert'," +
                                                                            "'" + Dns.GetHostName().ToString() + "'," +
                                                                            "GetDate()," +
                                                                            "'" + Session["EmployeeId"] + "'," +
                                                                            "'" + Session["EmployeeName"] + "'");
                        TempData["Message"] = abc;
                        TempData["Icon"] = "error";
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
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
    }
}
