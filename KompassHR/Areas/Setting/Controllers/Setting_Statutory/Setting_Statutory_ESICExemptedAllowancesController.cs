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
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_ESICExemptedAllowancesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Statutory_ESICExemptedAllowances
        #region ESICExemptedAllowances
        public ActionResult Setting_Statutory_ESICExemptedAllowances(int? CmpId, string ESICExemptedAllowancesMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 451;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_ESICExemptedAllowances_Master ESICExempted = new Payroll_ESICExemptedAllowances_Master();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyname = GetComapnyName;

                if (CmpId != null)
                {
                    DynamicParameters paramESIC = new DynamicParameters();
                    paramESIC.Add("@p_PayrollMap_CompanyId", CmpId);
                    ViewBag.GetCTCHead = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_ESICExempted", paramESIC).ToList();
                }
                else
                {
                    ViewBag.GetCTCHead = "";
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 451;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ESICExemptedAllowancesMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_ESICExemptedAllowances_Master", param).ToList();
                ViewBag.ESICExemptedGetList = data;
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
        public ActionResult SaveUpdate(List<Payroll_ESICExemptedAllowances_Detail> Task, int? CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                var GetESICCunt = DapperORM.DynamicQuerySingle("Select count(ESICExemptedAllowancesMasterId) as ESICExemptedcount from Payroll_ESICExemptedAllowances_Master where  deactivate=0 and  CmpID=" + CmpID + "");
                if (GetESICCunt.ESICExemptedcount != 0)
                {

                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "Select ESICExemptedAllowancesMasterId as Id,'' as Name from Payroll_ESICExemptedAllowances_Master where  deactivate=0 and  CmpID=" + CmpID + "");
                    var GetMasterId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();

                    var GetId = DapperORM.DynamicQuerySingle("Select  ESICExemptedAllowancesMasterId from Payroll_ESICExemptedAllowances_Master where  deactivate=0 and  CmpID=" + CmpID + "");
                    DapperORM.Execute("update Payroll_ESICExemptedAllowances_Master set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where ESICExemptedAllowancesMasterId='" + GetMasterId[0].Id + "'");
                    DapperORM.Execute("Delete from Payroll_ESICExemptedAllowances_Detail where ESICExemptedAllowancesDetail_MasterId=" + GetMasterId[0].Id + "");

                    foreach (var Data in Task)
                    {
                        string ESICExemptedallowances = "Insert Into Payroll_ESICExemptedAllowances_Detail(" +
                                              "   ESICExemptedAllowancesDetail_MasterId " +
                                              " , ESICExemptedallowancesName " +
                                              " , ESICExemptedallowancesName_TempTable " +
                                              " , IsActive " +
                                              ") values (" +
                                              "'" + GetMasterId[0].Id + "'," + "'" + Data.ESICExemptedallowancesName + "'," +
                                              "'" + Data.TempTableESICExemptedallowancesName + "'," +
                                              "'1')" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(ESICExemptedallowances);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
                    }
                    //if (abc != "")
                    //{
                    //    DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                    //                                                        "   Error_Desc " +
                    //                                                        " , Error_FormName " +
                    //                                                        " , Error_MachinceName " +
                    //                                                        " , Error_Date " +
                    //                                                        " , Error_UserID " +
                    //                                                        " , Error_UserName " + ") values (" +
                    //                                                        "'" + strBuilder + "'," +
                    //                                                        "'BuklInsert'," +
                    //                                                        "'" + Dns.GetHostName().ToString() + "'," +
                    //                                                        "GetDate()," +
                    //                                                        "'" + Session["EmployeeId"] + "'," +
                    //                                                        "'" + Session["EmployeeName"] + "'");
                    //    TempData["Message"] = abc;
                    //    TempData["Icon"] = "error";
                    //}
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_CmpID", CmpID);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICExemptedAllowances_Master", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");


                    foreach (var Data in Task)
                    {
                        string ESICExemptedallowances = "Insert Into Payroll_ESICExemptedAllowances_Detail(" +
                                              "   ESICExemptedAllowancesDetail_MasterId " +
                                              " , ESICExemptedallowancesName " +
                                              " , ESICExemptedallowancesName_TempTable " +
                                              ") values (" +
                                              "'" + PID + "'," + "'" + Data.ESICExemptedallowancesName + "'," +
                                              "'" + Data.TempTableESICExemptedallowancesName + "')" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(ESICExemptedallowances);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }
                    //if (abc != "")
                    //{
                    //    DapperORM.DynamicQuerySingle("Insert Into Tool_ErrorLog ( " +
                    //                                                        "   Error_Desc " +
                    //                                                        " , Error_FormName " +
                    //                                                        " , Error_MachinceName " +
                    //                                                        " , Error_Date " +
                    //                                                        " , Error_UserID " +
                    //                                                        " , Error_UserName " + ") values (" +
                    //                                                        "'" + strBuilder + "'," +
                    //                                                        "'BuklInsert'," +
                    //                                                        "'" + Dns.GetHostName().ToString() + "'," +
                    //                                                        "GetDate()," +
                    //                                                        "'" + Session["EmployeeId"] + "'," +
                    //                                                        "'" + Session["EmployeeName"] + "'");
                    //    TempData["Message"] = abc;
                    //    TempData["Icon"] = "error";
                    //}
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete Delete
        [HttpGet]
        public ActionResult Delete(int? ESICExemptedAllowancesMasterId, string ESICExemptedAllowancesMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ESICExemptedAllowancesMasterId", ESICExemptedAllowancesMasterId);
                param.Add("@p_ESICExemptedAllowancesMasterId_Encrypted", ESICExemptedAllowancesMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICExemptedAllowances_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_ESICExemptedAllowances");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        //#region IsValidation
        //[HttpGet]
        //public ActionResult IsESICExemptedExists(int CmpID, string ESICExemptedAllowancesMasterId_Encrypted)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        param.Add("@p_process", "IsValidation");
        //        param.Add("@p_ESICExemptedAllowancesMasterId_Encrypted", ESICExemptedAllowancesMasterId_Encrypted);
        //        param.Add("@p_CmpID", CmpID);
        //        param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //        param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //        param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //        var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ESICExemptedAllowances_Master", param);
        //        var Message = param.Get<string>("@p_msg");
        //        var Icon = param.Get<string>("@p_Icon");
        //        if (Message != "")
        //        {
        //            return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion



    }
}