using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
using KompassHR.Areas.Setting.Models.Setting_LMS;
using System.Net;
using System.Data;
using System.Text;

namespace KompassHR.Areas.Setting.Controllers.Setting_LMS
{
    public class Setting_LMS_CategoryGroupController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        // GET: Setting/Setting_LMS_CategoryGroup
        public ActionResult Setting_LMS_CategoryGroup(string CategoryGroupMasterId_Encrypted, int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 569;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }


                DynamicParameters param = new DynamicParameters();
                LMS_Category_GroupMaster LMS_Category_GroupMaster = new LMS_Category_GroupMaster();

                param.Add("@query", "Select LMSCategoryId as Id, LMSCategoryName as Name from LMS_Category where Deactivate=0 order by Name");
                var GetCategory = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                ViewBag.CategoryName = GetCategory;


                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var GetCompanyId = GetComapnyName[0].Id;
               
                ViewBag.AddUpdateTitle = "Add";
                if (CompanyId != null)
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", CompanyId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }
                else
                {
                    DynamicParameters ParamBranch = new DynamicParameters();
                    ParamBranch.Add("@p_employeeid", Session["EmployeeId"]);
                    ParamBranch.Add("@p_CmpId", GetCompanyId);
                    ViewBag.Location = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", ParamBranch).ToList();
                }

                if (CategoryGroupMasterId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var Data = DapperORM.DynamicQuerySingle($@"Select CategoryGroupMasterId , CategoryGroupName, CategoryGroupCompanyId, CategoryGroupBranchId from LMS_Category_GroupMaster where Deactivate = 0 and CategoryGroupMasterId_Encrypted='{CategoryGroupMasterId_Encrypted}'");
                    //var GetDocRead = Data.DownloadUploadURL;
                    LMS_Category_GroupMaster.CategoryGroupCompanyId = Data.CategoryGroupCompanyId;
                    LMS_Category_GroupMaster.CategoryGroupBranchId = Data.CategoryGroupBranchId;
                    LMS_Category_GroupMaster.LMSCategoryGroupName = Data.CategoryGroupName;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", $@" SELECT LMS_Category.LMSCategoryId as Id, LMS_Category.LMSCategoryName as Name, LMS_Category_GroupDetails.IsActivate as IsActive
                        FROM  LMS_Category 
                        LEFT JOIN LMS_Category_GroupDetails   ON LMS_Category.LMSCategoryId = LMS_Category_GroupDetails.CategoryId  AND LMS_Category_GroupDetails.CategoryGroupMasterId = {Data.CategoryGroupMasterId} order by Name");
                    ViewBag.CategoryName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param1).ToList();

                }
                return View(LMS_Category_GroupMaster);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsValidation
        [HttpPost]
        public ActionResult IsExists(List<LMS_CategoryIds> Task,int CompanyId,int BranchId,string CategoryName, string CategoryGroupMasterId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_CategoryGroupMasterId_Encrypted", CategoryGroupMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CategoryGroupCompanyId", CompanyId);
                param.Add("@p_CategoryGroupBranchId", BranchId);
                param.Add("@p_CategoryGroupName", CategoryName);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_CategoryGroupMaster", param);

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
        public ActionResult SaveUpdate(List<LMS_CategoryIds> Task, int CompanyId, int BranchId, string CategoryName, string CategoryGroupMasterId_Encrypted)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                param.Add("@p_process", string.IsNullOrEmpty(CategoryGroupMasterId_Encrypted) ? "Save" : "Update");
                param.Add("@p_CategoryGroupMasterId_Encrypted", CategoryGroupMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());

                param.Add("@p_CategoryGroupCompanyId", CompanyId);
                param.Add("@p_CategoryGroupBranchId", BranchId);
                param.Add("@p_CategoryGroupName", CategoryName);

                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_LMS_CategoryGroupMaster", param);
                var PID = param.Get<string>("@p_Id");

                //THIS IS UPDATEMODEL CASE
                if (CategoryGroupMasterId_Encrypted != "")
                {
                    //DapperORM.DynamicQuerySingle("Update LMS_Category_GroupDetails set Deactivate=1,ModifiedBy='" + Session["EmployeeName"] + "',ModifiedDate=GETDATE(),MachineName='" + Dns.GetHostName().ToString() + "' where CategoryGroupDetailId=" + PID + "");
                    DapperORM.DynamicQuerySingle("DELETE FROM LMS_Category_GroupDetails WHERE CategoryGroupMasterId = " + PID + "");
                    if (Task != null)
                    {
                        foreach (var Data in Task)
                        {
                            string Answer = "Insert Into LMS_Category_GroupDetails(" +
                                                  "   Deactivate " +
                                                  " , CreatedBy " +
                                                  " , CreatedDate " +
                                                  " , MachineName " +
                                                  " , CategoryGroupMasterId " +
                                                  " , CategoryId " +
                                                  " , IsActivate " +
                                                   ") values (" +
                                                  "'0'," + "'" + Session["EmployeeName"] + "'," + "Getdate()," + "'" + Dns.GetHostName().ToString() + "'," +
                                                  "'" + PID + "'," +
                                                  "'" + Data.CategoryId + "'," +
                                                  "'" + Data.IsActive + "'" +
                                                  ")" +
                                                   " " +
                                                  " " +
                                                   " ";
                            strBuilder.Append(Answer);

                        }
                        // Append your update query AFTER all inserts
                        string updateQuery = "UPDATE LMS_Category_GroupDetails " +
                                             "SET CategoryGroupDetailId_Encrypted = " +
                                             "(master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(nvarchar(70), CategoryGroupDetailId)))) " +
                                             "WHERE CategoryGroupDetailId = CategoryGroupDetailId;"; // 👈 This condition updates all rows (can be improved!)

                        strBuilder.Append(updateQuery);

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
                                                                                "'LMS_Category_GroupDetails'," +
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
                    if (Task != null)
                    {
                        foreach (var Data in Task)
                        {
                            string Answer = "INSERT INTO LMS_Category_GroupDetails(" +
                                            "   Deactivate, " +
                                            "   CreatedBy, " +
                                            "   CreatedDate, " +
                                            "   MachineName, " +
                                            "   CategoryGroupMasterId, " +
                                            "   CategoryId, " +
                                            "   IsActivate " +
                                            ") VALUES (" +
                                            "'0'," +
                                            "'" + Session["EmployeeName"] + "'," +
                                            "GETDATE()," +
                                            "'" + Dns.GetHostName().ToString() + "'," +
                                            "'" + PID + "'," +
                                            "'" + Data.CategoryId + "'," +
                                            "'" + Data.IsActive + "'" +
                                            "); "; 
                            strBuilder.Append(Answer);
                        }

                        // Append your update query AFTER all inserts
                        string updateQuery = "UPDATE LMS_Category_GroupDetails " +
                                             "SET CategoryGroupDetailId_Encrypted = " +
                                             "(master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(nvarchar(70), CategoryGroupDetailId)))) " +
                                             "WHERE CategoryGroupDetailId = CategoryGroupDetailId;"; // 👈 This condition updates all rows (can be improved!)

                        strBuilder.Append(updateQuery);

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
                                                                                "'LMS_Category_GroupDetails'," +
                                                                                "'" + Dns.GetHostName().ToString() + "'," +
                                                                                "GetDate()," +
                                                                                "'" + Session["EmployeeId"] + "'," +
                                                                                "'" + Session["EmployeeName"] + "'");
                            TempData["Message"] = abc;
                            TempData["Icon"] = "error";
                        }
                    }

                }

                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);

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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 569;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                param.Add("@p_CategoryGroupMasterId_Encrypted", "List");
                var data = DapperORM.ReturnList<dynamic>("sp_List_LMS_CategoryGroupMaster", param).ToList();
                ViewBag.CategoryGroupMaster = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region  Delete
        public ActionResult Delete(string CategoryGroupMasterId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_CategoryGroupMasterId_Encrypted", CategoryGroupMasterId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_LMS_CategoryGroupMaster", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_LMS_CategoryGroup");
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