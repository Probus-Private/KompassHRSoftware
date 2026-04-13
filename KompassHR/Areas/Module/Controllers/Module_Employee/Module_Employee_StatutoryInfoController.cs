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

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_StatutoryInfoController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: Module/Module_Employee_StatutoryInfo
        #region Statutory Main View
        [HttpGet]
        public ActionResult Module_Employee_StatutoryInfo(string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 418;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                Mas_Employee_Statutory EmployeeStatutory = new Mas_Employee_Statutory();
                ViewBag.AddUpdateTitle = "Add";
                var ResultList = DapperORM.DynamicQueryMultiple(@"Select PTCodeId as Id,PTRemark as Name from Payroll_PTCode Where Deactivate=0 and CmpID=" + Session["OnboardCmpId"] + ""
                                                        + "Select LWFCodeId as Id,LWFRemark as Name from Payroll_LWFCode Where Deactivate=0 and CmpID=" + Session["OnboardCmpId"] + ""
                                                        + "Select ESICCodeId as Id,ESICRemark as Name from Payroll_ESICCode Where Deactivate=0 and  CmpID=" + Session["OnboardCmpId"] + " "
                                                        + "Select PFCodeId as Id,PFRemark as Name from Payroll_PFCode Where Deactivate=0 and  CmpId=" + Session["OnboardCmpId"] + ""
                                                        + "Select PTSlabMasterId as Id,Remark as Name from Payroll_PTSlab_Master Where Deactivate=0 ;"
                                                        + "Select LWFSlabMasterId as Id,Remark as Name from Payroll_LWFSlab_Master Where Deactivate=0 ;"
                                                        + "Select PFWagesMasterId as Id,PFWagesRemark as Name from Payroll_PFWages_Master Where Deactivate=0 and  CmpId=" + Session["OnboardCmpId"] + ""
                                                        + "Select RelationId as Id,RelationName as Name from Mas_Relation Where Deactivate=0;");
                ViewBag.GetPTStateCode = ResultList[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFCode = ResultList[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetESICCode = ResultList[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFCode = ResultList[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPTSlabMaster = ResultList[4].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetLWFSlabMaster = ResultList[5].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetPFWageMaster = ResultList[6].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetRelationName = ResultList[7].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();


                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_StatutoryEmployeeId", Session["OnboardEmployeeId"]);
                EmployeeStatutory = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_List_Mas_Employee_Statutory", paramList).FirstOrDefault();
                Mas_Employee_Statutory Statutory = new Mas_Employee_Statutory();
                var countEMP = DapperORM.DynamicQuerySingle("select COUNT(StatutoryId) as StatutoryId from Mas_Employee_Statutory where Mas_Employee_Statutory.StatutoryEmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["CountReferenceEmployeeId"] = countEMP.StatutoryId;
                if (EmployeeStatutory != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    TempData["PF_DOB1"] = EmployeeStatutory.PF_DOB1;
                }

                if (Status == "Pre")
                {
                    var GetPreboardingFid = DapperORM.DynamicQuerySingle("Select PreboardingFid from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");
                    var PreboardingFid = GetPreboardingFid.PreboardingFid;

                    param = new DynamicParameters();
                    param.Add("@p_FId", PreboardingFid);
                    Statutory = DapperORM.ReturnList<Mas_Employee_Statutory>("sp_GetList_Mas_PreboardEmployeeDetails", param).FirstOrDefault();
                    ViewBag.GetEmployeePersonal = Statutory;
                    TempData["PF_DOB1"] = Statutory.PF_DOB1;
                    return View(Statutory);

                }
                ViewBag.PFApplicable = EmployeeStatutory;
                return View(EmployeeStatutory);
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
        public ActionResult SaveUpdate(Mas_Employee_Statutory Statutory)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(Statutory.StatutoryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_StatutoryId", Statutory.StatutoryId);
                param.Add("@p_StatutoryId_Encrypted", Statutory.StatutoryId_Encrypted);
                param.Add("@p_StatutoryEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_ESIC_Applicable", Statutory.ESIC_Applicable);
                param.Add("@p_ESIC_CodeId", Statutory.ESIC_CodeId);
                param.Add("@p_ESIC_NO", Statutory.ESIC_NO);
                param.Add("@p_ESIC_ClosingDate", Statutory.ESIC_ClosingDate);
                param.Add("@p_ESIC_IS_OldESICNo", Statutory.ESIC_IS_OldESICNo);
                param.Add("@p_ESIC_IS_LinkWithESIC", Statutory.ESIC_IS_LinkWithESIC);
                param.Add("@p_ESIC_PreviousESICNo", Statutory.ESIC_PreviousESICNo);
                param.Add("@p_PT_Applicable", Statutory.PT_Applicable);
                param.Add("@p_PT_CodeId", Statutory.PT_CodeId);
                param.Add("@p_LWF_Applicable", Statutory.LWF_Applicable);
                param.Add("@p_LWF_CodeId", Statutory.LWF_CodeId);
                param.Add("@p_PF_Applicable", Statutory.PF_Applicable);
                param.Add("@p_PF_CodeId", Statutory.PF_CodeId);
                param.Add("@p_PF_Limit", Statutory.PF_Limit);
               // param.Add("@p_PF_EPS", Statutory.PF_EPS);
                param.Add("@p_PF_VPF", Statutory.PF_VPF);
                param.Add("@p_PF_FSType", Statutory.PF_FSType);
                param.Add("@p_PF_FS_Name", Statutory.PF_FS_Name);
                param.Add("@p_PF_UAN", Statutory.PF_UAN);
                param.Add("@p_PF_NO", Statutory.PF_NO);
                param.Add("@p_PF_Nominee1", Statutory.PF_Nominee1);
                param.Add("@p_PF_Reletion1", Statutory.PF_Reletion1);
                param.Add("@p_PF_DOB1", Statutory.PF_DOB1);
                param.Add("@p_PF_Share1", Statutory.PF_Share1);
                param.Add("@p_PF_Address1", Statutory.PF_Address1);
                param.Add("@p_PF_GuardianName1", Statutory.PF_GuardianName1);
                param.Add("@p_PF_Nominee2", Statutory.PF_Nominee2);
                param.Add("@p_PF_Reletion2", Statutory.PF_Reletion2);
                param.Add("@p_PF_DOB2", Statutory.PF_DOB2);
                param.Add("@p_PF_Share2", Statutory.PF_Share2);
                param.Add("@p_PF_Address2", Statutory.PF_Address2);
                param.Add("@p_PF_GuardianName2", Statutory.PF_GuardianName2);
                param.Add("@p_PF_Nominee3", Statutory.PF_Nominee3);
                param.Add("@p_PF_Reletion3", Statutory.PF_Reletion3);
                param.Add("@p_PF_DOB3", Statutory.PF_DOB3);
                param.Add("@p_PF_Share3", Statutory.PF_Share3);
                param.Add("@p_PF_Address3", Statutory.PF_Address3);
                param.Add("@p_PF_GuardianName3", Statutory.PF_GuardianName3);
                param.Add("@p_PF_MobileNo", Statutory.PF_MobileNo);
                param.Add("@p_PF_BankName", Statutory.PF_BankName);
                param.Add("@p_PF_BankIFSC", Statutory.PF_BankIFSC);
                param.Add("@p_PF_Account", Statutory.PF_Account);
                param.Add("@p_PF_1952", Statutory.PF_1952);
                param.Add("@p_PF_1995", Statutory.PF_1995);
                param.Add("@p_PF_PreviousPFNo", Statutory.PF_PreviousPFNo);
                param.Add("@p_PF_ExitDate", Statutory.PF_ExitDate);
                param.Add("@p_PF_CertificateNo", Statutory.PF_CertificateNo);
                param.Add("@p_PF_PPO", Statutory.PF_PPO);
                param.Add("@p_PF_OldUANNo", Statutory.PF_OldUANNo);
                param.Add("@p_PF_LinkWithUAN", Statutory.PF_LinkWithUAN);


                param.Add("@p_PTSlab_MasterId", Statutory.PTSlab_MasterId);
                param.Add("@p_LWFSlab_MasterId", Statutory.LWFSlab_MasterId);
                param.Add("@p_PFWages_MasterId", Statutory.PFWages_MasterId);

                param.Add("@p_LWF_LIN", Statutory.LWF_LIN);
                param.Add("@p_Gratuity_Applicable", Statutory.Gratuity_Applicable);
                param.Add("@p_Gratuity_No", Statutory.Gratuity_No);

                param.Add("@p_Medical_Insurance_Applicable", Statutory.Medical_Insurance_Applicable);
                param.Add("@p_Accidental_Policy_Applicable", Statutory.Accidental_Policy_Applicable);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Statutory", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_StatutoryInfo", "Module_Employee_StatutoryInfo");
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