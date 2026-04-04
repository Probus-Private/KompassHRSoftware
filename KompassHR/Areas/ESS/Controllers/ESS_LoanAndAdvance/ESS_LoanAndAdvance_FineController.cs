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

namespace KompassHR.Areas.ESS.Controllers.ESS_LoanAndAdvance
{
    public class ESS_LoanAndAdvance_FineController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_LoanAndAdvance_Fine
        #region Fine Main View
        [HttpGet]
        public ActionResult ESS_LoanAndAdvance_Fine(string PenaltyID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Payroll_FineAndPenalty FineAndPenalty = new Payroll_FineAndPenalty();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from Payroll_FineAndPenalty";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }

                param.Add("@query", "Select EmployeeId AS Id,EmployeeName As [Name] from Mas_Employee Where Deactivate=0 order by Name");
                var listMas_Employee = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", param).ToList();
                ViewBag.GetEmployeeName = listMas_Employee;

                if (PenaltyID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PenaltyID_Encrypted", PenaltyID_Encrypted);
                    FineAndPenalty = DapperORM.ReturnList<Payroll_FineAndPenalty>("sp_List_Payroll_FineAndPenalty", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from Payroll_FineAndPenalty where PenaltyID_Encrypted='" + PenaltyID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = FineAndPenalty.DocDate;
                TempData["DeducationMonth"] = FineAndPenalty.DeductionMonth;
                return View(FineAndPenalty);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Fine");
            }
        }
        #endregion

        #region IsVerification
        public JsonResult IsFineExists(double PenaltyEmployeeID, string DeductionMonth, string PenaltyID_Encrypted)
        {
            try
            {              
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_PenaltyEmployeeID", PenaltyEmployeeID);
                    param.Add("@p_PenaltyID_Encrypted", PenaltyID_Encrypted);
                    param.Add("@p_DeductionMonth", DeductionMonth);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("[sp_SUD_Payroll_FineAndPenalty]", param);
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
            catch (Exception Ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
                //return RedirectToAction(Ex.Message.ToString(), "Wage");
            }
        }

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Payroll_FineAndPenalty Fine)
        {
            try
            {
                //var CompanyId = Session["CompanyId"];
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Fine.PenaltyID_Encrypted) ? "Save" : "Update");
                param.Add("@p_PenaltyID", Fine.PenaltyID);
                param.Add("@p_PenaltyID_Encrypted", Fine.PenaltyID_Encrypted);
               // param.Add("@p_CmpID", CompanyId);
                param.Add("@p_DocNo", Fine.DocNo);
                param.Add("@p_DocDate", Fine.DocDate);
                param.Add("@p_PenaltyName", Fine.PenaltyName);
                param.Add("@p_PenaltyEmployeeID", Fine.PenaltyEmployeeID);
                param.Add("@p_PenaltyDescription", Fine.PenaltyDescription);
                param.Add("@p_DeductionMonth", Fine.DeductionMonth);
                param.Add("@p_PenaltyAmount", Fine.PenaltyAmount);

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_FineAndPenalty", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_LoanAndAdvance_Fine", "ESS_LoanAndAdvance_Fine");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Fine");
            }

        }
        #endregion

        #region fine List View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_PenaltyID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_Payroll_FineAndPenalty", param);
                ViewBag.GetFinePenaltyList = data;
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_LoanAndAdvance_Fine");
            }
        }
        #endregion

        #region Delete fine
        public ActionResult Delete(string PenaltyID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_PenaltyID_Encrypted", PenaltyID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_FineAndPenalty", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_LoanAndAdvance_Fine");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_LoanAndAdvance_Fine");
            }

        }
        #endregion
    }
}
