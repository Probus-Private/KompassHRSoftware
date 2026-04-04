using Dapper;
using KompassHR.Areas.Setting.Models.Setting_Payroll;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Payroll
{
    public class Setting_Payroll_HeadMappingController : Controller
    {
        // GET: Setting/Setting_Payroll_HeadMapping
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);

        public ActionResult Setting_Payroll_HeadMapping(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                BulkAccessClass Obj = new BulkAccessClass();
                ViewBag.CompanyName = Obj.GetCompanyName();

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@query", "Select HeadId as Id , HeadName as Name from Payroll_Master_Head where Deactivate=0 and IsActive=1");
                ViewBag.HeadName = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramCount).ToList();

                ViewBag.AddUpdateTitle = "Add";
                Payroll_HeadMapping ObjMapHead = new Payroll_HeadMapping();
                if (EncryptedId != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_EncryptedId", EncryptedId);
                    ObjMapHead = DapperORM.ReturnList<Payroll_HeadMapping>("sp_List_Payroll_HeadMapping", param).FirstOrDefault();
                    return View(ObjMapHead);
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpPost]
        public ActionResult IsExists(string EncryptedId, string CompanyId, string HeadId, string ColumnName)
        {
            try
            {
                //var GetAllParam = JsonConvert.DeserializeObject<dynamic>(AllParam);
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_EncryptedId", EncryptedId);
                    param.Add("@p_HeadId", HeadId);
                    param.Add("@p_CompanyId", CompanyId);
                    param.Add("@p_ColumnName", ColumnName);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadMapping", param);
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
        public ActionResult SaveUpdate(Payroll_HeadMapping tblObj, string EncryptedId, string CompanyId, string HeadId, string ColumnName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(tblObj.EncryptedId) ? "Save" : "Update");
                param.Add("@p_HeadId", HeadId);
                param.Add("@p_EncryptedId", tblObj.EncryptedId);
                param.Add("@p_CompanyId", CompanyId);
                param.Add("@p_ColumnName", tblObj.ColumnName);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                //return RedirectToAction("Setting_Payroll_HeadMapping", "Setting_Payroll_HeadMapping");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetAllMappings(int? CompanyId)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    //var query = $@"Select MC.CompanyName,PH.HeadName,PM.ColumnName from Payroll_HeadMapping PM
                    //            inner Join Payroll_Master_Head PH On PH.HeadId = PM.Mapping_HeadId
                    //            inner Join Mas_CompanyProfile MC On MC.CompanyId = PM.Mapping_CompanyId
                    //            Where PM.Mapping_CompanyId= {CompanyId} and PH.Deactivate=0"; // Adjust table name if needed.
                    var query = $@"SELECT PM.EncryptedId
				                ,MC.CompanyName
				                ,PH.HeadName
				                ,CASE 
					                WHEN PH.EarningDeductionType = '1'
						                THEN 'Earning'
					                WHEN PH.EarningDeductionType = '2'
						                THEN 'Deduction'
					                ELSE 'Unknown'
					                END AS EarningDeductionType
			                FROM Payroll_HeadMapping PM
			                FULL OUTER JOIN Payroll_Master_Head PH ON PH.HeadId = PM.Mapping_HeadId
			                FULL OUTER JOIN Mas_CompanyProfile MC ON MC.CompanyId = PM.Mapping_CompanyId
			                WHERE PH.Deactivate = 0 and PM.Mapping_CompanyId= {CompanyId}";
                    var mappings = DapperORM.DynamicQueryList(query).ToList();
                    string jsonString = JsonConvert.SerializeObject(mappings);
                    var deserializedMappings = JsonConvert.DeserializeObject<List<dynamic>>(jsonString);
                    return Json(jsonString, JsonRequestBehavior.AllowGet);
                }
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
                param.Add("@p_EncryptedId", "List");
                var data = DapperORM.ExecuteSP<dynamic>("sp_List_Payroll_HeadMapping", param).ToList();
                ViewBag.HeadMapping = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string EncryptedId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_EncryptedId", EncryptedId);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Payroll_HeadMapping", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_Payroll_HeadMapping");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        [HttpPost]
        public ActionResult SaveUpdateHeadMapping(string CompanyId, string HeadId, string ColumnName)
        {
            try
            {
                //var GetAllParam = JsonConvert.DeserializeObject<dynamic>(AllParam);
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    // Check if the record already exists
                    var existsQuery = @"SELECT COUNT(1) 
                                    FROM Payroll_HeadMapping 
                                    WHERE CompanyId = @CompanyId AND HeadId = @HeadId";
                    bool recordExists = sqlcon.ExecuteScalar<int>(existsQuery, new { CompanyId, HeadId }) > 0;

                    if (recordExists)
                    {
                        return Json("Exist", JsonRequestBehavior.AllowGet);
                    }

                    // If the record doesn't exist, perform the insert
                    var insertQuery = @"INSERT INTO Payroll_HeadMapping (CompanyId, HeadId, ColumnName) 
                                        VALUES (@CompanyId, @HeadId, @ColumnName)";

                    int rowsAffected = sqlcon.Execute(insertQuery, new { CompanyId, HeadId, ColumnName });

                    if (rowsAffected > 0)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }

                    return Json("Insert failed", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        

    }
}
