using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Outbox
{
    public class ESS_Outbox_OutboxController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        #region Outbox Main View Page
        // GET: ESS/ESS_Outbox_Outbox
        public ActionResult ESS_Outbox_Outbox()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters Outbox = new DynamicParameters();
                Outbox.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_OutBox", Outbox).ToList();
                TempData["TotalCount"] = data.Count;
                if (data != null)
                {
                    ViewBag.GetOutboxList = data;
                }
                else
                {
                    ViewBag.GetOutboxList = "";
                }

                DynamicParameters OutboxReject = new DynamicParameters();
                OutboxReject.Add("@p_EmployeeId", Session["EmployeeId"]);
                var data1 = DapperORM.ExecuteSP<dynamic>("sp_List_OutBox_Rejected", OutboxReject).ToList();
                TempData["RejectTotalCount"] = data1.Count;
                if (data != null)
                {
                    ViewBag.GetOutboxRejectList = data1;
                }
                else
                {
                    ViewBag.GetOutboxRejectList = "";
                }
                return View();
            }
            catch (Exception)
            {

                throw;
            }
            #endregion
        }

        #region GetContactDetails
        [HttpGet]
        public ActionResult GetContactDetails(string Origin, string DocID)
        {
            try
            {
                //param.Add("@EmployeeId", Session["EmployeeId"]);
                //var GetOutDoorlist = DapperORM.ExecuteSP<dynamic>("SP_getReportingManager", param).ToList();
                //var GetOutDoor = GetOutDoorlist;

                param.Add("@p_Origin", Origin+"Request");
                param.Add("@p_DocId_Encrypted", DocID);
                var GetAllList = DapperORM.ExecuteSP<dynamic>("Sp_GetManager_Module", param).ToList(); // SP_getReportingManager
                var GetAllData = GetAllList;
                return Json(new { data = GetAllData }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet); ;
            }
        }
        #endregion
    }
}