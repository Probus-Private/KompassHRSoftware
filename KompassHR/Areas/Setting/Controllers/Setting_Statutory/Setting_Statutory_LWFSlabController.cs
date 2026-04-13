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
    public class Setting_Statutory_LWFSlabController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: StatutorySetting/StatutoryLWFSlab
        #region LWFSlab MAin View
        [HttpGet]
        public ActionResult Setting_Statutory_LWFSlab(string LWFSlabMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 99;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_LWFSlab payroll_LWFSlab = new Payroll_LWFSlab();
                Payroll_LWFSlab_Master Master_LWFSlab = new Payroll_LWFSlab_Master();

                DynamicParameters paramstatesName = new DynamicParameters();
                paramstatesName.Add("@query", "Select StateId  as Id ,StateName as Name from  Mas_States Where Deactivate=0 and LWFApplicable=1");
                var listmas_states = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramstatesName).ToList();
                ViewBag.GetstatesName = listmas_states;

                if (LWFSlabMasterId_Encrypted != null)
                {
                    DynamicParameters paramList = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    paramList.Add("@p_LWFSlabMasterId_Encrypted", LWFSlabMasterId_Encrypted);
                    payroll_LWFSlab = DapperORM.ReturnList<Payroll_LWFSlab>("sp_List_Payroll_LWFSlab_Master", paramList).FirstOrDefault();

                    var data = DapperORM.DynamicQueryList(@"Select Mas_States.StateName,LowerLimit,UpperLimit,LWFEmployee,LWFEmployer from [Payroll_LWFSlab],Mas_States,Payroll_LWFSlab_Master where  [Payroll_LWFSlab].Deactivate=0 and LWFSlab_MasterId =" + payroll_LWFSlab.LWFSlabMasterId + " and Payroll_LWFSlab_Master.LWFStateCode=Mas_States.StateId and Payroll_LWFSlab.LWFSlab_MasterId=Payroll_LWFSlab_Master.LWFSlabMasterId").ToList();
                    ViewBag.LWFSlabLimit = data;

                    DynamicParameters parammonth = new DynamicParameters();
                    parammonth.Add("@query", "Select LWFSlab_Month,IsApplicable from Payroll_LWFSlab_Month where LWFSlab_MasterId=" + payroll_LWFSlab.LWFSlabMasterId + "");
                    ViewBag.LWFSlabMonth = DapperORM.ReturnList<Payroll_LWFSlab_Month>("sp_QueryExcution", parammonth).ToList();
                   // ViewBag.GetstatesName = listmas_states;
                   // var data1 = DapperORM.DynamicQuerySingle(@"Select LWFSlab_Month,IsApplicable from Payroll_LWFSlab_Month where LWFSlab_MasterId=" + payroll_LWFSlab.LWFSlabMasterId + "").ToList();
                    //ViewBag.LWFSlabMonth = data1;
                }
                //TempData["LWFStateCode"] = LWFStateCode;
                //TempData["StateId_Encrypted"] = StateId_Encrypted;
                return View(payroll_LWFSlab);
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
        public ActionResult GetLowerlimit(int? LWFStateCode)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Limit = DapperORM.DynamicQuerySingle("Select Max(UpperLimit)As UpprLimit from Payroll_LWFSlab where Deactivate=0 And [LWFStateCode] =" + LWFStateCode + "");
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
        public ActionResult IsLWFSlabExists(string StateCode, string Remark, string LWFSlabMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_LWFSlabMasterId_Encrypted", LWFSlabMasterId_Encrypted);
                param.Add("@p_Remark", Remark);
                param.Add("@p_LWFStateCode", StateCode);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFSlab_Master", param);
                var Message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
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
        public ActionResult SaveUpdate(List<Payroll_LWFSlab> Task, string StateCode, string Remark, string LWFSlabMasterId_Encrypted, List<Payroll_LWFSlab_Month> Record,int? LWFSlabMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                StringBuilder strBuilder = new StringBuilder();
                StringBuilder strBuilderMonth = new StringBuilder();
                if (LWFSlabMasterId_Encrypted == "")
                {
                    param.Add("@p_process", "Save");
                    param.Add("@p_LWFStateCode", StateCode);
                    param.Add("@p_Remark", Remark);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFSlab_Master", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var PID = param.Get<string>("@p_Id");


                    foreach (var Data in Task)
                    {
                        string LWFSlabMaster = "INSERT INTO Payroll_LWFSlab(" +
                         "Deactivate, CreatedBy, CreatedDate, MachineName, " +
                         "LWFSlab_MasterId, LowerLimit, UpperLimit, LWFEmployee, LWFEmployer) " +
                         "VALUES (" +
                         "0," + // Deactivate bit
                         "'" + Session["EmployeeName"] + "'," +
                         "GETDATE()," +
                         "'" + Dns.GetHostName().ToString() + "'," +
                         PID + "," +
                         Data.LowerLimit + "," +
                         Data.UpperLimit + "," +
                         Data.LWFEmployee + "," +
                         Data.LWFEmployer + ");";

                        strBuilder.Append(LWFSlabMaster);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record save successfully";
                        TempData["Icon"] = "success";
                    }


                    foreach (var Data in Record)
                    {
                        string LWFSlabMasterMonth = "INSERT INTO Payroll_LWFSlab_Month(" +
                                "LWFSlab_MasterId, LWFSlab_Month, IsApplicable) " +
                                "VALUES (" +
                                PID + "," +
                                Data.LWFSlab_Month + "," +
                                (Data.IsApplicable ? "1" : "0") + ");";

                        strBuilderMonth.Append(LWFSlabMasterMonth);

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
                    DapperORM.DynamicQuerySingle("Update Payroll_LWFSlab_Master set ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=Getdate()  where LWFSlabMasterId_Encrypted='" + LWFSlabMasterId_Encrypted + "'");
                    DapperORM.DynamicQuerySingle("Update Payroll_LWFSlab set Deactivate=1 ,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=Getdate()  where LWFSlab_MasterId=" + LWFSlabMasterId + "");
                    foreach (var Data in Task)
                    {
                        string LWFSlabMaster = "Insert Into Payroll_LWFSlab(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , LWFSlab_MasterId " +
                                              " , LowerLimit " +
                                              " , UpperLimit " +
                                              " , LWFEmployee " +
                                              " , LWFEmployer " + ") values (" +
                                              "0," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "" + LWFSlabMasterId + "," +
                                              "" + Data.LowerLimit + "," +
                                              "" + Data.UpperLimit + "," +
                                              "" + Data.LWFEmployee + "," +
                                              "" + Data.LWFEmployer + ")" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilder.Append(LWFSlabMaster);

                    }
                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {

                        TempData["Message"] = "Record update successfully";
                        TempData["Icon"] = "success";
                    }

                    foreach (var Data in Record)
                    {
                        string LWFSlabMasterMonth = "  Update Payroll_LWFSlab_Month set IsApplicable="+ Data.IsApplicable+ " where LWFSlab_MasterId="+ LWFSlabMasterId + " and LWFSlab_Month='"+Data.LWFSlab_Month+"'" +
                                               " " +
                                               " " +
                                               " ";
                        strBuilderMonth.Append(LWFSlabMasterMonth);

                    }
                    string abcd = "";
                    if (objcon.SaveStringBuilder(strBuilderMonth, out abcd))
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
               // return RedirectToAction("Setting_Statutory_PTSlab", "Setting_Statutory_PTSlab");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 99;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_LWFSlabMasterId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_LWFSlab_Master", param).ToList();
                ViewBag.LWFSlabGetList = data;
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
        public ActionResult DeleteState(string LWFSlabMasterId_Encrypted,int LWFSlabMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_LWFSlabMasterId_Encrypted", LWFSlabMasterId_Encrypted);
                param.Add("@p_LWFSlabMasterId", LWFSlabMasterId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_LWFSlab_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_LWFSlab");
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