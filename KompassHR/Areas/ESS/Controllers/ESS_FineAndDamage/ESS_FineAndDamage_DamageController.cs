using Dapper;
using KompassHR.Areas.ESS.Models.ESS_FineAndDamage;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FineAndDamage
{
    public class ESS_FineAndDamage_DamageController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_FineAndDamage_Damage
        #region Damage Main View
        [HttpGet]
        public ActionResult ESS_FineAndDamage_Damage(string DamageID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 222;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                ViewBag.AddUpdateTitle = "Add";
                Payroll_Damage ObjPayroll_Damage = new Payroll_Damage();

                var Manager1 = Session["ManagerId1"];
                var Manger2 = Session["ManagerId2"];
                var HRId = Session["HRId"];
                var EmployeeId = Session["EmployeeId"];

                var DocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Payroll_Damage");
                TempData["DocNo"] = DocNo.DocNo;

                param = new DynamicParameters();
                var EmployeerReviweLastRecord = "select max(CreatedDate) As CreatedDate from Payroll_Damage where DamageEmployeeID="+ EmployeeId + "  and Deactivate = 0";
                var LastRecored = DapperORM.DynamicQuerySingle(EmployeerReviweLastRecord);
                ViewBag.LastRecored = LastRecored.CreatedDate;

                param.Add("@query", "Select EmployeeId as Id ,EmployeeName+'-'+Convert(varchar(50),EmployeeNo) as Name from Mas_Employee Where Deactivate=0 and (ReportingManager1=" + Manager1 + " Or ReportingManager2=" + Manger2 + " Or ReportingHR =" + HRId + " ) order by EmployeeName");
                var listMas_Employee = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeName = listMas_Employee;

                if (DamageID_Encrypted != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param1.Add("@p_DamageID_Encrypted", DamageID_Encrypted);
                    ObjPayroll_Damage = DapperORM.ReturnList<Payroll_Damage>("sp_List_Payroll_Damage", param1).FirstOrDefault();
                }
                TempData["DocDate"] = ObjPayroll_Damage.DocDate;
                TempData["DeductionInMonth"] = ObjPayroll_Damage.DeductionInMonth;
                return View(ObjPayroll_Damage);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_FineAndDamage_Damage ");
            }
        }

        #endregion


        #region IsVerification
        [HttpGet]
        public JsonResult IsDamageExists(int DamageEmployeeID, string DeductionInMonth, string DamageID_Encrypted)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {

                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_DamageEmployeeID", DamageEmployeeID);
                    param.Add("@p_DamageID_Encrypted", DamageID_Encrypted);
                    param.Add("@P_DeductionInMonth", DeductionInMonth);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Damage", param);
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

        #region SaveUpdate 
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_Damage ObjDamage)
        {
            try
            {
                param.Add("@p_process", string.IsNullOrEmpty(ObjDamage.DamageID_Encrypted) ? "Save" : "Update");
                param.Add("@p_DamageID_Encrypted", ObjDamage.DamageID_Encrypted);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_DamageEmployeeID", ObjDamage.DamageEmployeeID);
                param.Add("@p_RequesterID", Session["EmployeeId"]);
                param.Add("@p_DocDate", ObjDamage.DocDate);
                param.Add("@p_DocNo", ObjDamage.DocNo);
                param.Add("@p_DamageName", ObjDamage.DamageName);
                param.Add("@p_DamageDescription", ObjDamage.DamageDescription);
                param.Add("@p_DeductionInMonth", ObjDamage.DeductionInMonth);
                param.Add("@p_DamageAmount", ObjDamage.DamageAmount);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_Damage", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_FineAndDamage_Damage", "ESS_FineAndDamage_Damage", new { Area = "ESS" });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message.ToString();
            }
            return RedirectToAction("ESS_FineAndDamage_Damage", "ESS_FineAndDamage_Damage", new { Area = "ESS" });
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 222;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_DamageID_Encrypted", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_Damage", param).ToList();
                ViewBag.GetDamageList = data;
                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_FineAndDamage_Damage");
            }
        }
        #endregion

        #region Delete 
        public ActionResult ConferenceBookingDelete(string DamageID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_DamageID_Encrypted", DamageID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_Damage", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_FineAndDamage_Damage", new { Area = "ESS" });
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_FineAndDamage_Damage ");
            }

        }
        #endregion
    }
}