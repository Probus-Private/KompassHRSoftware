using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Reports.Models;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Report_ExitManagement
{
    public class Rpt_ExitManagement_FNFStatementController : Controller
    {
        // GET: Reports/Rpt_ExitManagement_FNFStatement
        public ActionResult Rpt_ExitManagement_FNFStatement(MonthWiseFilter MonthWiseFilter)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 712;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

              
                //GET COMPANY NAME
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;

                //GET BRANCH NAME
                //var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CMPID), Convert.ToInt32(Session["EmployeeId"]));
                //ViewBag.BranchName = Branch;
                ViewBag.BranchName = "";
                DynamicParameters paramemp = new DynamicParameters();
                paramemp.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and employeeLeft=0 and ContractorId=1 order by Name");
                var data7 = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramemp).ToList();
                ViewBag.AllEmployeeName = data7;
                if (MonthWiseFilter.BranchId != 0)
                {
                    if (MonthWiseFilter.EmployeeId != 0)
                    {
                        var GetCompanyName = "select Mas_CompanyProfile.CompanyName,Mas_Employee.CmpID from Mas_Employee inner join Mas_CompanyProfile on Mas_CompanyProfile.CompanyId = Mas_Employee.CmpID where Mas_CompanyProfile.Deactivate=0 and Mas_Employee.EmployeeId='" + MonthWiseFilter.EmployeeId + "'";
                        var CompanyName = DapperORM.DynamicQuerySingle(GetCompanyName);
                        ViewBag.CompanyName = CompanyName.CompanyName;
                        var CmpId = CompanyName.CmpID;


                        DynamicParameters paramfnf = new DynamicParameters();
                        paramfnf.Add("@p_CompanyId", MonthWiseFilter.CmpId);
                        paramfnf.Add("@p_BranchId", MonthWiseFilter.BranchId);
                        paramfnf.Add("@p_EmployeeId", MonthWiseFilter.EmployeeId);
                        var FNFStatementData = DapperORM.ReturnList<dynamic>("sp_Rpt_FNF_FNFStatement", paramfnf).FirstOrDefault();
                        ViewBag.GetFNFStatementData = FNFStatementData;

                        if (FNFStatementData.EmployeeCheckerStatus != "Pending")
                        {
                            //SET COMPANY LOGO COMPANY WISE
                            var path = DapperORM.DynamicQuerySingle("Select SignaturePath from Mas_Employee_Signature where Deactivate=0 and SignatureEmployeeId=" + FNFStatementData.EmployeeCheckerId + "");
                            var SecondPath = path.SignaturePath;
                            var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Onboarding'");
                            var FirstPath = GetDocPath.DocInitialPath + FNFStatementData.EmployeeCheckerId + "\\" + "Signature" + "\\";
                            // string GetBase64 = null;
                            string fullPath = "";
                            fullPath = FirstPath + SecondPath;
                            string extensionType1 = Path.GetExtension(fullPath).ToLower();
                            string mimeType1 = "image/jpeg";

                            if (extensionType1 == ".png")
                            {
                                mimeType1 = "image/png";
                            }
                            else if (extensionType1 == ".jpg" || extensionType1 == ".jpeg")
                            {
                                mimeType1 = "image/jpeg";
                            }

                            //string Extention = System.IO.Path.GetExtension(fullPath);
                            if (path.SignaturePath != null)
                            {
                                try
                                {
                                    string directoryPath = Path.GetDirectoryName(fullPath);
                                    if (!Directory.Exists(directoryPath))
                                    {
                                        Session["SignaturePath"] = "";
                                    }
                                    else
                                    {
                                        using (Image image = Image.FromFile(fullPath))
                                        {
                                            using (MemoryStream m = new MemoryStream())
                                            {
                                                image.Save(m, image.RawFormat);
                                                byte[] imageBytes = m.ToArray();

                                                // Convert byte[] to Base64 String
                                                string base64String = Convert.ToBase64String(imageBytes);
                                                Session.Remove("SignaturePath");
                                                Session["SignaturePath"] = "data:" + mimeType1 + ";base64," + base64String;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.Message != null)
                                    {
                                        Session["SignaturePath"] = "";
                                    }
                                }
                            }
                        }


                        var GetCompanyStamp = "Select Stamp from Mas_CompanyProfile Where CompanyId= " + CmpId + "";
                        var path1 = DapperORM.DynamicQuerySingle(GetCompanyStamp);
                        var SecondPath1 = path1.Stamp;

                        var GetDocPath1 = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='CompanyStamp'");
                        var FisrtPath1 = GetDocPath1.DocInitialPath + CmpId + "\\";

                        string GetBase64 = null;
                        string fullPath1 = "";
                        fullPath1 = FisrtPath1 + SecondPath1;
                        string extensionType = Path.GetExtension(fullPath1).ToLower();
                        string mimeType = "image/jpeg";

                        if (extensionType == ".png")
                        {
                            mimeType = "image/png";
                        }
                        else if (extensionType == ".jpg" || extensionType == ".jpeg")
                        {
                            mimeType = "image/jpeg";
                        }
                        if (SecondPath1 != null)
                        {
                            if (!Directory.Exists(fullPath1))
                            {
                                using (Image image = Image.FromFile(fullPath1))
                                {
                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.Save(m, image.RawFormat);
                                        byte[] imageBytes = m.ToArray();
                                        // Convert byte[] to Base64 String
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        Session.Remove("Stamp");
                                        Session["Stamp"] = "data:" + mimeType + ";base64," + base64String;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Session["Stamp"] = "";
                        }
                    }
                }
                else
                {
                    ViewBag.GetFNFStatementData = "";
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #region Get Branch Name
        [HttpGet]
        public ActionResult GetBusinessUnit(int CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                var Branch = new BulkAccessClass().GetBusinessUnit(Convert.ToInt32(CmpId), Convert.ToInt32(Session["EmployeeId"]));
                return Json(new { Branch = Branch }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetEmployeeName
        [HttpGet]
        public ActionResult GetEmployeeName(int CmpId, int? BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });

                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and  CmpId=" + CmpId + "  and ContractorId=1 order by Name");
                var data = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
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