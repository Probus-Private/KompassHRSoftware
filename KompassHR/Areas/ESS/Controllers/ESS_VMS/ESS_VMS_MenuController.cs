using Dapper;
using KompassHR.Areas.ESS.Models.ESS_VMS;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static KompassHR.Areas.ESS.Models.ESS_VMS.Visitor_Tra_Master;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_VMS_Menu
        #region ESS_VMS_Menu Main View
        [HttpGet]
        public ActionResult ESS_VMS_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 184;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                DynamicParameters paramCount = new DynamicParameters();
                paramCount.Add("@p_Employeeid", Session["EmployeeId"]);
                var GetCount = DapperORM.ExecuteSP<dynamic>("sp_VMSDasboardCount_My", paramCount).ToList(); // SP_getReportingManager
                TempData["TodayAppointment"] = GetCount[0].TodayAppointment;
                TempData["TodayInVisitor"] = GetCount[1].TodayAppointment;
                TempData["TotalOutVisitors"] = GetCount[2].TodayAppointment;
                TempData["TodayInOutVistor"] = GetCount[3].TodayAppointment;
                TempData["PersonalGatepassCount"] = GetCount[4].TodayAppointment;
                TempData["OutDoorCount"] = GetCount[5].TodayAppointment;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region InOutVisitorList View
        public ActionResult InOutVisitorList(VisitorMasterInOutList OBJVisitorMaster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 184;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                if (OBJVisitorMaster.InDateTime!=null)
                {
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@P_InDateTime", OBJVisitorMaster.InDateTime);
                    param.Add("@P_GateOutTime", OBJVisitorMaster.GateOutTime);
                    param.Add("@p_EmployeeId", Session["EmployeeId"]);
                    var GetVisitorList = DapperORM.ReturnList<dynamic>("sp_List_Visitor_InOutVisitor", param).ToList();
                    ViewBag.VisitorList = GetVisitorList;
                    if (GetVisitorList.Count == 0)
                    {
                        TempData["Message"] = "Record Not Found";
                        TempData["Icon"] = "error";
                    }
                }
                else
                {
                    ViewBag.VisitorList = "";
                }
               
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region InOutVisitorList View
        public ActionResult VisitorPassInvoice(string VisitorId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramInvoice = new DynamicParameters();
                paramInvoice.Add("@P_VisitorId_Encrypted", VisitorId_Encrypted);
                var VisitorList = DapperORM.ExecuteSP<dynamic>("sp_Visitor_InvoiceProfile", paramInvoice).FirstOrDefault();
                ViewBag.InvoiceVisitor = VisitorList;


                var paramnew = new DynamicParameters();
                paramnew.Add("@query", $"SELECT Instruction FROM Visitor_Mas_Instruction WHERE Deactivate = 0");
                ViewBag.GetInstruction = DapperORM.ExecuteSP<dynamic>("sp_QueryExcution", paramnew).ToList();
                //var instructions = DapperORM.DynamicQuerySingle("SELECT Instruction FROM Visitor_Mas_Instruction WHERE Deactivate = 0");
                //ViewBag.GetInstruction = instructions.Instruction;

                string fullPath = ViewBag.InvoiceVisitor.PhotoPath;
                //fullPath = FisrtPath + SecondPath;
                if (fullPath != "File Not Found")
                {
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (Directory.Exists(directoryPath))
                    {
                        try
                        {
                            if (!Directory.Exists(fullPath))
                            {
                                using (Image image = Image.FromFile(fullPath))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();

                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        ViewBag.UploadPhoto = "data:image; base64," + base64String;
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.UploadPhoto = "File Not Found";
                            }
                        }
                        catch (Exception)
                        {
                            ViewBag.UploadPhoto = "File Not Found";
                        }
                    }
                    else
                    {
                        ViewBag.UploadPhoto = "File Not Found";
                    }
                }
                else
                {
                    ViewBag.UploadPhoto = fullPath;
                }         
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region TodaysAppointmentVisitorEntry
        public ActionResult TodaysAppointmentVisitorEntry()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_qry", " and  VisitorAppointmentBranchID in (select mas_branch.BranchID as Id from UserBranchMapping,mas_branch where  UserBranchMapping.employeeid = " + Session["EmployeeId"] + " and mas_branch.BranchId = UserBranchMapping.branchid and mas_branch.Deactivate = 0 and mas_branch.CmpId = " + Session["CompanyId"] + " and UserBranchMapping.IsActive = 1)");
                var VisitorAppointmentList = DapperORM.DynamicList("sp_List_Visitor_TodaysAppointmet", param);
                ViewBag.VisitorAppointment = VisitorAppointmentList;
                return View();
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