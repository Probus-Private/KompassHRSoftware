using Dapper;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;
using KompassHR.Areas.Setting.Models.Setting_Prime;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_PolicyAndLibrary
{
    public class Setting_PolicyAndLibrary_PolicyGroupController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: PrimeSetting/PolicyGroup
        public ActionResult Setting_PolicyAndLibrary_PolicyGroup(string PolicyGroupMasterId_Encrypted,int? PolicyGroupMasterId, int? CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var GetCompanyName = DapperORM.ReturnList<Setting.Models.Setting_PolicyAndLibrary.Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;

                param = new DynamicParameters();
                if (CmpID != null)
                {
                    param.Add("@query", "Select  BranchId, BranchName from Mas_Branch where Deactivate=0 and CmpId=" + CmpID + "");
                    var GetsLocation = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", param).ToList();
                    ViewBag.Location = GetsLocation;
                }
                else
                {
                    ViewBag.Location = "";
                }
                Mas_PolicyGroup_Master MasPolicyGroupMaster = new Mas_PolicyGroup_Master();
                if (PolicyGroupMasterId_Encrypted!=null && PolicyGroupMasterId!= null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";                   
                    param1.Add("@p_PolicyGroupMasterId_Encrypted", PolicyGroupMasterId_Encrypted);
                    MasPolicyGroupMaster = DapperORM.ReturnList<Mas_PolicyGroup_Master>("sp_List_Mas_PolicyGroup_Master", param1).FirstOrDefault();
                    var PolicyGroupBranchId = MasPolicyGroupMaster.PolicyGroupBranchId;
                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", @"  Select Mas_PolicyLibrary.PolicyLibraryId,case when PolicyGroupPolicyLibraryId IS null  then  'unchecked' else 'checked'  end as checkbox,
                                        Mas_Policy.PolicyName , Mas_PolicyLibrary.remark,Mas_PolicyLibrary.DocumentPath  from Mas_PolicyLibrary
	                                    Inner join Mas_Policy On Mas_PolicyLibrary.PolicyLibraryPolicyId = Mas_Policy.PolicyId
	                                    left join Mas_PolicyGroup_Detail on PolicyGroupPolicyLibraryId=Mas_PolicyLibrary.PolicyLibraryId  and DetailPolicyGroupMasterId="+ PolicyGroupMasterId + " where Mas_PolicyLibrary.deactivate=0 and Mas_Policy.deactivate=0 and Mas_PolicyLibrary.PolicyLibraryBranchId= "+PolicyGroupBranchId+"");
                    ViewBag.SelectedShiftList = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param2).ToList();
                }
                return View(MasPolicyGroupMaster);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

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

        [HttpGet]
        public ActionResult GetPolicyGroup(int LocationId)
        {
            try
            {
                DynamicParameters Group = new DynamicParameters();
                Group.Add("@p_PolicyLibraryBranchId", LocationId);
                var GetList = DapperORM.ReturnList<Models.Setting_PolicyAndLibrary.GetGroupList>("sp_GetPolicyGroup_List", Group).ToList();
                var data = new { GetList };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult IsPolicyLaibraryExist(string CompanyId, string LocationId, string GroupShiftName, string PolicyGroupMasterIdEncrypted, List<PolicyGroup>lstPolicyGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                List<int> Arraylist = new List<int>() { };
                string result = "";
                for (var i = 0; i < lstPolicyGroup.Count; i++)
                {
                    result = result + lstPolicyGroup[i].PolicyLibraryId + ",";
                }
                result = result.TrimEnd(',');
                Session["lstShiftGroup"] = result;
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PolicyGroupMasterId_Encrypted", PolicyGroupMasterIdEncrypted);
                    param.Add("@p_CmpID", CompanyId);
                    param.Add("@p_PolicyGroupBranchId", LocationId);
                    param.Add("@p_GroupName", GroupShiftName);
                    param.Add("@p_PolicyGroupMasterIds", result);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyGroup_Master", param);
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

        [HttpPost]
         public ActionResult SaveUpdate(Mas_PolicyGroup_Master PolicyGroup)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var PolicyGrouplist = Session["lstShiftGroup"];
                param.Add("@p_process", string.IsNullOrEmpty(PolicyGroup.PolicyGroupMasterId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PolicyGroupMasterId_Encrypted", PolicyGroup.PolicyGroupMasterId_Encrypted);
                param.Add("@p_CmpID", PolicyGroup.CmpID);
                param.Add("@p_PolicyGroupBranchId", PolicyGroup.PolicyGroupBranchId);
                param.Add("@p_GroupName", PolicyGroup.GroupName);
                param.Add("@p_PolicyGroupMasterIds", PolicyGrouplist);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyGroup_Master", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("Setting_PolicyAndLibrary_PolicyGroup", "Setting_PolicyAndLibrary_PolicyGroup");
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
                param.Add("@p_PolicyGroupMasterId_Encrypted", "List");              
                var PolicyGroup = DapperORM.DynamicList("sp_List_Mas_PolicyGroup_Master", param);                
                ViewBag.GetPolicyGroupList = PolicyGroup;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string PolicyGroupMasterId_Encrypted ,string PolicyGroupMasterId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                param.Add("@p_process", "Delete");
                param.Add("@p_PolicyGroupMasterId", PolicyGroupMasterId);
                param.Add("@p_PolicyGroupMasterId_Encrypted", PolicyGroupMasterId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyGroup_Master", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyGroup");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }

}