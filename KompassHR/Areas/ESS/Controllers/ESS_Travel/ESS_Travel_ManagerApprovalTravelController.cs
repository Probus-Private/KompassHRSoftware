using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Travel;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
 
namespace KompassHR.Areas.ESS.Controllers.ESS_Travel
{
    public class ESS_Travel_ManagerApprovalTravelController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Travel_ManagerApprovalTravel
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
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 258;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_TravelPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID",0);
                param1.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and Tra_Approval.TraApproval_ApproverEmployeeId=" + Session["EmployeeId"]+ "order by TravelPlanID desc");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Travel>("sp_List_Travel_Plan_Travel", param1).ToList();

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param3.Add("@p_AccomodationPlanID_Encrypted", "List");
                param3.Add("@p_TravelCalenderID", 0);
                param3.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and TraApproval_ApproverEmployeeId = " + Session["EmployeeId"]+ "order by AccomodationPlanID desc");

                ViewBag.AccomodationPlanDetails = DapperORM.ReturnList<Travel_Plan_Accomodation>("sp_List_Travel_Plan_Accomodation", param3).ToList();

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param4.Add("@p_LocalTranPlanID_Encrypted", "List");
                param4.Add("@p_TravelCalenderID",0);
                param4.Add("@P_Qry", "and Tra_Approval.Status in('Pending') and TraApproval_ApproverEmployeeId = " + Session["EmployeeId"] + "order by LocalTranPlanID desc");

                ViewBag.LocalPlanDetails = DapperORM.ReturnList<Travel_Plan_LocalTransport>("sp_List_Travel_Plan_LocalTransport", param4).ToList();

                DynamicParameters param5 = new DynamicParameters();
                param5.Add("@ManagerId", Session["EmployeeId"]);
                ViewBag.LocalPlanDetails11 = DapperORM.ExecuteSP<dynamic>("SP_List_TravelManagerApprover", param5).FirstOrDefault();
                
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        #endregion
        public ActionResult TravelApproval(string TravelPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 258;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_Travel Where Deactivate=0  and TravelPlanID_Encrypted='" + TravelPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;


                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_TravelPlanID_Encrypted", TravelPlanID_Encrypted);
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                //param1.Add("@p_Qry", "");
                ViewBag.TravelPlanDetails = DapperORM.ExecuteSP<dynamic>("sp_List_Travel_Plan_Travel", param1).FirstOrDefault();
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();
               
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }
        
        public ActionResult AccomodationApproval(string AccomodationPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_Accomodation Where Deactivate=0  and AccomodationPlanID_Encrypted='" + AccomodationPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;


                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_AccomodationPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_AccomodationPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                //param1.Add("@p_Qry", "");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_Accomodation>("sp_List_Travel_Plan_Accomodation", param1).FirstOrDefault();
             
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();
               
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        public ActionResult LocTransportApproval(string LocalTranPlanID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                var TravelCalenderID1 = DapperORM.DynamicQuerySingle("SELECT TravelCalenderID FROM Travel_Plan_LocalTransport Where Deactivate=0  and LocalTranPlanID_Encrypted='" + LocalTranPlanID_Encrypted + "'");
                var TravelCalenderID = TravelCalenderID1.TravelCalenderID;
                
                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_LTPlanEmployeeId", Session["EmployeeId"]);
                param1.Add("@p_LocalTranPlanID_Encrypted", "List");
                param1.Add("@p_TravelCalenderID", TravelCalenderID);
                //param1.Add("@p_Qry", "");
                ViewBag.TravelPlanDetails = DapperORM.ReturnList<Travel_Plan_LocalTransport>("sp_List_Travel_Plan_LocalTransport", param1).FirstOrDefault();
                var encyCalId = ViewBag.TravelPlanDetails.TravelCalenderID_Encrypted;
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_Employeeid", Session["EmployeeId"]);
                param2.Add("@p_TravelCalenderID_Encrypted", encyCalId);
                ViewBag.Calender = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param2).FirstOrDefault();

                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        [HttpPost]
        public ActionResult Save(Travel_Plan_Travel Travel)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                //if (Travel.TravelPlanID_Encrypted == " " || Travel.TravelPlanID_Encrypted == null)
                //{
                //    param.Add("@p_process", "Save");
                //    param.Add("@P_DocDate", DateTime.Now);
                //}
                //else
                //{
                //    param.Add("@p_process", "Update");
                //    param.Add("@P_CmpId", Travel.CmpId);

                //}
                //param.Add("@p_DocNo", Travel.DocNo);
                //param.Add("@p_TravelPlanID_Encrypted", Travel.TravelPlanID_Encrypted);
                //param.Add("@p_TravelPlanEmployeeId", Session["EmployeeId"]);
                //param.Add("@p_TravelCalenderID", Travel.TravelCalenderID);
                //param.Add("@p_PlanType", Travel.PlanType);
                //param.Add("@p_Mode", Travel.Mode);
                //param.Add("@p_BookingStatus", "Approved");
                //param.Add("@p_RejectionRemark", Travel.RejectionRemark);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Travel", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");

                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", Travel.TravelPlanID_Encrypted);
                paramApprove.Add("@p_DocId", Travel.DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Approved");
                paramApprove.Add("@p_Type", "Travel");
                paramApprove.Add("@p_ApproveRejectRemark", Travel.RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");

                return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new { TravelPlanID_Encrypted = Travel.TravelPlanID_Encrypted });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "TravelApproval");
            }
        }

        [HttpPost]
        public ActionResult SaveAccomodation(Travel_Plan_Accomodation Travel)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", Travel.AccomodationPlanID_Encrypted);
                paramApprove.Add("@p_DocId", Travel.DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Approved");
                paramApprove.Add("@p_Type", "Accomodation");
                paramApprove.Add("@p_ApproveRejectRemark", Travel.RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");
                return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new {  });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "AccomodationApproval");
            }
        }
        
        [HttpPost]
        public ActionResult SaveLocalTransport(Travel_Plan_LocalTransport Travel)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", Travel.LocalTranPlanID_Encrypted);
                paramApprove.Add("@p_DocId", Travel.DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Approved");
                paramApprove.Add("@p_Type", "LocalTransport");
                paramApprove.Add("@p_ApproveRejectRemark", Travel.RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");

                return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new { });
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "LocalTransportApproval");
            }
        }

        public ActionResult TravelReject(string TravelPlanID_Encrypted , string RejectionRemark , int ? DocNo)
        {
            try
            {
                //if (TravelPlanID_Encrypted == " " || TravelPlanID_Encrypted == null)
                //{
                //    param.Add("@p_process", "Save");
                //}
                //else
                //{
                //    param.Add("@p_process", "Update");

                //}
                //param.Add("@p_TravelPlanID_Encrypted", TravelPlanID_Encrypted);
                //param.Add("@p_BookingStatus", "Rejected");
                //param.Add("@p_RejectionRemark", RejectionRemark);
                //param.Add("@p_MachineName", Dns.GetHostName().ToString());
                //param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                //param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                //var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Plan_Travel", param);
                //TempData["Message"] = param.Get<string>("@p_msg");
                //TempData["Icon"] = param.Get<string>("@p_Icon");
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", TravelPlanID_Encrypted);
                paramApprove.Add("@p_DocId", DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Rejected");
                paramApprove.Add("@p_Type", "Travel");
                paramApprove.Add("@p_ApproveRejectRemark", RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");

                if (TempData["Message"] != "")
                {
                    //return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new { TravelPlanID_Encrypted = TravelPlanID_Encrypted });
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        public ActionResult AccomodationReject(string AccomodationPlanID_Encrypted, string RejectionRemark, int? DocNo)
        {
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", AccomodationPlanID_Encrypted);
                paramApprove.Add("@p_DocId", DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Rejected");
                paramApprove.Add("@p_Type", "Accomodation");
                paramApprove.Add("@p_ApproveRejectRemark", RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");

                if (TempData["Message"] != "")
                {
                    //return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new { TravelPlanID_Encrypted = TravelPlanID_Encrypted });
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

        public ActionResult LocalTransportReject(string LocalTranPlanID_Encrypted, string RejectionRemark , int? DocNo)
        {
            try
            {
                DynamicParameters paramApprove = new DynamicParameters();
                paramApprove.Add("@p_Origin", "TravelDesk");
                paramApprove.Add("@p_DocId_Encrypted", LocalTranPlanID_Encrypted);
                paramApprove.Add("@p_DocId", DocNo);
                paramApprove.Add("@p_Managerid", Session["EmployeeId"]);
                paramApprove.Add("@p_Status", "Rejected");
                paramApprove.Add("@p_Type", "LocalTransport");
                paramApprove.Add("@p_ApproveRejectRemark", RejectionRemark);
                paramApprove.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                paramApprove.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var GetApprovedResult = DapperORM.ExecuteSP<dynamic>("sp_Approved_Rejected", paramApprove);
                var Message = paramApprove.Get<string>("@p_msg");
                var Icon = paramApprove.Get<string>("@p_icon");
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_icon");
                if (TempData["Message"] != "")
                {
                    //return RedirectToAction("GetList", "ESS_Travel_ManagerApprovalTravel", new { TravelPlanID_Encrypted = TravelPlanID_Encrypted });
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Plans");
            }

        }

    }
}