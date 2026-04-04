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
    public class Setting_IncomeTax_TaxSlabController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_IncomeTax_TaxSlab
        #region Setting_IncomeTax_TaxSlab
        public ActionResult Setting_IncomeTax_TaxSlab(string TaxRateMasterId_Encrypted)
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
                ViewBag.AddUpdateTitle = "Add";
                IncomeTax_Slab Tax_Slab = new IncomeTax_Slab();
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@query", "Select TaxFyearId as Id ,TaxYear as Name from IncomeTax_Fyear where deactivate=0");
                var TaxYear = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                ViewBag.GetTaxYear = TaxYear;

                if (TaxRateMasterId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_TaxRateMasterId_Encrypted", TaxRateMasterId_Encrypted);
                    Tax_Slab = DapperORM.ReturnList<IncomeTax_Slab>("sp_List_IncomeTax_TaxRateMaster", paramList).FirstOrDefault();

                    var data = DapperORM.DynamicQueryList(@"Select IncomeTax_Fyear.TaxYear,IncomeTax_TaxRateMaster.TaxRateType,IncomeTax_TaxRateMaster.TaxRateAge,LowerLimit,UpperLimit,TaxRateAmount,TaxSurcharge,TaxRatePercentage,TaxRate_AboveAmt from IncomeTax_TaxRateDetails,IncomeTax_TaxRateMaster,IncomeTax_Fyear where IncomeTax_TaxRateDetails.TaxRate_MasterId=IncomeTax_TaxRateMaster.TaxRateMasterId and IncomeTax_Fyear.TaxFyearId=IncomeTax_TaxRateMaster.TaxFyearId and IncomeTax_TaxRateDetails.Deactivate=0  and  IncomeTax_TaxRateMaster.Deactivate=0  and IncomeTax_Fyear.Deactivate=0  and IncomeTax_TaxRateDetails.TaxRate_MasterId=" + Tax_Slab.TaxRateMasterId + "").ToList();
                    ViewBag.TaxSlab_Master = data;

                }
                return View(Tax_Slab);
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
        public ActionResult IsTaxSlabExists(int? TaxFyearId, int? TaxRateType, int? TaxRateAge, string TaxRateMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_TaxRateMasterId_Encrypted", TaxRateMasterId_Encrypted);
                param.Add("@p_TaxRateAgeId", TaxRateAge);
                param.Add("@p_TaxRateTypeId", TaxRateType);
                param.Add("@p_TaxFyearId", TaxFyearId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxRateMaster", param);
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
        public ActionResult SaveUpdate(List<IncomeTax_Slab_Bulk> Task, int? TaxFyearId, int? TaxRateType, int? TaxRateAge, string TaxRateTypeText, string TaxRateAgeText, string TaxRateMasterId_Encrypted, int? TaxRate_MasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                // StringBuilder strBuilderMonth = new StringBuilder();
                if (TaxRateMasterId_Encrypted == "")
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_TaxFyearId", TaxFyearId);
                    param.Add("@p_TaxRateType", TaxRateTypeText);
                    param.Add("@p_TaxRateTypeId", TaxRateType);
                    param.Add("@p_TaxRateAgeId", TaxRateAge);
                    param.Add("@p_TaxRateAge", TaxRateAgeText);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxRateMaster", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");


                    foreach (var Data in Task)
                    {
                        string TaxSlabMaster = "Insert Into IncomeTax_TaxRateDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , TaxRate_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                              " , TaxRateAmount " +
                                              " , TaxSurcharge " +
                                              " , TaxRate_AboveAmt " +
                                              " , TaxRatePercentage " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'" + Data.LowerLimit + "'," +
                                              "'" + Data.UpperLimit + "'," +
                                              "'" + Data.TaxRateAmount + "'," +
                                              "'0'," +
                                             "'" + Data.TaxRate_AboveAmt + "'," +
                                              "'" + Data.TaxRatePercentage + "')" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(TaxSlabMaster);

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
                    DapperORM.DynamicQuerySingle("update IncomeTax_TaxRateMaster set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where TaxRateMasterId_Encrypted='" + TaxRateMasterId_Encrypted + "'");
                    DapperORM.DynamicQuerySingle("update IncomeTax_TaxRateDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where TaxRate_MasterId=" + TaxRate_MasterId + "");
                    foreach (var Data in Task)
                    {
                        string TaxSlabMaster = "Insert Into IncomeTax_TaxRateDetails(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , TaxRate_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                              " , TaxRateAmount " +
                                              " , TaxSurcharge " +
                                              " , TaxRate_AboveAmt " +
                                              " , TaxRatePercentage " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + TaxRate_MasterId + "'," +
                                              "'" + Data.LowerLimit + "'," +
                                              "'" + Data.UpperLimit + "'," +
                                              "'" + Data.TaxRateAmount + "'," +
                                              "'0'," +
                                              "'" + Data.TaxRate_AboveAmt + "'," +
                                              "'" + Data.TaxRatePercentage + "')" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(TaxSlabMaster);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
                    }
                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }

                ////return RedirectToAction("Setting_Statutory_PTSlab", "Setting_Statutory_PTSlab");
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 505;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_TaxRateMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_IncomeTax_TaxRateMaster", param).ToList();
                ViewBag.TaxRateMasterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete With 
        [HttpGet]
        public ActionResult Delete(int? TaxRateMasterId, string TaxRateMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TaxRateMasterId", TaxRateMasterId);
                param.Add("@p_TaxRateMasterId_Encrypted", TaxRateMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_IncomeTax_TaxRateMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_IncomeTax_TaxSlab");
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
