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
    public class Setting_Statutory_PFWagesController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_Statutory_PFWages
        #region Setting_Statutory_PFWages
        public ActionResult Setting_Statutory_PFWages(int? CmpId, string PFWagesMasterId_Encrypted)
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
                Payroll_PFWages_Master PFWages_Master = new Payroll_PFWages_Master();

                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.GetCompanyname = GetComapnyName;

                if (CmpId != null)
                {
                    DynamicParameters paramESIC = new DynamicParameters();
                    paramESIC.Add("@p_PayrollMap_CompanyId", CmpId);
                    paramESIC.Add("@p_Process", "Save");
                    ViewBag.GetCTCHead = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_PFWages", paramESIC).ToList();
                }
                else
                {
                    ViewBag.GetCTCHead = "";
                }
                if (PFWagesMasterId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_PFWagesMasterId_Encrypted", PFWagesMasterId_Encrypted);
                    PFWages_Master = DapperORM.ReturnList<Payroll_PFWages_Master>("sp_List_Payroll_PFWages_Master", paramList).FirstOrDefault();

                    DynamicParameters paramESIC = new DynamicParameters();
                    paramESIC.Add("@p_PayrollMap_CompanyId", PFWages_Master.CmpID);
                    paramESIC.Add("@p_PFWagesMasterId_Encrypted", PFWagesMasterId_Encrypted);
                    paramESIC.Add("@p_Process", "Update");

                    ViewBag.GetCTCHead = DapperORM.ExecuteSP<dynamic>("sp_GetCTCHead_PFWages", paramESIC).ToList();
                }
                    return View(PFWages_Master);
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
                param.Add("@p_PFWagesMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_PFWages_Master", param).ToList();
                ViewBag.PFWageGetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpGet]
        public ActionResult IsPFWagesExists(int? CmpID,string PFWagesRemark, string PFWagesMasterId_Encrypted)
        {
            try
            {

                param.Add("@p_process", "IsValidation");
                param.Add("@p_PFWagesRemark", PFWagesRemark);
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_PFWagesMasterId_Encrypted", PFWagesMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PFWages_Master", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                if (Icon == "error")
                {
                    return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
                }


                return Json(true, JsonRequestBehavior.AllowGet);
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
        public ActionResult SaveUpdate(List<Payroll_PFWages_Detail> Task, int? CmpID, string PFWagesRemark,int? PFWagesMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
               // var GetESICCunt = DapperORM.DynamicQuerySingle("Select count(PFWagesMasterId) as PFWagesMasterIdCount from Payroll_PFWages_Master where  deactivate=0 and  CmpID=" + CmpID + " and lower(PFWagesRemark)=lower('"+ PFWagesRemark + "')");
                if (PFWagesMasterId != 0)
                {

                    //DynamicParameters paramBranch = new DynamicParameters();
                    //paramBranch.Add("@query", "Select PFWagesMasterId as Id,'' as Name from Payroll_PFWages_Master where  deactivate=0 and  CmpID=" + CmpID + " and lower(PFWagesRemark)=lower('" + PFWagesRemark + "') ");
                    //var GetMasterId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();

                    DapperORM.DynamicQuerySingle("update Payroll_PFWages_Master set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where PFWagesMasterId='" + PFWagesMasterId + "'");
                    DapperORM.DynamicQuerySingle("Delete from Payroll_PFWages_Detail where PFWagesDetailId_MasterId=" + PFWagesMasterId + "");

                    param.Add("@p_process", "Update");
                    param.Add("@p_PFWagesMasterId", PFWagesMasterId);
                    param.Add("@p_PFWagesRemark", PFWagesRemark);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PFWages_Master", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");


                    foreach (var Data in Task)
                    {
                        string ESICExemptedallowances = "Insert Into Payroll_PFWages_Detail(" +
                                              "   PFWagesDetailId_MasterId " +
                                              " , PFWagesName " +
                                              " , PFWagesName_TempTable " +
                                              " , IsActive " +
                                              ") values (" +
                                              "'" + PFWagesMasterId + "'," + "'" + Data.PFWagesName + "'," +
                                              "'" + Data.PFWagesName_TempTable + "'," +
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
                else
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_CmpID", CmpID);
                    param.Add("@p_PFWagesRemark", PFWagesRemark);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PFWages_Master", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");


                    foreach (var Data in Task)
                    {
                        string ESICExemptedallowances = "Insert Into Payroll_PFWages_Detail(" +
                                              "   PFWagesDetailId_MasterId " +
                                              " , PFWagesName " +
                                              " , PFWagesName_TempTable " +
                                              " , IsActive " +
                                              ") values (" +
                                              "'" + PID + "'," + "'" + Data.PFWagesName + "'," +
                                              "'" + Data.PFWagesName_TempTable + "'," +
                                              "'1')" +
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
        public ActionResult Delete(int? PFWagesMasterId, string PFWagesMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PFWagesMasterId", PFWagesMasterId);
                param.Add("@p_PFWagesMasterId_Encrypted", PFWagesMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PFWages_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_PFWages");
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