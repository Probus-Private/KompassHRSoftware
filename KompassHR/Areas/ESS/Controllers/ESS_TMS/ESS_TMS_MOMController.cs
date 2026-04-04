using Dapper;
using KompassHR.Areas.ESS.Models.ESS_TMS;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_TMS
{
    public class ESS_TMS_MOMController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        StringBuilder strBuilder = new StringBuilder();

        #region ESS_TMS_MOM Main View 
        // GET: ESS/ESS_TMS_MOM
        [HttpGet]
        public ActionResult ESS_TMS_MOM(string MOMId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
            
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 783;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
         
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = DapperORM.DynamicQuerySingle("SELECT ISNULL(MAX(CAST(DocNo AS INT)), 0) + 1 AS DocNo FROM TMS_MOM WHERE Deactivate = 0");
                    ViewBag.DocNo = GetDocNo.DocNo;
                }

                //TMS_MOM MOM = new TMS_MOM();
                DynamicParameters paramEmployee = new DynamicParameters();
                paramEmployee.Add("@query", "select Distinct EmployeeId as Id,EmployeeName as Name from Mas_Employee Where Deactivate=0 and EmployeeLeft=0 ORDER BY Name;");
                ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmployee).ToList();

                TMS_MOM MOM = new TMS_MOM();
                if (!string.IsNullOrEmpty(MOMId_Encrypted))
                {
                    DynamicParameters param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_MOMId_Encrypted", MOMId_Encrypted);
                    MOM = DapperORM.ReturnList<TMS_MOM>("sp_List_TMS_MOM", param).FirstOrDefault();
              
                        var GetDocNo = "Select DocNo from TMS_MOM where Deactivate=0 and MOMId_Encrypted='" + MOMId_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo.DocNo;
               
                    if (MOM != null)
                    {
                        DynamicParameters paramDetails = new DynamicParameters();
                        paramDetails.Add("@p_MOMId", MOM.MOMId);
                        MOM.MOMDetails = DapperORM.ReturnList<MOM_Detail>("sp_List_TMS_MOMDetails", paramDetails).ToList();
                        
                        TempData["DocDate"] = MOM.DocDate.ToString("yyyy-MM-dd");
                        TempData["Deadline"] = MOM.Deadline.ToString("yyyy-MM-dd");
                        TempData["NextMeetDate"] = MOM.NextMeetDate.ToString("yyyy-MM-dd");
                        TempData["NextMeetTime"] = MOM.NextMeetTime.ToString("HH:mm");
                        TempData["StartTime"] = MOM.StartTime.ToString("HH:mm");
                        TempData["EndTime"] = MOM.EndTime.ToString("HH:mm");
                    }
                }
                return View(MOM);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region IsMOMExists
        public ActionResult IsMOMExists(int DocNo, string MOMId_Encrypted, int MOMId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_DocNo", DocNo);
                    param.Add("@p_MOMId", MOMId);
                    param.Add("@p_MOMId_Encrypted", MOMId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_MOM", param);
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
        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(TMS_MOM MOM)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // ---------- Save/Update MASTER ----------
                DynamicParameters param = new DynamicParameters();
                // param.Add("@p_process", string.IsNullOrEmpty(MOM.MOMId_Encrypted) ? "Save" : "Update");
                string processType = (MOM.MOMId > 0 && !string.IsNullOrEmpty(MOM.MOMId_Encrypted)) ? "Update" : "Save";
                param.Add("@p_process", processType);
                param.Add("@p_MOMId_Encrypted", MOM.MOMId_Encrypted);
                param.Add("@p_MOMId", MOM.MOMId);
                param.Add("@p_DocDate", MOM.DocDate);
                param.Add("@p_DocNo", MOM.DocNo);
                param.Add("@p_StartTime", MOM.StartTime);
                param.Add("@p_EndTime", MOM.EndTime);
                param.Add("@p_Title", MOM.Title);
                param.Add("@p_Owner", Session["EmployeeId"]);
                param.Add("@p_Agenda", MOM.Agenda);
                param.Add("@p_NextMeeting", MOM.NextMeeting);
               // param.Add("@p_NextMeetDate", MOM.NextMeetDate);
               // param.Add("@p_NextMeetTime", MOM.NextMeetTime);
                if (MOM.NextMeetDate != DateTime.MinValue)
                {
                    param.Add("@p_NextMeetDate", MOM.NextMeetDate);
                }
                else
                {
                    param.Add("@p_NextMeetDate", " ");
                }

                if (MOM.NextMeetTime != DateTime.MinValue)
                {
                    param.Add("@p_NextMeetTime", MOM.NextMeetTime);
                }
                else
                {
                    param.Add("@p_NextMeetTime", " ");
                }
                param.Add("@p_NextMeetAgenda", MOM.NextMeetAgenda);
                param.Add("@p_Remark", MOM.Remark);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_MOM", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                string masterId = param.Get<string>("@p_Id");
              //  string detailEncryptedId = Guid.NewGuid().ToString();

                // ---------- Save Update DETAILS ----------
                if (MOM.MOMDetails != null && MOM.MOMDetails.Count > 0)
                {
                    StringBuilder strBuilder = new StringBuilder();

                    foreach (var detail in MOM.MOMDetails)
                    {
                 
                       if (detail.MOMDetailsId == 0)
                      
                        {
                            string detailEncryptedId = Guid.NewGuid().ToString();

                            string qry = "INSERT INTO dbo.TMS_MOMDetails " +
                            "(MOMId,MOMDetailsId_Encrypted, ActionItem, Priority, Employee, Deadline, Deactivate, CreatedBy, CreatedDate, MachineName) " +
                            "VALUES ('" + masterId + "', " +
                            "'" + detailEncryptedId + "', " +
                            "'" + detail.ActionItem + "', " +
                            "'" + detail.Priority + "', " +
                            "'" + detail.Employee + "', " +
                            "'" + detail.Deadline.ToString("yyyy-MM-dd") + "', " +
                            "0, '" + Session["EmployeeName"] + "', GETDATE(), '" + Dns.GetHostName() + "');";

                            strBuilder.Append(qry);
                            
                        }

                        else
                        {
                            string qry = "UPDATE dbo.TMS_MOMDetails " +
                           "SET ActionItem = '" + detail.ActionItem + "', " +
                           "Priority = '" + detail.Priority + "', " +
                           "Employee = '" + detail.Employee + "', " +
                           "Deadline = '" + detail.Deadline.ToString("yyyy-MM-dd") + "', " +
                           "ModifiedBy = '" + Session["EmployeeName"] + "', " +
                           "ModifiedDate = GETDATE(), " +
                           "MachineName = '" + Dns.GetHostName() + "' " +
                           "WHERE MOMDetailsId  = '" + detail.MOMDetailsId + "';";
                            strBuilder.Append(qry);

                        }
                    }
                    //string abc = "";
                    //if (objcon.SaveStringBuilder(strBuilder, out abc))
                    //{
                    //    TempData["Message"] = "Record save successfully";
                    //    TempData["Icon"] = "success";
                    //}

                    
                   bool hasUpdate = MOM.MOMDetails.Any(d => d.MOMDetailsId > 0);
                   bool   hasInsert = MOM.MOMDetails.Any(d => d.MOMDetailsId == 0);

                    if (hasInsert && !hasUpdate)
                    {
                        string abc = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abc))
                        {
                            TempData["Message"] = "Record save successfully";
                            TempData["Icon"] = "success";
                        }
                    }
                    else 
                    {
                        string abcd = "";
                        if (objcon.SaveStringBuilder(strBuilder, out abcd))
                        {
                            TempData["Message"] = "Record Update successfully";
                            TempData["Icon"] = "success";
                        }
                    }
                    
                }
                return RedirectToAction("GetList", "ESS_TMS_MOM");
            }
            
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
        
        #region GetList Main View 
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 783;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_MOMId_Encrypted", "List");
                param.Add("@p_EmployeeId", Session["EmployeeId"]);

                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TMS_MOM", param).ToList();
                ViewBag.GetMOMList = data;
                
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
        public ActionResult DeleteMOM(int? MOMId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_MOMId", MOMId);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"].ToString());
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var Result = DapperORM.ExecuteReturn("sp_SUD_TMS_MOM", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                // TempData["P_Id"] = param.Get<string>("@p_Id");
                return RedirectToAction("GetList", "ESS_TMS_MOM");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Show Employee List
        public ActionResult ShowEmployeeList(int MOMId)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            DynamicParameters param = new DynamicParameters();
            param.Add("@p_MOMId", MOMId);
            var data = DapperORM.ReturnList<dynamic>("sp_List_TMS_MOMDetails", param).ToList();
            //string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            //return Json(jsonData, JsonRequestBehavior.AllowGet);
            var employees = data.Select(d => new {
                EmployeeName = d.EmployeeName,
                ActionItem = d.ActionItem,
                Priority = d.Priority,
                Deadline = d.Deadline != null
                      ? Convert.ToDateTime(d.Deadline).ToString("dd/MMM/yyyy").ToLower()
                     : ""
            });
            return Json(employees, JsonRequestBehavior.AllowGet);

        }
        #endregion
        
        #region DeleteRow
        public ActionResult Delete(int? MOMDetailsId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "ESS" });
                }
                DynamicParameters param = new DynamicParameters();

                var Result = "UPDATE dbo.TMS_MOMDetails SET Deactivate = 1 where MOMDetailsId='" + MOMDetailsId + "'";
                var Record = DapperORM.DynamicQuerySingle(Result);
                return RedirectToAction("GetList", "ESS_TMS_MOM");
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