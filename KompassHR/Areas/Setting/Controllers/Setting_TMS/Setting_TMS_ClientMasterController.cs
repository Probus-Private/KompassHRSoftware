using Dapper;
using KompassHR.Areas.Setting.Models.Setting_TMS;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_TMS
{
    public class Setting_TMS_ClientMasterController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        clsCommonFunction objcon = new clsCommonFunction();

        // GET: TMSSetting/ClientMaster
        public ActionResult Setting_TMS_ClientMaster(string ClientID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 87;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                TMS_Client TMS_Client = new TMS_Client();
                if (ClientID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_ClientID_Encrypted", ClientID_Encrypted);
                    // TMS_Client = DapperORM.ReturnList<TMS_Client>("sp_List_TMS_Client", param).FirstOrDefault();
                    var data = DapperORM.ReturnList<TMS_Client>("sp_List_TMS_Client", param).ToList();

                    if (data.Count > 0)
                    {
                        TMS_Client = data.First();

                        TMS_Client.ClientDetails = data
                            .Where(x => x.ClientDetailsId > 0)
                            .Select(x => new TMS_ClientDetails
                            {
                                ClientDetailsId = x.ClientDetailsId,
                                ResponsiblePerson = x.ResponsiblePerson
                            }).ToList();
                    }

                }

                return View(TMS_Client);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        public ActionResult IsClientMasterExists(string ClientName, string ClientIDEncrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 87;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("p_ClientName", ClientName);
                    param.Add("@p_ClientID_Encrypted", ClientIDEncrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_Client", param);
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
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult SaveUpdate(TMS_Client TMSClient)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 87;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(TMSClient.ClientID_Encrypted) ? "Save" : "Update");
                param.Add("@p_ClientID", TMSClient.ClientID);
                param.Add("@p_ClientID_Encrypted", TMSClient.ClientID_Encrypted);
                param.Add("@p_ClientName", TMSClient.ClientName);
              //  param.Add("@p_ResponsilePerson", TMSClient.ResponsiblePerson);
                
                param.Add("@p_IsActive", TMSClient.IsActive);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_Client", param);
                var msg = param.Get<string>("@p_msg");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                //  string masterId = param.Get<string>("@p_Id");

                // FIX HERE
                string masterId = param.Get<string>("@p_Id");

                if (string.IsNullOrEmpty(masterId) || masterId == "0")
                {
                    masterId = TMSClient.ClientID.ToString();
                }


                // ---------- Save Update DETAILS ----------
                if (TMSClient.ClientDetails != null && TMSClient.ClientDetails.Count > 0)
                {
                    StringBuilder strBuilder = new StringBuilder();

                    foreach (var detail in TMSClient.ClientDetails)
                    {
                        if (detail.ClientDetailsId == null || detail.ClientDetailsId == 0)

                        {
                            string ClientDetailsId_Encrypted = Guid.NewGuid().ToString();

                            string qry = "INSERT INTO dbo.TMS_ClientDetails " +
                            "(ClientId,ClientDetailsId_Encrypted, ResponsiblePerson,  Deactivate, CreatedBy, CreatedDate, MachineName) " +
                            "VALUES ('" + masterId + "', " +
                            "'" + ClientDetailsId_Encrypted + "', " +
                            "'" + detail.ResponsiblePerson + "', " +
                            "0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";

                            strBuilder.Append(qry);

                        }

                        else
                        {
                            string qry = "UPDATE dbo.TMS_ClientDetails " +
                           "SET ResponsiblePerson = '" + detail.ResponsiblePerson + "', " +

                           "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                           "ModifiedDate = GETDATE(), " +
                           "MachineName = '" + Dns.GetHostName() + "' " +
                           "WHERE ClientDetailsId  = '" + detail.ClientDetailsId + "';";
                            strBuilder.Append(qry);

                        }
                    }


                    string abc = "";
                    if (objcon.SaveStringBuilder(strBuilder, out abc))
                    {
                        if (string.IsNullOrEmpty(TMSClient.ClientID_Encrypted))
                        {
                            TempData["Message"] = "Record save successfully";
                        }
                        else
                        {
                            TempData["Message"] = "Record update successfully";
                        }

                        TempData["Icon"] = "success";
                    }
                }

                //    bool hasUpdate = TMSClient.ClientDetails.Any(d => d.ClientDetailsId > 0);
                //    bool hasInsert = TMSClient.ClientDetails.Any(d => d.ClientDetailsId == null);

                //    if (hasInsert && !hasUpdate)
                //    {
                //        string abc = "";
                //        if (objcon.SaveStringBuilder(strBuilder, out abc))
                //        {
                //            TempData["Message"] = "Record save successfully";
                //            TempData["Icon"] = "success";
                //        }
                //    }
                //    else
                //    {
                //        string abcd = "";
                //        if (objcon.SaveStringBuilder(strBuilder, out abcd))
                //        {
                //            TempData["Message"] = "Record Update successfully";
                //            TempData["Icon"] = "success";
                //        }
                //    }

                //}


                return RedirectToAction("Setting_TMS_ClientMaster", "Setting_TMS_ClientMaster");

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 87;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_ClientID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_TMS_Client", param);
                ViewBag.GetClientMastertList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult Delete(string ClientID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 87;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_ClientID_Encrypted", ClientID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_Client", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TMS_ClientMaster");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }


        #region DeleteRow
        public ActionResult DeleteRow(int? ClientDetailsId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();

                var Result = "UPDATE dbo.TMS_ClientDetails SET Deactivate = 1 where ClientDetailsId='" + ClientDetailsId + "'";
                var Record = DapperORM.DynamicQuerySingle(Result);
                return RedirectToAction("GetList", "Setting_TMS_ClientMaster");
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