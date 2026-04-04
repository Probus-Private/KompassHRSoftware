using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Inbox;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Inbox
{
    public class ESS_Inbox_InboxController : Controller
    {
        DynamicParameters param = new DynamicParameters();

        // GET: Inbox/Inbox
        #region Main page of Leave Details Inbox
        // GET: ESS/ESS_Inbox_Inbox
        public ActionResult ESS_Inbox_Inbox(string GetFilters)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                if (GetFilters == null)
                {
                    param.Add("@p_List", "All");
                }
                else
                {
                    param.Add("@p_List", GetFilters);
                }
                var GetPending_ForApproval = DapperORM.ExecuteSP<dynamic>("sp_List_Pending_ForApproval", param).ToList();
                TempData["LeaveCount"] = GetPending_ForApproval.Count();
                ViewBag.GetPending_ForApproval = GetPending_ForApproval;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_EmployeeId", Session["EmployeeId"]);
                var GetRquisitionCount = DapperORM.ExecuteSP<dynamic>("sp_ESSDashboard_Team", param1);
                if (GetRquisitionCount != null)
                {
                    ViewBag.RquisitionCount = GetRquisitionCount;
                }
                else
                {
                    ViewBag.RquisitionCount = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
            #endregion

        #region Get InboxList And Filter InboxList
        public ActionResult GetInboxList(string GetFilter)
        {
            try
            {
                return RedirectToAction("ESS_Inbox_Inbox", "ESS_Inbox_Inbox", new { area = "ESS", GetFilters = GetFilter });

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Email Details Based on ID and Origin
        [HttpGet]
        public ActionResult GetEmailDetails(string DocID, string Origin)
        {
            try
            {
                Session["DocID"] = DocID;
                Session["Origin"] = Origin;

                return RedirectToAction("EmailDetails", "ESS_Inbox_Inbox", new { area = "ESS" });
                //return Json(new { data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Main page of Email details
        public ActionResult EmailDetails()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_DocId_Encrypted", Session["DocID"]);
                param.Add("@p_Origin", Session["Origin"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_TimeOffice_Profiles", param).First();
                ViewBag.GetEmailDetails = data;
                //TempData["HideShow"] = data.HideShow;
                //TempData["Attachment"] = data.Attachment;
                Session["DocID"] = null;
                Session["Origin"] = null;

                //param.Add("@p_DocID", Session["DocID"]);
                //param.Add("@p_Origin", Session["Origin"]);
                //param.Add("@p_ManagerId", Session["EmployeeId"]);
                //var data = DapperORM.ExecuteSP<dynamic>("sp_List_Pending_ForApproval_Details", param).First();
                //ViewBag.GetEmailDetails = data;
                //TempData["HideShow"] = data.HideShow;
                //TempData["Attachment"] = data.Attachment;
                //Session["DocID"] = null;
                //Session["Origin"] = null;


                //Get Buttom RecruitmentModal data on ViewMore Click
                //param.Add("@P_Qry", "and Status='Pending'");
                //param.Add("@p_ResourceId_Encrypted", "List");
                //var RecruitmentList = DapperORM.DynamicList("sp_List_Recruitment_ResourceRequest", param);
                //ViewBag.RecruitmentList = RecruitmentList;

                //SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                //var path = DapperORM.DynamicQuerySingle("Select Stamp from Mas_CompanyProfile where CompanyId= " + Session["CompanyId"] + "");
                // var fullPath = path.Stamp;

                //var path = DapperORM.DynamicQuerySingle(GetCompanyStamp);
                //byte[] fileBytes = GetFile(fullPath);

                //System.IO.File.ReadAllBytes(fullPath);
                //return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(fullPath));

                //TempData["LeaveFile"] = fullPath;
                //fullPath = null;
                //if (data.Attachment != "")
                //{
                //    using (Image image = Image.FromFile(data.Attachment))
                //    {
                //        using (MemoryStream m = new MemoryStream())
                //        {
                //            image.Save(m, image.RawFormat);
                //            byte[] imageBytes = m.ToArray();
                //            string base64String = Convert.ToBase64String(imageBytes);
                //            //Session["LeaveFile"] = "data:image; base64," + base64String;
                //            if(base64String!=null)
                //            {
                //                TempData["LeaveFile"] = "data:image; base64," + base64String;
                //            }
                //            else
                //            {
                //                TempData["LeaveFile"] = "data:image; base64," + "";
                //            }

                //        }
                //    }
                //}
                return View();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Download Image 
        public ActionResult Download(string filePath)
        {
            //var GetDrivePath = (from d in dbContext.Onboarding_Mas_DocumentPath select d.DosumentPath).FirstOrDefault();
            //var fullPath = GetDrivePath + filePath;
            //byte[] fileBytes = GetFile(fullPath);

            if (filePath != "")
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.ReadAllBytes(filePath);
                    return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(filePath));
                }
                else
                {
                    return Json(false);
                }
            }
            else
            {
                return RedirectToAction("EmailDetails", "ESS_Inbox_Inbox", new { Area = "ESS" });
            }
        }
        #endregion

        #region  Approve Leave Request function
        [HttpPost]
        public ActionResult ApproveLeaveRequest(string Status, string Remark , int? ApproveAmount)
        {
            try
            {
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
                param.Add("@p_Origin", Session["Origin"]);
                param.Add("@p_DocId_Encrypted", Session["DocID"]);
                param.Add("@p_ApproveRejectRemark", Remark);
                param.Add("@p_Status", Status);
                param.Add("@p_ApproveAmount", ApproveAmount);
                param.Add("@p_Managerid", Session["EmployeeId"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region  Multuple Approve Leave Request function
        [HttpPost]
        public ActionResult MultipleApproveLeaveRequest(List<RecordList> RecordList)
        {
            try
            {
                for (var i = 0; i < RecordList.Count; i++)
                {
                    param.Add("@p_Origin", RecordList[i].Origin);
                    param.Add("@p_DocId_Encrypted", RecordList[i].DocID);
                    param.Add("@p_ApproveRejectRemark", RecordList[i].ApproveRejectRemark);
                    param.Add("@p_Status", RecordList[i].Status);
                    param.Add("@p_Managerid", Session["EmployeeId"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                    var data = DapperORM.ExecuteReturn("sp_Approved_Rejected", param);
                    var message = param.Get<string>("@p_msg");
                    var Icon = param.Get<string>("@p_Icon");
                    TempData["Message"] = message;
                    TempData["Icon"] = Icon;
                }
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("Inbox", "Inbox", new { Area = "Inbox" });
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region  GetRecruitmentClaim Pending List
        [HttpGet]
        public ActionResult GetRecruitmentList()
        {
            try
            {
                param.Add("@P_Qry", "and Status='Pending'");
                param.Add("@p_ResourceId_Encrypted", "List");
                var RecruitmentList = DapperORM.DynamicList("sp_List_Recruitment_ResourceRequest", param);
                
                return Json(new { data = RecruitmentList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region  GetTravelClaim Pending List
        [HttpGet]
        public ActionResult GetTravelClaimList()
        {
            try
            {
                //Get Buttom TravelClaimModal data on ViewMore Click
                var EmployeeId = Session["EmployeeId"];
                param = new DynamicParameters();
                param.Add("@p_TravelClaimId_Encrypted", "List");
                param.Add("@P_Qry", "and Status = 'Pending'");
                var TravleClaimList = DapperORM.ExecuteSP<dynamic>("sp_List_Claim_Travel", param).ToList();
                
                return Json(new { data= TravleClaimList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
