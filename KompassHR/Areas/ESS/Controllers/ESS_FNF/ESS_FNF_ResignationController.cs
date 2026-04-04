using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FNF;
using KompassHR.Areas.Setting.Models.Setting_FullAndFinal;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Globalization;

namespace KompassHR.Areas.ESS.Controllers.ESS_FNF
{
    public class ESS_FNF_ResignationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        // GET: ESS/ESS_FNF_Resignation
        #region ESS_FNF_Resignation Main View
        [HttpGet]
        public ActionResult ESS_FNF_Resignation(double? FnfID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 169;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                FNF_EmployeeResignation NewFNF_EmployeeResignation = new FNF_EmployeeResignation();

                param.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and FnfEmployeeId = '" + Session["EmployeeId"] + "'");
                var FNFResignationApproval = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_EmployeeResignation", param).FirstOrDefault();
                ViewBag.ResignationApprovalList = FNFResignationApproval;

                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM FNF_EmployeeResignation Where Deactivate=0";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    DynamicParameters param5 = new DynamicParameters();
                    param5.Add("@query", "Select EmployeeId,EmployeeName from Mas_Employee Where Deactivate=0 order by EmployeeName");
                    var listMas_Employee = DapperORM.ReturnList<Mas_Employee>("sp_QueryExcution", param5).ToList();
                    ViewBag.GetEmployeeName = listMas_Employee;

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@query", "Select ReasonId,ReasonName from FNF_Reason Where Deactivate=0 order by ReasonName");
                    var list_FNF_Reason = DapperORM.ReturnList<FNF_Reason>("sp_QueryExcution", param6).ToList();
                    ViewBag.GetReasonName = list_FNF_Reason;

                    DynamicParameters param7 = new DynamicParameters();
                    param7.Add("@query", "select EmployeeDepartmentID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");
                    var DepartmentID = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param7).FirstOrDefault();

                    DynamicParameters param8 = new DynamicParameters();
                    param8.Add("@query", "select EmployeeDesignationID from Mas_Employee where EmployeeId=" + Session["EmployeeID"] + "");
                    var DesignationID = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param8).FirstOrDefault();

                    DynamicParameters param9 = new DynamicParameters();
                    param9.Add("@query", "select NoticePeriodGradeId from FNF_NoticePeriod where DepartmentID = " + DepartmentID.EmployeeDepartmentID + " and DesignationID = " + DesignationID.EmployeeDesignationID + " and Deactivate = 0");
                    var NoticePeriodGradeId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param9).FirstOrDefault();

                    if (NoticePeriodGradeId != null)
                    {
                        DynamicParameters param13 = new DynamicParameters();
                        param.Add("@query", "select NoticePeriodDays from FNF_NoticePeriod where DepartmentID = " + DepartmentID.EmployeeDepartmentID + " and DesignationID = " + DesignationID.EmployeeDesignationID + " and NoticePeriodGradeId = " + NoticePeriodGradeId.NoticePeriodGradeId + " and Deactivate = 0");
                        var list_FNF_NoticePeriodDays = DapperORM.ReturnList<FNF_EmployeeResignation>("sp_QueryExcution", param13).FirstOrDefault();
                        ViewBag.GetNoticePeriodDays = list_FNF_NoticePeriodDays;
                    }
                    DynamicParameters param10 = new DynamicParameters();
                    param10.Add("@query", "select CmpID from mas_employee where EmployeeId=" + Session["EmployeeId"] + "");
                    var cmpid = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param10).FirstOrDefault();

                    DynamicParameters param11 = new DynamicParameters();
                    param11.Add("@query", "select EmployeeBranchId from mas_employee where EmployeeId=" + Session["EmployeeId"] + "");
                    var EmployeeBranchId = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param11).FirstOrDefault();

                    DynamicParameters param12 = new DynamicParameters();
                    param12.Add("@query", "select TermsAndConditionName from FNF_TermsAndCondition where Deactivate =0 and CompanyID=" + cmpid.CmpID + " and BusinessUnitID=" + EmployeeBranchId.EmployeeBranchId + "");
                    var TermAndConditionName = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", param12).FirstOrDefault();
                    ViewBag.TermAndConditionName = TermAndConditionName;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_Employeeid", Session["EmployeeId"]);
                    var info = DapperORM.ExecuteSP<dynamic>("sp_FNF_GetEmployeeDetails", param1).FirstOrDefault();
                    DateTime NoticePeriodEndDate;
                    DateTime LastWorkingDate;
                    if (info != null)
                    {
                        if (DateTime.TryParseExact(info.LastWorkingDate, "dd/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out LastWorkingDate))
                        {
                            TempData["LastWorkingDate"] = LastWorkingDate.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            TempData["LastWorkingDate"] = "";
                        }
                        if (DateTime.TryParseExact(info.NoticePeriodEndDate, "dd/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out NoticePeriodEndDate))
                        {
                            NewFNF_EmployeeResignation.NoticePeriodEndDate = NoticePeriodEndDate;
                        }
                        else
                        {
                            TempData["LastWorkingDate"] = "";
                        }
                    }
                    ViewBag.EmployeeDetails = info;
                    NewFNF_EmployeeResignation.NoticePeriodDays = ViewBag.EmployeeDetails.DayMonth;
                    NewFNF_EmployeeResignation.NoticePeriodType = ViewBag.EmployeeDetails.NoticePeriodType;
                }
                return View(NewFNF_EmployeeResignation);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_Resignation");
            }

        }
        #endregion

        #region IsFnfResignationExists
        [HttpGet]
        public JsonResult IsFnfResignationExists(string FnfIDEncrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_FnfEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_FnfId_Encrypted", FnfIDEncrypted);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignation", param);
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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Save
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(FNF_EmployeeResignation EmployeeResignation, HttpPostedFileBase AssignFilePath, string Remark)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                param.Add("@p_process", "Save");
                param.Add("@P_FnfID", EmployeeResignation.FnfId);
                param.Add("@P_FnfID_Encrypted", EmployeeResignation.FnfId_Encrypted);
                param.Add("@P_FnfEmployeeId", Session["EmployeeId"]);
                param.Add("@P_ResignationDate", Convert.ToDateTime(EmployeeResignation.ResignationDate));
                param.Add("@p_LastWorkingDate", Convert.ToDateTime(EmployeeResignation.LastWorkingDate));
                param.Add("@p_NoticePeriodEndDate", Convert.ToDateTime(EmployeeResignation.NoticePeriodEndDate));
                param.Add("@P_FnfReasonId", EmployeeResignation.FnfReasonId);
                param.Add("@p_Cmpid", EmployeeResignation.CmpId);
                param.Add("@p_FnfBranchId", EmployeeResignation.FnfBranchID);
                param.Add("@p_NoticePeriodDays", EmployeeResignation.NoticePeriodDays);
                param.Add("@p_NoticePeriodType", EmployeeResignation.NoticePeriodType);
                param.Add("@P_Remark", Remark);
                param.Add("@P_IsImmediatelyRelieve", EmployeeResignation.IsImmediatelyRelieve);
                param.Add("@P_IsNoticePeriodPurchase", EmployeeResignation.IsNoticePeriodPurchase);
                param.Add("@p_PrimaryMobile", EmployeeResignation.PrimaryMobile);
                param.Add("@p_PersonalEmailId", EmployeeResignation.PersonalEmailId);
                param.Add("@p_FullAddress", EmployeeResignation.FullAddress);
                param.Add("@p_AcceptTerms", EmployeeResignation.AcceptTerms);
                param.Add("@p_AttachedFileName", AssignFilePath == null ? "" : AssignFilePath.FileName);
                param.Add("@P_RequestFrom", "WEB");
                param.Add("@p_IsCalculated", EmployeeResignation.IsCalculated);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                Session["pid"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && EmployeeResignation.FnfId_Encrypted != null || AssignFilePath != null)
                {
                    var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='FNF'").FirstOrDefault();
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (AssignFilePath != null)
                    {
                        string fileFullPath = "";
                        fileFullPath = FirstPath + AssignFilePath.FileName;
                        AssignFilePath.SaveAs(fileFullPath);
                    }
                }

                var GetcmpId = DapperORM.DynamicQuerySingle("Select CmpID From Mas_Employee where Deactivate = 0 and EmployeeId='" + Session["EmployeeId"] + "'");
                decimal CmpId = GetcmpId.CmpID;
                var SMTPGET = DapperORM.DynamicQueryList("Select CmpId, Origin, SMTPServerName, PortNo, SSL, FromEmailId, Password from Tool_EmailSetting where Deactivate = 0 and CmpId = '" + CmpId + "'  and  Origin =1").ToList();
                if (SMTPGET.Count > 0)
                {
                    clsEmail emailsaveProcess = new clsEmail();
                    emailsaveProcess.SendMail(int.Parse(Session["EmployeeId"].ToString()), Convert.ToInt16(CmpId), "2", "1");
                }

                return RedirectToAction("GetList", "ESS_FNF_Resignation");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_FNF_Resignation");
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
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 169;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters paramAddRevoke = new DynamicParameters();
                paramAddRevoke.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetAddRevoke = DapperORM.ExecuteSP<dynamic>("sp_FNF_Add_Revoke", paramAddRevoke).FirstOrDefault();
                ViewBag.AddRevoke = GetAddRevoke.Add_Revoke;

                DynamicParameters MulQuery = new DynamicParameters();
                MulQuery.Add("@P_EmployeeId", Session["EmployeeId"]);
                using (var multi = DapperORM.DynamicMultipleResultList("sp_FNF_ResignationStatus", MulQuery))
                {
                    ViewBag.ResignationDetails = multi.Read<dynamic>().FirstOrDefault();
                    ViewBag.AcknowledgementDetails = multi.Read<dynamic>().ToList();
                    ViewBag.ApprovalDetail= multi.Read<dynamic>().ToList();
                    ViewBag.DuesDetails = multi.Read<dynamic>().ToList();
                    ViewBag.FNFDetails = multi.Read<dynamic>().ToList();
                }
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_FNF_Resignation ");
            }

        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string FnfId)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@P_FnfEmployeeId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FNF_EmployeeResignation", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_FNF_Resignation");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetResignationHistory
        [HttpGet]
        public ActionResult GetResignationHistory()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_FNF_ResignedEmployeeHistroy", param).ToList();
                string jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}