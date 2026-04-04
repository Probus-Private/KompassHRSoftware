using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.Setting.Models.Setting_TicketManagement;
using System.Net;
using System.Data;

namespace KompassHR.Areas.Setting.Controllers.Setting_TicketManagement
{
    public class Setting_TicketManagement_TicketMatrixController : Controller
    {
        // GET: Setting/Setting_TicketManagement_TicketMatrix
        public ActionResult Setting_TicketManagement_TicketMatrix(string TicketMatrixID_Encrypted)
        {
            try
            {
                ViewBag.AddUpdateTitle = "Add";
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 281;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("Query", "select TicketCategoryID as Id,TicketCategoryName as Name from Ticket_Category where Deactivate=0 order by  TicketCategoryName");
                    var GetCategoryId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                    ViewBag.GetTicketCategory = GetCategoryId;

                    //DynamicParameters param4 = new DynamicParameters();
                    //param4.Add("Query", "select TicketSubCategoryID as Id,TicketSubCategoryName as Name  from Ticket_SubCategory WHERE DEACTIVATE=0");
                    //var GetSubCategoryId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                    ViewBag.GetTicketSubCategory = "";

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("Query", "select DepartmentId as Id,DepartmentName as Name from Mas_Department where Deactivate=0 order by Name");
                    var GetDepartmentId = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param1).ToList();
                    ViewBag.GetDepartment = GetDepartmentId;

                    DynamicParameters paramTAT = new DynamicParameters();
                    paramTAT.Add("Query", "SELECT TicketLevel_Id AS Id ,TAT AS Name  FROM Ticket_LevelTAT where Deactivate=0 order by Name");
                    var GetTAT = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTAT).ToList();
                    ViewBag.GetTAT = GetTAT;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("Query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee where  Deactivate=0 and employeeleft=0 and contractorid=1 and employeeid<>1 order by Name");
                    var GetEmployeeID = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param2).ToList();
                    ViewBag.getEmployeeID = GetEmployeeID;

                    Ticket_Matrix TicketMatrix = new Ticket_Matrix();


                    if (TicketMatrixID_Encrypted != null)
                    {
                        ViewBag.AddUpdateTitle = "Update";
                        DynamicParameters param3 = new DynamicParameters();
                        param3.Add("@p_TicketMatrixId_Encrypted", TicketMatrixID_Encrypted);
                        TicketMatrix = DapperORM.ReturnList<Ticket_Matrix>("sp_List_Ticket_Matrix", param3).FirstOrDefault();

                        if (TicketMatrix.TicketMatrixKPICategoryId > 0)
                        {
                            DynamicParameters param4 = new DynamicParameters();
                            param4.Add("Query", "select TicketSubCategoryID as Id,TicketSubCategoryName as Name  from Ticket_SubCategory where deactivate=0 and TicketCategory_Id='" + TicketMatrix.TicketMatrixKPICategoryId + "'");
                            var TicketSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                            ViewBag.GetTicketSubCategory = TicketSubCategory;

                            bool isAnyLevelChecked =TicketMatrix.chkAllLevel1 || TicketMatrix.chkAllLevel2 || TicketMatrix.chkAllLevel3 || TicketMatrix.chkAllLevel4;
                            if (isAnyLevelChecked)
                            { 
                                DynamicParameters paramallemp = new DynamicParameters();
                                paramallemp.Add("Query", "select EmployeeId as Id,EmployeeName as Name from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID = 1 order by Name");
                                var GetAllEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramallemp).ToList();
                                ViewBag.getEmployeeID = GetAllEmployee;
                            }
                            else
                            {
                                DynamicParameters paramemp = new DynamicParameters();
                                paramemp.Add("Query", "select EmployeeId as Id,EmployeeName as Name from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID = 1 and EmployeeDepartmentID='" + TicketMatrix.DepartmentID + "' order by Name");
                                var GetEmployee = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramemp).ToList();
                                ViewBag.getEmployeeID = GetEmployee;
                            }
                        }
                    }
                    return View(TicketMatrix);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult GetEmployee(int DepartmentID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("Query", "select EmployeeId as Id,Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID = 1 and EmployeeDepartmentID='" + DepartmentID + "' order by Name");
                var GetEmployeeID = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                var result = GetEmployeeID;
                var data = new { result };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult GetAllEmployees()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("Query", "select EmployeeId as Id,Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee where Deactivate=0 and EmployeeLeft=0 and ContractorID = 1 order by Name");
                var GetEmployeeID = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                var result = GetEmployeeID;
                var data = new { result };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        public ActionResult GetSubCategory(int CategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("Query", "select TicketSubCategoryID as Id,TicketSubCategoryName as Name  from Ticket_SubCategory where deactivate=0 and TicketCategory_Id='" + CategoryId + "'");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region Validation
        public ActionResult IsResponserEmployeeExists(string TicketMatrixID_Encrypted, double? ddlTicketCategoryID, string txtTicketTypeName, string txtTicketMatrixUnit,
         string txtTicketMatrixTAT, double? ddlDepartmentID, double? ddlResponsibleEmployeeID, int? TicketSubCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_TicketMatrixId_Encrypted", TicketMatrixID_Encrypted);
                    param.Add("@p_TicketMatrixKPICategoryId", ddlTicketCategoryID);
                    param.Add("@p_TicketSubCategoryId", TicketSubCategoryId);

                    param.Add("@p_TicketType", txtTicketTypeName);
                    param.Add("@p_TicketMatrixReponsibleEmployeeId", ddlResponsibleEmployeeID);
                    param.Add("@p_DepartmentID", ddlDepartmentID);
                    param.Add("@p_TicketMatrixUnit", txtTicketMatrixUnit);
                    param.Add("@p_TicketMatrixTAT", txtTicketMatrixTAT);

                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Matrix", param);
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

        #endregion


        public ActionResult SaveUpdate(Ticket_Matrix TicketMatrix)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_process", string.IsNullOrEmpty(TicketMatrix.TicketMatrixID_Encrypted) ? "Save" : "Update");
                    param.Add("@p_TicketMatrixId", TicketMatrix.TicketMatrixID);
                    param.Add("@p_TicketMatrixId_Encrypted", string.IsNullOrWhiteSpace(TicketMatrix.TicketMatrixID_Encrypted) ? null : TicketMatrix.TicketMatrixID_Encrypted);
                    param.Add("@p_CmpID", Session["CompanyId"]);
                    param.Add("@p_TicketMatrixKPICategoryId", TicketMatrix.TicketMatrixKPICategoryId);
                    param.Add("@p_TicketType", TicketMatrix.TicketType);
                    param.Add("@p_TicketMatrixReponsibleEmployeeId", TicketMatrix.TicketMatrixReponsibleEmployeeId);
                    param.Add("@p_DepartmentID", TicketMatrix.DepartmentID);
                    param.Add("@p_TicketMatrixUnit", TicketMatrix.TicketMatrixUnit);
                    param.Add("@p_TicketMatrixTAT", TicketMatrix.TicketMatrixTAT);
                    param.Add("@p_TicketSubCategoryId", TicketMatrix.TicketSubCategoryId);
                    param.Add("@p_KeyPoints", TicketMatrix.KeyPoints);


                param.Add("@p_Level1Employee", TicketMatrix.Level1Employee);
                   // param.Add("@p_Level1TAT", TicketMatrix.Level1TAT);
                    param.Add("@p_Level1Status", TicketMatrix.Level1Status);
                    param.Add("@p_chkAllLevel1", TicketMatrix.chkAllLevel1);

                    param.Add("@p_Level2Employee", TicketMatrix.Level2Employee);
                    //param.Add("@p_Level2TAT", TicketMatrix.Level2TAT);
                    param.Add("@p_Level2Status", TicketMatrix.Level2Status);
                    param.Add("@p_chkAllLevel2", TicketMatrix.chkAllLevel2);

                    param.Add("@p_Level3Employee", TicketMatrix.Level3Employee);
                  //  param.Add("@p_Level3TAT", TicketMatrix.Level3TAT);
                    param.Add("@p_Level3Status", TicketMatrix.Level3Status);
                    param.Add("@p_chkAllLevel3", TicketMatrix.chkAllLevel3);

                    param.Add("@p_Level4Employee", TicketMatrix.Level4Employee);
                   // param.Add("@p_Level4TAT", TicketMatrix.Level4TAT);
                    param.Add("@p_Level4Status", TicketMatrix.Level4Status);
                    param.Add("@p_chkAllLevel4", TicketMatrix.chkAllLevel4);

                    
                    param.Add("@p_Critical_ResponseTime", TicketMatrix.Critical_ResponseTime);
                    param.Add("@p_Critical_ResolutionTime", TicketMatrix.Critical_ResolutionTime);

                    param.Add("@p_High_ResponseTime", TicketMatrix.High_ResponseTime);
                    param.Add("@p_High_ResolutionTime", TicketMatrix.High_ResolutionTime);

                    param.Add("@p_Medium_ResponseTime", TicketMatrix.Medium_ResponseTime);
                    param.Add("@p_Medium_ResolutionTime", TicketMatrix.Medium_ResolutionTime);

                    param.Add("@p_Low_ResponseTime", TicketMatrix.Low_ResponseTime);
                    param.Add("@p_Low_ResolutionTime", TicketMatrix.Low_ResolutionTime);


                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

              
                   var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Matrix", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
               // }
             

                return RedirectToAction("Setting_TicketManagement_TicketMatrix", "Setting_TicketManagement_TicketMatrix");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 281;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "List");
                var result = DapperORM.ReturnList<dynamic>("sp_List_Ticket_Matrix", param).ToList();
                ViewBag.GetTicketMatrixList = result;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult Delete(string TicketMatrixId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", "Delete");
                param.Add("@p_TicketMatrixId_Encrypted", TicketMatrixId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Matrix", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_TicketManagement_TicketMatrix");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region GetTaskSubCategory
        [HttpGet]
        public ActionResult GetTaskSubCategory(int TaskCategoryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramTaskSubCategory = new DynamicParameters();
                paramTaskSubCategory.Add("@query", "SELECT TaskSubCategoryId as Id, TaskSubCategoryName as Name FROM TMS_TaskSubCategory WHERE Deactivate = 0 and TaskCategoryId ='" + TaskCategoryId + "' ORDER BY Name");
                var TaskSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramTaskSubCategory).ToList();
                return Json(new { TaskSubCategory = TaskSubCategory }, JsonRequestBehavior.AllowGet);
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