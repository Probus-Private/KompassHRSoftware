using Dapper;
using KompassHR.Areas.Setting.Models.Setting_IncomeTax;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_IncomeTax
{
    public class Setting_IncomeTax_SurchargeSettingController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_IncomeTax_SurchargeSetting

        #region Setting_IncomeTax_SurchargeSetting
        public ActionResult Setting_IncomeTax_SurchargeSetting(string TaxSurchargeMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 505;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                IncomeTax_TaxSurchargeDetails Tax_Surcharge = new IncomeTax_TaxSurchargeDetails();
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select TaxFyearId as Id ,TaxYear as Name from IncomeTax_Fyear where deactivate=0");
                var TaxYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetTaxYear = TaxYear;

                if (TaxSurchargeMasterId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_TaxSurcahrgeMasterId_Encrypted", TaxSurchargeMasterId_Encrypted);
                    Tax_Surcharge = DapperORM.ReturnList<IncomeTax_TaxSurchargeDetails>("sp_List_IncomeTax_TaxSurchargeMaster", paramList).FirstOrDefault();

                    var data = DapperORM.DynamicQueryList(@"Select IncomeTax_Fyear.TaxYear,IncomeTax_TaxSurchargeMaster.TaxSurchargeType,LowerLimit,UpperLimit,TaxSurcharge,TaxSurcharge_AboveAmt from IncomeTax_TaxSurchargeDetails,IncomeTax_TaxSurchargeMaster,IncomeTax_Fyear where IncomeTax_TaxSurchargeDetails.TaxSurcharge_MasterId=IncomeTax_TaxSurchargeMaster.TaxSurchargeMasterId and IncomeTax_Fyear.TaxFyearId=IncomeTax_TaxSurchargeMaster.TaxFyearId and IncomeTax_TaxSurchargeDetails.Deactivate=0  and  IncomeTax_TaxSurchargeMaster.Deactivate=0  and IncomeTax_Fyear.Deactivate=0  and IncomeTax_TaxSurchargeDetails.TaxSurcharge_MasterId=" + Tax_Surcharge.TaxSurcharge_MasterId + "").ToList();
                    ViewBag.TaxSurcharge_Master = data;

                }
                return View(Tax_Surcharge);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion Setting_IncomeTax_SurchargeSetting


        #region IsValidation
        [HttpGet]
        public ActionResult IsTaxSurchargeExists(int? TaxFyearId, int? TaxSurchargeTypeId,  string TaxSurchargeMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_TaxSurchargeMasterId_Encrypted", TaxSurchargeMasterId_Encrypted);             
                param.Add("@p_TaxFyearId", TaxFyearId);
                param.Add("@p_TaxSurchargeTypeId", TaxSurchargeTypeId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxSurchargeMaster", param);
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

        #region SaveUpdate

        [HttpPost]
        public ActionResult SaveUpdate(List<IncomeTax_Surcharge_Bulk> Task, int? TaxFyearId, int? TaxSurchargeTypeId,  string TaxSurchargeTypeText,  string TaxSurchargeMasterId_Encrypted, int? TaxSurcharge_MasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                // StringBuilder strBuilderMonth = new StringBuilder();
                DynamicParameters param = new DynamicParameters();
                if (TaxSurchargeMasterId_Encrypted == "")
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_TaxFyearId", TaxFyearId);
                    param.Add("@p_TaxSurchargeType", TaxSurchargeTypeText);
                    param.Add("@p_TaxSurchargeTypeId", TaxSurchargeTypeId);                    
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxSurchargeMaster", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");

                    foreach (var Data in Task)
                    {
                        string TaxSurcargeMaster = "Insert Into IncomeTax_TaxSurchargeDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , TaxSurcharge_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                              " , TaxSurcharge " +
                                              " , TaxSurcharge_AboveAmt" +
                                              "  " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "" + Data.LowerLimit + "," +
                                              "" + Data.UpperLimit + "," +
                                              "" + Data.TaxSurcharge + "," +
                                              "" + Data.TaxSurcharge_AboveAmt + ")" +
                                              " " +
                                              " " +
                                              " ";
                        strBuilder.Append(TaxSurcargeMaster);
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
                else
                {
                    DapperORM.DynamicQuerySingle("update IncomeTax_TaxSurchargeMaster set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where TaxSurchargeMasterId_Encrypted='" + TaxSurchargeMasterId_Encrypted + "'");
                    DapperORM.DynamicQuerySingle("update IncomeTax_TaxSurchargeDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where TaxSurcharge_MasterId=" + TaxSurcharge_MasterId + "");
                    foreach (var Data in Task)
                    {
                        string TaxSurcargeMaster = "Insert Into IncomeTax_TaxSurchargeDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , TaxSurcharge_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +                                            
                                              " , TaxSurcharge " +
                                              " , TaxSurcharge_AboveAmt" +
                                              "  " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + TaxSurcharge_MasterId + "'," +
                                              "'" + Data.LowerLimit + "'," +
                                              "'" + Data.UpperLimit + "'," +
                                              "'" + Data.TaxSurcharge + "'," +
                                               "" + Data.TaxSurcharge_AboveAmt + ")" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(TaxSurcargeMaster);
                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
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

        #endregion SaveUpdate

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 505;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TaxSurcahrgeMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_IncomeTax_TaxSurchargeMaster", param).ToList();
                ViewBag.TaxSurchargeMasterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion GetList

        #region Delete With 
        [HttpGet]
        public ActionResult Delete(int? TaxSurchargeMasterId, string TaxSurchargeMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TaxSurchargeMasterId", TaxSurchargeMasterId);
                param.Add("@p_TaxSurchargeMasterId_Encrypted", TaxSurchargeMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxSurchargeMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_IncomeTax_SurchargeSetting");
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