using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Assets;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Assets
{
    public class ESS_Assets_AssetStatusController : Controller
    {
        DynamicParameters param = new DynamicParameters();

             #region Main view 
        // GET: ESS/ESS_Assets_AssetStatus
        public ActionResult ESS_Assets_AssetStatus(Mas_Employee_Asset EmployeeAsset)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 839;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

          
                 DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();
                ViewBag.CompanyName = GetComapnyName;
                var CmpId = GetComapnyName[0].Id;
                

                if (EmployeeAsset.CompanyId != 0)
                {
                    DynamicParameters paramBranch = new DynamicParameters();
                    paramBranch.Add("@query", "select Distinct BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0 AND CmpId='" + EmployeeAsset.CompanyId + "';");
                    var branchList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();
                    ViewBag.GetMasBranch = branchList;

                }
                else
                {
                    ViewBag.GetMasBranch = new List<AllDropDownBind>();
                }

                if (EmployeeAsset.CompanyId != 0 && EmployeeAsset.BranchId != null)
                {

                    DynamicParameters paramEmp = new DynamicParameters();
                    paramEmp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + EmployeeAsset.CompanyId + "' and EmployeeId<>1 and EmployeeBranchId=" + EmployeeAsset.BranchId + " and EmployeeLeft=0 order by Name");
                    var EmployeeList = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    ViewBag.GetEmployeeList = EmployeeList;
                }
             
                else
                {
                    ViewBag.GetEmployeeList = new List<AllDropDownBind>();
                }

                if (EmployeeAsset.CompanyId != 0)
                {
                    DynamicParameters paramList = new DynamicParameters();
                     paramList.Add("@p_AssetEmployeeId", EmployeeAsset.AssetEmployeeId);
                    paramList.Add("@p_CompanyId", EmployeeAsset.CompanyId);
                    paramList.Add("@p_BranchId", EmployeeAsset.BranchId);
                    
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Assets_AssetStatusList", paramList).ToList();
                    ViewBag.GetAssetList = GetData;
                    
                    if (GetData.Count > 0)
                    {
                        ViewBag.GetAssetList = GetData;
                    }
                    else
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                        //return View(EmployeeAsset);

                    }

                }
                else
                {
                    DynamicParameters paramList = new DynamicParameters();
                    paramList.Add("@p_AssetEmployeeId", EmployeeAsset.AssetEmployeeId);
                    paramList.Add("@p_CompanyId", EmployeeAsset.CompanyId);
                    paramList.Add("@p_BranchId", EmployeeAsset.BranchId);
                    var GetData = DapperORM.ExecuteSP<dynamic>("sp_Assets_AssetStatusList", paramList).ToList();
                    ViewBag.GetAssetList = GetData;
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


    
        
        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int? CompanyId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters param = new DynamicParameters();

                DynamicParameters paramBranchName = new DynamicParameters();
                paramBranchName.Add("@query", "select BranchId as Id, BranchName as Name  from Mas_Branch where Deactivate = 0 and CmpId ='" + CompanyId + "'  order by Name");
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranchName).ToList();
                ViewBag.GetMasBranch = Branch;

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "'  order by Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();
                ViewBag.GetEmployeeList = EmployeeName;

                return Json(new { Branch = Branch, EmployeeName = EmployeeName }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int? CompanyId, int? BranchId)
        {
            try
            {

                DynamicParameters param = new DynamicParameters();
                //if (BranchId != null)
                //{
                //    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "' AND EmployeeBranchId='" + BranchId + "' order by Name");
                //}
                //else
                //{
                //    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate = 0 and EmployeeLeft=0 AND CmpID='" + CompanyId + "' order by Name");

                //}
                if (BranchId == 0 || BranchId == null)
                {
                    DynamicParameters paramEmp = new DynamicParameters();
                    param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + CompanyId + "' and EmployeeId<>1  and EmployeeLeft=0 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DynamicParameters paramEmp = new DynamicParameters();
                    paramEmp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID ='" + CompanyId + "' and EmployeeId<>1 and EmployeeBranchId=" + BranchId + " and EmployeeLeft=0 order by Name");
                    var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmp).ToList();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region  ViewAsset
        public ActionResult ViewAsset(int? EmployeeId,string AssetId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_AssetId_Encrypted", AssetId_Encrypted);
             //   param.Add("@p_AssetAllocationDate", AssetAllocationDate);

                // param.Add("@p_Origin", "ApprovalRequest");
                var Asset = DapperORM.ExecuteSP<dynamic>("sp_Assets_ViewStatusList", param).FirstOrDefault();
                if (Asset != null)
                {
                    TempData["ReturnDate"] = (Asset.ReturnDate != null && Asset.ReturnDate != DateTime.MinValue)
                                 ? Asset.ReturnDate.ToString("yyyy-MM-dd")
                                 : string.Empty;

                    ViewBag.AssetList1 = Asset;
                    ViewBag.ReturnStatus = Asset.ReturnStatus;
                }
                else
                {
                    ViewBag.AssetList1 = "";
                    ViewBag.ReturnStatus = false;
                }


                return View();
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

                // var EmployeeId = Session["EmployeeId"];
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
                param.Add("@p_ReturnDate", EmployeeAsset.ReturnDate);
                param.Add("@p_ReturnRemark", EmployeeAsset.ReturnRemark);
                param.Add("@p_ReturnStatus", EmployeeAsset.ReturnStatus);
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
                return RedirectToAction("ESS_Assets_AssetStatus", "ESS_Assets_AssetStatus");
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