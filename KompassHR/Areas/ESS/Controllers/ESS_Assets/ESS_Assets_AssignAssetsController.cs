using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Assets;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Assets
{
    public class ESS_Assets_AssignAssetsController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        #region ESS_Assets_AssignAssets Main View 
        // GET: ESS/ESS_Assets_AssignAssets
        public ActionResult ESS_Assets_AssignAssets(string AssetId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 838;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Mas_Employee_Asset EmployeeAsset = new Mas_Employee_Asset();

                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                //var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                //ViewBag.CompanyName = GetComapnyName;
                //var CmpId = GetComapnyName[0].Id;

                //ViewBag.GetMasBranch = "";
                // ViewBag.GetEmployeeList = "";
                DynamicParameters Param = new DynamicParameters();
                Param.Add("@query", "SELECT EmployeeId AS Id, CONCAT(EmployeeName, ' - ', EmployeeNo) AS Name " + "FROM Mas_Employee WHERE Deactivate = 0 AND EmployeeLeft = 0  ORDER BY Name");
                var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", Param).ToList();
                ViewBag.GetEmployeeList = EmployeeList;


                param.Add("@query", "select AssetId as Id,AssetName  as [Name] from Mas_Asset where Deactivate=0");
                var AssetName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetAssetName = AssetName;

                param.Add("@query", "select JoiningDate,EmployeeNo,EmployeeName,DepartmentName,DesignationName from Mas_Employee INNER JOIN Mas_Department ON Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId INNER JOIN Mas_Designation ON Mas_Employee.EmployeeDesignationID = Mas_Designation.DesignationId where EmployeeId='"+ Session["EmployeeId"] + "'");
                var EmployeeInfo = DapperORM.ReturnList<Mas_EmployeeInfo>("sp_QueryExcution", param).FirstOrDefault();
                ViewBag.GetEmployeeInfo = EmployeeInfo;

                  param.Add("@query", @"SELECT Mas_Employee_Asset.AssetId,Mas_Employee_Asset.AssetId_Encrypted,
                  Mas_Employee_Asset.CreatedBy,Mas_Employee_Asset.CreatedDate,Mas_Employee_Asset.MachineName,
                 Mas_Employee_Asset.AssetEmployeeId,REPLACE(CONVERT(NVARCHAR(12), Mas_Employee_Asset.Date, 106), ' ', '/') AS Date,
                 Mas_Asset.AssetName,Mas_Employee_Asset.Remark,Mas_Employee_Asset.AssetRemark,
                 Mas_Employee_Asset.RequiredAlert,CASE WHEN RequiredAlert = 1 THEN 'Yes' ELSE 'No' END AS RequiredAlertText,
                 REPLACE(CONVERT(NVARCHAR(12), Mas_Employee_Asset.RequiredAlertDate, 106), ' ', '/') AS RequiredAlertDate FROM Mas_Employee_Asset INNER JOIN Mas_Asset ON Mas_Asset.AssetId = Mas_Employee_Asset.AssetName
                 WHERE Mas_Employee_Asset.Deactivate = 0 AND Mas_Employee_Asset.AssetEmployeeId = '" + Session["EmployeeId"] + "'");
                var data = DapperORM.ReturnList<Mas_Employee_Asset>("sp_QueryExcution", param).ToList();
                ViewBag.GetAssetList1 = data;

                if (AssetId_Encrypted != null)
                {
                    // ---------------- UPDATE ----------------
                    ViewBag.AddUpdateTitle = "Update";
                    var updateParam = new DynamicParameters();
                    updateParam.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
                    EmployeeAsset = DapperORM.ReturnList<Mas_Employee_Asset>("sp_List_Mas_Employee_AssignAsset", updateParam).FirstOrDefault();

                    //DynamicParameters branchParam = new DynamicParameters();
                    //branchParam.Add("@query", "SELECT BranchId AS Id, BranchName AS Name FROM Mas_Branch WHERE Deactivate = 0 AND CmpId = '"+ EmployeeAsset.CompanyId + "' ORDER BY Name");

                    //var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", branchParam).ToList();
                    //ViewBag.GetMasBranch = branchList;

                    DynamicParameters AssetParam = new DynamicParameters();
                    AssetParam.Add("@query", "select AssetId as Id,AssetName as [Name] from Mas_Asset where Deactivate=0");
                    var Asset = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", AssetParam).ToList();
                    ViewBag.GetAssetName = Asset;


                    DynamicParameters empParam = new DynamicParameters();
                    empParam.Add("@query","SELECT EmployeeId AS Id, CONCAT(EmployeeName, ' - ', EmployeeNo) AS Name " + "FROM Mas_Employee WHERE Deactivate = 0 AND EmployeeLeft = 0  ORDER BY Name");
                    var empList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", empParam).ToList();
                    ViewBag.GetEmployeeList = empList;


                    TempData["Date"] = EmployeeAsset.Date.HasValue ? EmployeeAsset.Date.Value.ToString("yyyy-MM-dd"): string.Empty;

                    TempData["RequiredAlertDate"] = EmployeeAsset.RequiredAlertDate.HasValue ? EmployeeAsset.RequiredAlertDate.Value.ToString("yyyy-MM-dd"): string.Empty;
                }

                return View(EmployeeAsset);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        //#region GetBusinessUnit
        //[HttpGet]
        //public ActionResult GetBusinessUnit(int CompanyId)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { area = "" });
        //        }

        //        DynamicParameters param = new DynamicParameters();

        //        DynamicParameters paramBranchName = new DynamicParameters();
        //        paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CompanyId + "'  order by Name");
        //        var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
        //        ViewBag.GetMasBranch = Branch;

        //        DynamicParameters paramEmpName = new DynamicParameters();
        //        paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "'  order by Name");
        //        var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
        //        ViewBag.GetEmployeeList = EmployeeName;

        //        return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}
        //#endregion

        //#region GetEmployee
        //[HttpGet]
        //public ActionResult GetEmployee(int? CompanyId, int? BranchId)
        //{
        //    try
        //    {

        //        DynamicParameters param = new DynamicParameters();
        //        if (BranchId != null)
        //        {
        //            param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "' AND EmployeeBranchId='" + BranchId + "' order by Name");
        //        }
        //        else
        //        {
        //            param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "' order by Name");

        //        }
        //        var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
        //        ViewBag.GetEmployeeList = data;

        //        return Json(data, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }

        //}
        //#endregion
        
        #region IsValidation
        [HttpGet]
        public ActionResult IsAssetExists(string AssetEmployeeId, string AssetName, string AssetId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
                param.Add("@p_AssetEmployeeId", AssetEmployeeId);
                param.Add("@p_AssetName", AssetName);
               // param.Add("@p_ComapnyId", CompanyId);
               // param.Add("@p_BranchId", BranchId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_AssignAsset", param);
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
        public ActionResult SaveUpdate(Mas_Employee_Asset EmployeeAsset)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(EmployeeAsset.AssetId_Encrypted) ? "Save" : "Update");
                param.Add("@p_AssetId", EmployeeAsset.AssetId);
                param.Add("@p_AssetId_Encrypted", EmployeeAsset.AssetId_Encrypted);
               // param.Add("@p_ComapnyId", EmployeeAsset.CompanyId);
                //param.Add("@p_BranchId", EmployeeAsset.BranchId);
                param.Add("@p_AssetEmployeeId", EmployeeAsset.AssetEmployeeId);
            
                param.Add("@p_AssetName", EmployeeAsset.AssetName);
                param.Add("@p_Remark", EmployeeAsset.Remark);
                param.Add("@p_RequiredAlert", EmployeeAsset.RequiredAlert);
                param.Add("@p_RequiredAlertDate", EmployeeAsset.RequiredAlertDate);
                param.Add("@p_AssetRemark", EmployeeAsset.AssetRemark);
                param.Add("@p_Date", EmployeeAsset.Date);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_AssignAsset", param);
                
                //   var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_AssignAsset]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("ESS_Assets_AssignAssets", "ESS_Assets_AssignAssets");
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 838;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ESS" });
                }
                 param.Add("@p_AssetId_Encrypted", "List");
                 //param.Add("@p_AssetEmployeeId", Session["EmployeeId"]);

                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_AssignAsset", param).ToList();
               // var data = DapperORM.ExecuteSP<dynamic>("sp_List_Mas_Employee_Asset", param).ToList();

                ViewBag.GetAssetList = data;

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
        public ActionResult Delete(string AssetId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_AssignAsset", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("ESS_Assets_AssignAssets", "ESS_Assets_AssignAssets");
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