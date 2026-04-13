using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Travel;
using KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement;
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
    public class ESS_Travel_CalenderController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Travel_Calender
        public ActionResult ESS_Travel_Calender( string TravelCalenderID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 234;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Travel_Calender travel = new Travel_Calender();
                ViewBag.AddUpdateTitle = "Add";
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                   
                    
                    param.Add("@query", "select TravelPurposeID ,TravelPurpose from Claim_TravelPurpose where Deactivate=0");
                    var list_TRA_Purpose = DapperORM.ReturnList<Claim_TravelPurpose>("sp_QueryExcution", param).ToList();
                    ViewBag.GetPurposeName = list_TRA_Purpose;

                    
                    //ViewBag.EmployeeDetails = DapperORM.DynamicList("sp_List_Travel_Calender", param1);
                    if(TravelCalenderID_Encrypted != null)
                    {
                        DynamicParameters param1 = new DynamicParameters();
                        param1.Add("@p_Employeeid", Session["EmployeeId"]);
                        param1.Add("@p_TravelCalenderID_Encrypted", TravelCalenderID_Encrypted);
                        travel = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param1).FirstOrDefault();
                        ViewBag.StartDate = travel.TourStart;
                        ViewBag.EndDate = travel.TourEnd;
                        var GetDocNo = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM Travel_Calender Where Deactivate=0 and TravelCalenderID_Encrypted="+TravelCalenderID_Encrypted;
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                    else
                    {
                        var GetDocNo = "SELECT CASE  WHEN MAX(DocNo) IS NULL THEN 1 ELSE MAX(DocNo) + 1  END AS DocNo FROM Travel_Calender Where Deactivate=0";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                   
                }
                return View(travel);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Calender");
            }
        }

        [HttpGet]
        public JsonResult IsTravelCalenderExists(string TravelCalenderID_Encrypted,DateTime TourStart,DateTime TourEnd)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TravelCalenderEmployeeId", Session["EmployeeId"]);
                    param.Add("@p_TravelCalenderID_Encrypted", TravelCalenderID_Encrypted);
                    param.Add("@p_TourStart", TourStart);
                    param.Add("@p_TourEnd", TourEnd);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Calender", param);
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


        [HttpPost]
        public ActionResult Save(Travel_Calender Travel)
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { Area = "" });
            }
            try
            {
                if(Travel.TravelCalenderID_Encrypted ==" " || Travel.TravelCalenderID_Encrypted == null)
                {
                    param.Add("@p_process", "Save");
                }
                else
                {
                    param.Add("@p_process", "Update");
                }
                param.Add("@p_DocNo", Travel.DocNo);
                param.Add("@p_TravelCalenderID_Encrypted", Travel.TravelCalenderID_Encrypted);
                param.Add("@p_TravelCalenderEmployeeId", Session["EmployeeId"]);
                param.Add("@p_TravelCalenderTravelPurposeID", Travel.TravelCalenderTravelPurposeID);
                param.Add("@p_PurposeDescription", Travel.PurposeDescription);
                param.Add("@p_TravelType", Travel.TravelType);
                param.Add("@P_DayType", Travel.DayType);
                param.Add("@p_TourStart", Travel.TourStart);
                param.Add("@p_TourEnd", Travel.TourEnd);
                param.Add("@p_NoofDays", Travel.NoofDays);
                param.Add("@p_TravelNote", Travel.TravelNote);
                param.Add("@p_Confidential", Travel.Confidential);
                param.Add("@p_TravelStatus", Travel.TravelStatus);
                param.Add("@p_AdditionalEmployee", Travel.AdditionalEmployee);
                param.Add("@p_Remark", Travel.Remark);
                param.Add("@p_CmpId", Travel.CmpId);
                param.Add("@p_TravelBranchID", Travel.TravelBranchID);
                param.Add("@P_CmpId", Travel.CmpId);
                param.Add("@P_DocDate", DateTime.Now);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Travel_Calender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_Travel_Calender", "ESS_Travel_Calender");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Travel_Calender");
            }
        }


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

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 234;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                DynamicParameters param1 = new DynamicParameters(); 
                param1.Add("@p_Employeeid", Session["EmployeeId"]);
                param1.Add("@p_TravelCalenderID_Encrypted","List");
                ViewBag.CalenderDetails = DapperORM.ReturnList<Travel_Calender>("sp_List_Travel_Calender", param1).ToList();
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
        public ActionResult Delete(string TravelCalenderID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_TravelCalenderID_Encrypted", TravelCalenderID_Encrypted);
                param.Add("@p_TravelCalenderEmployeeId", Session["EmployeeId"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Travel_Calender", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Travel_Calender");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Travel_Calender");
            }
        }
        #endregion
    }
}