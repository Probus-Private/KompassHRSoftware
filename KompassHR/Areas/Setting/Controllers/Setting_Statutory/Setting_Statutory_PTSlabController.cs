using Dapper;
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
    public class Setting_Statutory_PTSlabController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        // GET: StatutorySetting/StatutoryPTSlab
        #region PTSlab MAin View
        [HttpGet]
        public ActionResult Setting_Statutory_PTSlab(string PTSlabMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 97;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_PTSlab payroll_ptslab = new Payroll_PTSlab();

                DynamicParameters paramstatesName = new DynamicParameters();
                paramstatesName.Add("@query", "Select StateId as Id,StateName as Name from  Mas_States Where Deactivate=0 And PTApplicable=1");
                var listmas_states = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramstatesName).ToList();
                ViewBag.GetstatesName = listmas_states;


                if (PTSlabMasterId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_PTSlabMasterId_Encrypted", PTSlabMasterId_Encrypted);
                    payroll_ptslab = DapperORM.ReturnList<Payroll_PTSlab>("sp_List_Payroll_PTSlab_Master", paramList).FirstOrDefault();

                    var data = DapperORM.DynamicQueryList(@"Select Mas_States.StateName,Payroll_PTSlab_Master.PTDeductionType,Payroll_PTSlab_Master.Remark,LowerLimit,UpperLimit,PTAmount,FebPTAmount, CASE  WHEN Payroll_PTSlab.PTSlabMonth = 0 THEN 'All'  WHEN Payroll_PTSlab.PTSlabMonth = 1 THEN '01 - Jan'WHEN Payroll_PTSlab.PTSlabMonth = 2 THEN '02 - Feb' WHEN Payroll_PTSlab.PTSlabMonth = 3 THEN '03 - Mar' WHEN Payroll_PTSlab.PTSlabMonth = 4 THEN '04 - Apr' WHEN Payroll_PTSlab.PTSlabMonth = 5 THEN '05 - May' WHEN Payroll_PTSlab.PTSlabMonth = 6 THEN '06 - Jun' WHEN Payroll_PTSlab.PTSlabMonth = 7 THEN '07 - Jul' WHEN Payroll_PTSlab.PTSlabMonth = 8 THEN '08 - Aug' WHEN Payroll_PTSlab.PTSlabMonth = 9 THEN '09 - Sep' WHEN Payroll_PTSlab.PTSlabMonth = 10 THEN '10 - Oct' WHEN Payroll_PTSlab.PTSlabMonth = 11 THEN '11 - Nov'  WHEN Payroll_PTSlab.PTSlabMonth = 12 THEN '12 - Dec' ELSE 'Unknown Month' END AS PTSlabMonth , Payroll_PTSlab.PTSlabMonth as Month from Payroll_PTSlab,Mas_States,Payroll_PTSlab_Master  where Mas_States.StateId=Payroll_PTSlab_Master.PTStateCode and Payroll_PTSlab.PTSlab_MasterId=Payroll_PTSlab_Master.PTSlabMasterId and Payroll_PTSlab.deactivate=0 and Payroll_PTSlab_Master.deactivate=0  and Payroll_PTSlab.PTSlab_MasterId=" + payroll_ptslab.PTSlabMasterId + "").ToList();
                    ViewBag.PTSlab_Master = data;

                    DynamicParameters parammonth = new DynamicParameters();
                    parammonth.Add("@query", "Select PTSlab_Month,IsApplicable from Payroll_PTSlab_Month where PTSlab_MasterId=" + payroll_ptslab.PTSlabMasterId + "");
                    ViewBag.PTSlabMonth = DapperORM.ReturnList<Payroll_PTSlab_Month>("sp_QueryExcution", parammonth).ToList();
                }
                //TempData["PTStateCode"] = PTStateCode;
                //TempData["StateId_Encrypted"] = StateId_Encrypted;
                return View(payroll_ptslab);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLowerlimit
        [HttpGet]
        public ActionResult GetLowerlimit(int? PTStateCode, string GetGender)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var Limit = DapperORM.DynamicQuerySingle("Select Max(UpperLimit)As UpprLimit from Payroll_PTSlab where Deactivate=0 and [PTStateCode] =" + PTStateCode + " and Payroll_PTSlab.Remark='" + GetGender + "'");
                return Json(Limit, JsonRequestBehavior.AllowGet);
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
        //public ActionResult IsPTSlabExists(List<Payroll_PTSlab> Task/*int PTStateCode, string PTSlabId_Encrypted, float LowerLimit, float UpperLimit, float PTAmount, float FebPTAmount*//*,string Gender*/)
        public ActionResult IsPTSlabExists(string StateCode, string Remark, string PTSlabMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PTSlabMasterId_Encrypted", PTSlabMasterId_Encrypted);
                param.Add("@p_Remark", Remark);
                param.Add("@p_PTStateCode", StateCode);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_PTSlab_Master", param);
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
        public ActionResult SaveUpdate(List<Payroll_PTSlab> Task, string StateCode, string PTDeductionType, string Remark, string PTSlabMasterId_Encrypted, int? PTSlabMasterId, int? PTSlabMonth, List<Payroll_PTSlab_Month> Record)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                StringBuilder strBuilder = new StringBuilder();
                StringBuilder strBuilderMonth = new StringBuilder();
                if (PTSlabMasterId_Encrypted == "")
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_PTDeductionType", PTDeductionType);
                    param.Add("@p_Remark", Remark);
                    param.Add("@p_PTStateCode", Convert.ToInt32(StateCode));
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PTSlab_Master", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");

                    foreach (var Data in Task)
                    {
                        //var GetPTSlabMonth = "";
                        //if (PTSlabMonth=="")
                        //{
                        //    GetPTSlabMonth = "0";
                        //}
                        //else
                        //{
                        //    GetPTSlabMonth = PTSlabMonth;
                        //}
                        string PtSlabMaster = "Insert Into Payroll_PTSlab(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , PTSlab_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                              " , PTSlabMonth " +
                                              " , PTAmount " +
                                              " , FebPTAmount " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'" + Data.LowerLimit + "'," +
                                              "'" + Data.UpperLimit + "'," +
                                              "'" + PTSlabMonth + "'," +
                                              "'" + Data.PTAmount + "'," +
                                              "'" + Data.FebPTAmount + "')" +
                                              " " +
                                              " " +
                                               " ";
                        strBuilder.Append(PtSlabMaster);

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



                    foreach (var Data in Record)
                    {
                        string PTSlabMasterMonth = "Insert Into Payroll_PTSlab_Month(" +
                                              "   PTSlab_MasterId " +
                                              " , PTSlab_Month " +
                                              " , IsApplicable " + ") values (" +
                                              "'" + PID + "'," +
                                              "'" + Data.PTSlab_Month + "'," +
                                              "'" + Data.IsApplicable + "')" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilderMonth.Append(PTSlabMasterMonth);

                    }

                    string abcd = "";
                    if (objcon.SaveStringBuilder(strBuilderMonth, out abcd))
                    {

                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }
                    if (abcd != "")
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
                        TempData["Message"] = abcd;
                        TempData["Icon"] = "error";
                    }


                    return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DapperORM.DynamicQuerySingle("update Payroll_PTSlab_Master set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where PTSlabMasterId_Encrypted='" + PTSlabMasterId_Encrypted + "'");
                    DapperORM.DynamicQuerySingle("update Payroll_PTSlab set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE() where PTSlab_MasterId=" + PTSlabMasterId + "");
                    foreach (var Data in Task)
                    {
                        string PtSlabMaster = "Insert Into Payroll_PTSlab(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , PTSlab_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                                " , PTSlabMonth " +
                                              " , PTAmount " +
                                              " , FebPTAmount " + ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PTSlabMasterId + "'," +
                                              "'" + Data.LowerLimit + "'," +
                                              "'" + Data.UpperLimit + "'," +
                                                  "'" + PTSlabMonth + "'," +
                                              "'" + Data.PTAmount + "'," +
                                              "'" + Data.FebPTAmount + "')" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilder.Append(PtSlabMaster);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
                    }

                    foreach (var Data in Record)
                    {
                        string PTSlabMasterMonth = "  Update Payroll_PTSlab_Month set IsApplicable='" + Data.IsApplicable + "' where PTSlab_MasterId=" + PTSlabMasterId + " and PTSlab_Month='" + Data.PTSlab_Month + "'" +
                                               " " +
                                               " " +
                                               " ";
                        strBuilderMonth.Append(PTSlabMasterMonth);

                    }
                    string abcd = "";
                    if (objcon.SaveStringBuilder(strBuilderMonth, out abcd))
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 97;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_PTSlabMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_PTSlab_Master", param).ToList();
                ViewBag.PTSlabGetList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete With State Wise
        [HttpGet]
        public ActionResult DeleteState(int? PTSlabMasterId, string PTSlabMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PTSlabMasterId", PTSlabMasterId);
                param.Add("@p_PTSlabMasterId_Encrypted", PTSlabMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_PTSlab_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_PTSlab");
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