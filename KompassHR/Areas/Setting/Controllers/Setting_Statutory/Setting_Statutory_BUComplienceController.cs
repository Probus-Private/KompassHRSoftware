using Dapper;
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
namespace KompassHR.Areas.Setting.Controllers.Setting_Statutory
{
    public class Setting_Statutory_BUComplienceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Setting/Setting_Statutory_BUComplience
        public ActionResult Setting_Statutory_BUComplience(string BUComplienceId_Encrypted)
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
                //Mas_Employee_Statutory EmployeeStatutory = new Mas_Employee_Statutory();
                ViewBag.AddUpdateTitle = "Add";
                var ResultList = DapperORM.DynamicQueryMultiple(@" Select PTCodeId as Id,PTRemark as Name from Payroll_PTCode Where Deactivate=0 and CmpID=" + Session["CompanyId"] + " order By Name"
                                                        + " Select LWFCodeId as Id,LWFRemark as Name from Payroll_LWFCode Where Deactivate=0 and CmpID=" + Session["CompanyId"] + " order By Name"
                                                        + " Select ESICCodeId as Id,ESICRemark as Name from Payroll_ESICCode Where Deactivate=0 and  CmpID=" + Session["CompanyId"] + " order By Name"
                                                        + " Select PFCodeId as Id,PFRemark as Name from Payroll_PFCode Where Deactivate=0 and  CmpId=" + Session["CompanyId"] + " order By Name"
                                                        + " Select PTSlabMasterId as Id,Remark as Name from Payroll_PTSlab_Master Where Deactivate=0 order By Name;"
                                                        + " Select LWFSlabMasterId as Id,Remark as Name from Payroll_LWFSlab_Master Where Deactivate=0 order By Name;"
                                                        + " Select PFWagesMasterId as Id,PFWagesRemark as Name from Payroll_PFWages_Master Where Deactivate=0 and  CmpId=" + Session["CompanyId"] + " order By Name"
                                                        + " Select RelationId as Id,RelationName as Name from Mas_Relation Where Deactivate=0 order By Name");
                ViewBag.GetPTStateCode = ResultList[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFCode = ResultList[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetESICCode = ResultList[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFCode = ResultList[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPTSlabMaster = ResultList[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFSlabMaster = ResultList[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFWageMaster = ResultList[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetRelationName = ResultList[7].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                DynamicParameters paramList = new DynamicParameters();
                Mas_Employee_Statutory objMas_Employee_Statutory = new Mas_Employee_Statutory();
                //paramList.Add("@p_StatutoryEmployeeId", Session["OnboardEmployeeId"]);
                //EmployeeStatutory = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_List_Mas_Employee_Statutory", paramList).FirstOrDefault();
                if (BUComplienceId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";

                    param = new DynamicParameters();
                    param.Add("@p_BUComplienceId_Encrypted", BUComplienceId_Encrypted);
                    objMas_Employee_Statutory = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_List_Payroll_BUComplience_Setting", param).FirstOrDefault();

                    var BranchName = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(objMas_Employee_Statutory.CmpID), Convert.ToInt32(Session["EmployeeId"]));
                    ViewBag.BranchName = BranchName;
                }
                ViewBag.PFApplicable = objMas_Employee_Statutory;

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                var CMPID = GetComapnyName[0].Id;
                //GET BRANCH NAME
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                ViewBag.BranchName = Branch;

                DynamicParameters param11 = new DynamicParameters();
                param11.Add("@query", "select StateId as Id,StateName as Name  from Mas_States where Deactivate=0 order by Name");
                var GetState = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param11).ToList();
                ViewBag.GetState = GetState;

                return View(objMas_Employee_Statutory);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_Employee_Statutory Statutory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Statutory.BUComplienceId_Encrypted) ? "Save" : "Update");
                param.Add("@p_BUComplienceId", Statutory.BUComplienceId);
                param.Add("@p_BUComplienceId_Encrypted", Statutory.BUComplienceId_Encrypted);
                param.Add("@p_CmpID", Statutory.CmpID);
                param.Add("@p_BranchID", Statutory.BranchID);
                param.Add("@p_BU_StateID", Statutory.BU_StateID);
                param.Add("@p_ESIC_Applicable", Statutory.ESIC_Applicable);
                param.Add("@p_ESIC_CodeId", Statutory.ESIC_CodeId);
                param.Add("@p_PT_Applicable", Statutory.PT_Applicable);
                param.Add("@p_PT_CodeId", Statutory.PT_CodeId);
                param.Add("@p_LWF_Applicable", Statutory.LWF_Applicable);
                param.Add("@p_LWF_CodeId", Statutory.LWF_CodeId);
                param.Add("@p_PF_Applicable", Statutory.PF_Applicable);
                param.Add("@p_PF_CodeId", Statutory.PF_CodeId);
                param.Add("@p_PF_Limit", Statutory.PF_Limit);
                param.Add("@p_PTSlab_MasterId", Statutory.PTSlab_MasterId);
                param.Add("@p_LWFSlab_MasterId", Statutory.LWFSlab_MasterId);
                param.Add("@p_PFWages_MasterId", Statutory.PFWages_MasterId);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_SUD_Payroll_BUComplience_Setting", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Setting_Statutory_BUComplience", "Setting_Statutory_BUComplience");
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
        public ActionResult IsBUExists(int CmpID, int BranchID, string BUComplienceId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_BUComplienceId_Encrypted", BUComplienceId_Encrypted);
                param.Add("@p_CmpID", CmpID);
                param.Add("@p_BranchID", BranchID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_BUComplience_Setting", param);
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


        #region Get Branch Name
        [HttpGet]
        public ActionResult GetMonthlyBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));

                DynamicParameters ParamESICCode = new DynamicParameters();
                ParamESICCode.Add("@query", "Select ESICCodeId as Id,ESICRemark as Name from Payroll_ESICCode Where Deactivate=0 and  CmpID=" + CmpId + "");
                var ESICCode = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamESICCode).ToList();
       
                return Json(new { Branch = Branch , ESICCode = ESICCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Get BU wise States
        [HttpGet]
        public ActionResult GetBUwiseStates(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                DynamicParameters ParamState = new DynamicParameters();
                ParamState.Add("@query", " select Mas_States.StateId as Id, Mas_States.StateName as Name from Mas_States,Mas_Branch where Mas_States.StateId=Mas_Branch.StateId and Mas_Branch.BranchId=" + BranchId + " and Mas_States.Deactivate=0 and Mas_Branch.Deactivate=0");
                var State = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", ParamState).ToList();

                return Json(new { State = State}, JsonRequestBehavior.AllowGet);
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 498;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                param.Add("@p_BUComplienceId_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_BUComplience_Setting", param).ToList();
                ViewBag.BUComplienceList = data;
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
        public ActionResult IsNoDuesCheckListExists(int FNFNoDuesId, int CompanyID, int BusinessUnitID, string FNFNoDuesMappingID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "IsValidation");
                param.Add("@p_FNFNoDuesMappingID_Encrypted", FNFNoDuesMappingID_Encrypted);
                param.Add("@P_FNFNoDuesId", FNFNoDuesId);
                param.Add("@P_CompanyID", CompanyID);
                param.Add("@P_BusinessUnitID", BusinessUnitID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_NoDuesMapping", param);
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

        #region Delete
        [HttpGet]
        public ActionResult Delete(string BUComplienceId_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_BUComplienceId_Encrypted", BUComplienceId_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_BUComplience_Setting", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Statutory_BUComplience");
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