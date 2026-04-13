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
    public class ESS_VMS_PersonalGatepassInvoiceReportController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_VMS_PersonalGatepassInvoiceReport
        #region ESS_VMS_PersonalGatepassInvoiceReport
        public ActionResult ESS_VMS_PersonalGatepassInvoiceReport(VisitorMasterInOutList OBJVisitorMaster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 385;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (OBJVisitorMaster.InDateTime != null)
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region InOutVisitorList View
        public ActionResult VisitorPassInvoiceReport(string VisitorId_Encrypted)
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
                var instructions = DapperORM.DynamicQueryList("SELECT Instruction FROM Visitor_Mas_Instruction WHERE Deactivate = 0").ToList();
                ViewBag.GetInstruction = instructions;
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


    }
}