using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace KompassHR.Areas.Setting.Controllers.Setting_FullAndFinal
{
    public class Setting_FullAndFinal_NoDuesChecklistController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();


        #region NoDuesChecklist main View
        [HttpGet]
        public ActionResult Setting_FullAndFinal_NoDuesChecklist(string FNFNoDuesId_Encrypted,string ClearanceTitle)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 71;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                FNF_NoDuesCheckList fnf_NoDuesCheckList = new FNF_NoDuesCheckList();
                if (FNFNoDuesId_Encrypted != null)
                {
                    //param = new DynamicParameters();
                    //param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                    //fnf_NoDuesCheckList = DapperORM.ReturnList<FNF_NoDuesCheckList>("sp_List_FNF_NoDuesCheckList", param).FirstOrDefault();
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@P_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                    fnf_NoDuesCheckList = DapperORM.ReturnList<FNF_NoDuesCheckList>("sp_List_FNF_DuesAndClearenceList", param).FirstOrDefault();

                    DynamicParameters paramOption = new DynamicParameters();
                    paramOption.Add("@query", "select ROW_NUMBER() OVER(ORDER BY(SELECT 1)) AS SrNo, RequiredClearanceName as Answer from FNF_DuesAndClearence_Details where NoDuesAndClearenceTitle_MasterID = "+fnf_NoDuesCheckList.FNFNoDuesId+" and Deactivate = 0");
                    var List_Options = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramOption).ToList();
                    ViewBag.GetOptions = List_Options;

                }
                return View(fnf_NoDuesCheckList);
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
        public ActionResult IsNoDuesCheckListExists(string ClearanceTitle, string FNFNoDuesId_Encrypted)
        {
            try
            {             
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                    param.Add("@p_NoDuesCheckListName", ClearanceTitle);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_DuesAndClearence", param);
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
        public ActionResult SaveUpdate(FNF_NoDuesCheckList NoDuesCheckList, List<FNF_Questions_option> Task, string ClearanceTitle,string FNFNoDuesId_Encrypted,int FNFNoDuesId)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(FNFNoDuesId_Encrypted) ? "Save" : "Update");
                param.Add("@p_FNFNoDuesId", FNFNoDuesId);
                param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                param.Add("@p_NoDuesCheckListName", ClearanceTitle);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_DuesAndClearence", param);
                var PID = param.Get<string>("@p_Id");

                if (FNFNoDuesId_Encrypted != "")
                {
                    DapperORM.DynamicQuerySingle("update FNF_DuesAndClearence_Details set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where NoDuesAndClearenceTitle_MasterID=" + FNFNoDuesId + "");
                    if (Task != null)
                    {
                        foreach (var Data in Task)
                        {
                            string Answer = "Insert Into FNF_DuesAndClearence_Details(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , NoDuesAndClearenceTitle_MasterID " +
                                                  " , RequiredClearanceName " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + FNFNoDuesId + "'," +
                                                  "'" + Data.Options + "'" +
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
                        string Answer = "Insert Into FNF_DuesAndClearence_Details(" +
                                              "   Deactivate " +
                                              " , CreatedBy " +
                                              " , CreatedDate " +
                                              " , MachineName " +
                                              " , NoDuesAndClearenceTitle_MasterID " +
                                              " , RequiredClearanceName " +
                                               ") values (" +
                                              "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                              "'" + PID + "'," +
                                              "'" + Data.Options + "'" +
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 71;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_FNFNoDuesId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_DuesAndClearenceList", param).ToList();
                ViewBag.GetNoDuesCheckListList = data;
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
        public ActionResult Delete(string FNFNoDuesId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FNFNoDuesId_Encrypted", FNFNoDuesId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoDuesCheckList", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FullAndFinal_NoDuesChecklist");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region view
        public ActionResult Viewa(string FNFNoDuesId)
        {
            try
            {
                var que = DapperORM.DynamicQuerySingle("select RequiredClearanceName from FNF_DuesAndClearence_Details where NoDuesAndClearenceTitle_MasterID="+ FNFNoDuesId + " and Deactivate=0");
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