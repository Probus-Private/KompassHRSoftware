using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_Statutory;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_ComplienceDocTypeController : Controller
    {
        // GET: Setting/Setting_Statutory_ComplienceDocType
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        public ActionResult Setting_Statutory_ComplienceDocType(string ComplienceDocTypeId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 498;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_DocType ObjDocType = new Payroll_DocType();

                if (ComplienceDocTypeId_Encrypted != null)
                {
                    //param = new DynamicParameters();
                    //param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                    //fnf_NoDuesCheckList = DapperORM.ReturnList<FNF_NoDuesCheckList>("sp_List_Payroll_ComplienceDocType", param).FirstOrDefault();

                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@P_ComplienceDocTypeId_Encrypted", ComplienceDocTypeId_Encrypted);
                    ObjDocType = DapperORM.ReturnList<Payroll_DocType>("sp_List_Payroll_ComplienceDocType", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS SrNo, SubTypeName as Answer from Payroll_ComplienceSubType where ComplienceDocTypeId ="+ObjDocType.ComplienceDocTypeId+" and Deactivate = 0");
                    var List_SubTypeName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetSubTypeName = List_SubTypeName;

                }
                return View(ObjDocType);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetList Main View
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 498;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@P_ComplienceDocTypeId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_ComplienceDocType", param).ToList();
                ViewBag.GetComplienceDocList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidaton
        [HttpGet]
        public ActionResult IsDocumentExists(string ComplienceDocTypeName, string ComplienceDocTypeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@P_ComplienceDocTypeId_Encrypted", ComplienceDocTypeId_Encrypted);
                param.Add("@P_ComplienceDocTypeName", ComplienceDocTypeName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ComplienceDocType", param);
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
        public ActionResult SaveUpdate(Payroll_DocType ObjDocType, List<Payroll_DocType> Task, string ComplienceDocTypeName, string ComplienceDocTypeId_Encrypted, int ComplienceDocTypeId)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(ComplienceDocTypeId_Encrypted) ? "Save" : "Update");
                param.Add("@p_ComplienceDocTypeId", ComplienceDocTypeId);
                param.Add("@p_ComplienceDocTypeId_Encrypted", ComplienceDocTypeId_Encrypted);
                param.Add("@p_ComplienceDocTypeName", ComplienceDocTypeName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_ComplienceDocType", param);
                var PID = param.Get<string>("@p_Id");

                if (ComplienceDocTypeId_Encrypted != "")
                {
                    DapperORM.DynamicQuerySingle("update Payroll_ComplienceSubType set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where ComplienceDocTypeId=" + ComplienceDocTypeId + "");
                    if (Task != null)
                    {
                        foreach (var Data in Task)
                        {
                            string Answer = "Insert Into Payroll_ComplienceSubType(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , ComplienceDocTypeId " +
                                                  " , SubTypeName " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + ComplienceDocTypeId + "'," +
                                                  "'" + Data.SubTypeName + "'" +
                                                  ")" +
                                                   " " +
                                                  " " +
                                                   " ";
                            strBuilder.Append(Answer);

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
                    }


                }
                else
                {
                    foreach (var Data in Task)
                    {
                        string Answer = "Insert Into Payroll_ComplienceSubType(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , ComplienceDocTypeId " +
                                              " , SubTypeName " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'" + Data.SubTypeName + "'" +
                                              ")" +
                                               " " +
                                              " " +
                                               " ";
                        strBuilder.Append(Answer);

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
                }
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Setting_FullAndFinal_NoDuesChecklist", "Setting_FullAndFinal_NoDuesChecklist");
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
        public ActionResult Delete(string ComplienceDocTypeId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_ComplienceDocTypeId_Encrypted", ComplienceDocTypeId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_ComplienceDocType", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_ComplienceDocType");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region view
        public ActionResult ViewModule(string ComplienceDocTypeId)
        {
            try
            {
                var que = DapperORM.DynamicQueryList("select SubTypeName from Payroll_ComplienceSubType where ComplienceDocTypeId=" + ComplienceDocTypeId + " and Deactivate=0").ToList();
                return Json(que, JsonRequestBehavior.AllowGet);
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