using Dapper;
using KompassHR.Areas.ESS.Models.ESS_HRSpace;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_HRSpace;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Text.RegularExpressions;


namespace KompassHR.Areas.ESS.Controllers.ESS_HRSpace
{
    public class ESS_HRSpace_IssuanceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_HRSpace_Issuance

        #region MainView
        public ActionResult ESS_HRSpace_Issuance(int? CmpId, int? BranchId, int? LetterId, int? Letter_EmployeeId, DateTime? ClosingDate, string Letter_DetailId_Encrypeted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 684;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;



                ViewBag.GetLetterList = "";
                ViewBag.GetEmployeeList = "";
                ViewBag.GetBranchName = "";

                HRSpace_LetterDetails model = new HRSpace_LetterDetails();
                if (Letter_DetailId_Encrypeted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_Letter_Encrypted", Letter_DetailId_Encrypeted);
                    param.Add("@p_CompanyId", CmpId);
                    model = DapperORM.ReturnList<HRSpace_LetterDetails>("sp_List_HRSpace_LetterIssuance", param).FirstOrDefault();
                }
                ViewBag.BranchId = model.BranchId;
                ViewBag.EmployeeId = Letter_EmployeeId;
                ViewBag.ClosingDate = model.ClosingDate;
                //HRSpace_Letter letterModel = new HRSpace_Letter();
                string FinalLetter = null;

                if (CmpId != null)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + model.BranchId + "' Order By EmployeeName");
                    ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "Select LetterId as Id,LetterName  As Name  from HRSpace_Letter where Deactivate=0 and CmpId='" + CmpId + "'");
                    ViewBag.GetLetterList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param2).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_employeeid", Session["EmployeeId"]);
                    param3.Add("@p_CmpId", CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param3).ToList();
                }
                return View(model);
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
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_employeeid", Session["EmployeeId"]);
                param.Add("@p_CmpId", CmpId);
                var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                param = new DynamicParameters();
                param.Add("@query", "Select LetterId as Id,LetterName  As Name  from HRSpace_Letter where Deactivate=0 and CmpId='" + CmpId + "'");
                var GetLetterList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param).ToList();

                return Json(new { BranchName, GetLetterList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetEmployee
        [HttpGet]
        public ActionResult GetEmployee(int ddlBranchId, int CmpId,string LetterName)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                List<AllDropDownClass> GetEmployeeList;

                bool isRelieving = Regex.IsMatch(LetterName ?? "",@"\bRelieving\W*Letter\b",RegexOptions.IgnoreCase);
                if (isRelieving)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + ddlBranchId + "' and CmpID='" + CmpId + "' Order By EmployeeName");
                     GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                }
                else
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + ddlBranchId + "' and CmpID='" + CmpId + "' Order By EmployeeName");
                     GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                }

                return Json(new { GetEmployeeList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetLetter

        public ActionResult GetLetter1(int? CmpId, int? LetterId, int? BranchId, int? Letter_EmployeeId, DateTime? ClosingDate)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                string FinalLetter = null;
                ViewBag.GetLetterList = null;
                ViewBag.GetBranchName = "";
                if (CmpId != null)
                {
                    ViewBag.ClosingDate = ClosingDate;
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name  from Mas_Employee where Deactivate=0  and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + BranchId + "' Order By EmployeeName");
                    ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "Select LetterId as Id,LetterName  As Name  from HRSpace_Letter where Deactivate=0 and CmpId='" + CmpId + "'");
                    ViewBag.GetLetterList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param2).ToList();

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("@p_employeeid", Session["EmployeeId"]);
                    param3.Add("@p_CmpId", CmpId);
                    ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param3).ToList();

                    // 1. Get Letter Template
                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("@query", "SELECT * FROM HRSpace_Letter WHERE Deactivate = 0 AND CmpId = '" + CmpId + "' AND LetterId = '" + LetterId + "'");
                    var letterResult = DapperORM.ReturnList<HRSpace_Letter>("sp_QueryExcution", param4).FirstOrDefault();

                    DynamicParameters param5 = new DynamicParameters();
                    param5.Add("@query", "SELECT CompanyName AS CompanyName FROM Mas_CompanyProfile WHERE CompanyId = '" + CmpId + "'");
                    var Company = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param5).FirstOrDefault();

                    DynamicParameters param6 = new DynamicParameters();
                    param6.Add("@p_Employeeid", Letter_EmployeeId);
                    var info = DapperORM.ExecuteSP<dynamic>("sp_GetEmployeeDetails", param6).FirstOrDefault();

                    string CompanyName = Company?.CompanyName ?? "";
                    string EmployeeName = info?.EmployeeName ?? "";
                    string EmployeeCode = info?.EmployeeNo ?? "";
                    string EmployeeDesignation = info?.DesignationName ?? "";
                    string EmployeeDepartment = info?.DepartmentName ?? "";
                    string JoiningDate = info?.JoiningDate != null
                        ? Convert.ToDateTime(info.JoiningDate).ToString("dd-MMM-yyyy")
                        : "";

                    string Description = letterResult?.Letter_Description ?? "";

                    var rawTokenMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Company Name", CompanyName },
                        { "Employee Name", EmployeeName },
                        { "Employee No", EmployeeCode },
                        { "Employee Designation", EmployeeDesignation },
                        { "Employee Department", EmployeeDepartment },
                        { "Date of Joining", JoiningDate }
                    };

                    Func<string, string> NormalizeKey = s =>
                    {
                        if (string.IsNullOrEmpty(s)) return "";
                        s = Regex.Replace(s, "<.*?>", "");
                        s = s.Replace("&nbsp;", " ");
                        s = Regex.Replace(s, @"\s+|[^A-Za-z0-9_]", "");
                        return s.ToLowerInvariant();
                    };

                    var braceRegex = new Regex(@"\{(?<inner>.*?)\}", RegexOptions.Singleline | RegexOptions.Compiled);
                    FinalLetter = braceRegex.Replace(Description, match =>
                    {
                        var inner = match.Groups["inner"].Value.Trim();
                        var plain = Regex.Replace(inner, "<.*?>", "").Replace("&nbsp;", " ").Trim();
                        var lookupKey = NormalizeKey(plain);

                        string replacementValue = null;
                        foreach (var kv in rawTokenMap)
                        {
                            if (NormalizeKey(kv.Key).Equals(lookupKey, StringComparison.OrdinalIgnoreCase))
                            {
                                replacementValue = kv.Value ?? "";
                                break;
                            }
                        }

                        if (replacementValue == null)
                        {
                            return match.Value;
                        }

                        var startTag = Regex.Match(inner, @"^\s*(<(?<tag>[a-zA-Z0-9]+)(\s[^>]*?)?>)");
                        var endTag = Regex.Match(inner, @"(</(?<tag2>[a-zA-Z0-9]+)>)\s*$");
                        if (startTag.Success && endTag.Success &&
                            startTag.Groups["tag"].Value.Equals(endTag.Groups["tag2"].Value, StringComparison.OrdinalIgnoreCase))
                        {
                            return startTag.Value + replacementValue + endTag.Value;
                        }
                        return replacementValue;
                    });
                }
                HRSpace_LetterDetails letterModel = new HRSpace_LetterDetails
                {
                    Description = FinalLetter
                };
                return View("ESS_HRSpace_Issuance", letterModel);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }        

        public ActionResult GetLetter(int? CmpId, int? LetterId, int? BranchId, int? Letter_EmployeeId, DateTime? ClosingDate,string Letter_DetailId_Encrypeted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                    return RedirectToAction("Login", "Login", new { Area = "" });

                if (CmpId == null)
                    return RedirectToAction("ESS_HRSpace_Issuance");

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                ViewBag.ClosingDate = ClosingDate;
                // ViewBag.EmployeeId = Letter_EmployeeId;

                // Your dropdowns (keep exactly as you have)
               
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@query", "Select LetterId as Id,LetterName As Name from HRSpace_Letter where Deactivate=0 and CmpId='" + CmpId + "'");
                ViewBag.GetLetterList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param2).ToList();

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_employeeid", Session["EmployeeId"]);
                param3.Add("@p_CmpId", CmpId);
                ViewBag.GetBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param3).ToList();

                DynamicParameters param4 = new DynamicParameters();
                param4.Add("@query", "Select LetterName As Name from HRSpace_Letter where Deactivate=0 and LetterId='" + LetterId + "'");
                var LetterName= DapperORM.ReturnList<dynamic>("sp_QueryExcution", param4).FirstOrDefault();

                bool isRelieving = Regex.IsMatch(LetterName.Name ?? "", @"\bRelieving\W*Letter\b", RegexOptions.IgnoreCase);
                if (isRelieving)
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name from Mas_Employee where Deactivate=0 and Employeeleft=1 and ContractorID=1 and EmployeeBranchId='" + BranchId + "' Order By EmployeeName");
                    ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                }
                else
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "Select EmployeeId as Id, Concat (EmployeeName,'-',EmployeeNo) As Name from Mas_Employee where Deactivate=0 and Employeeleft=0 and ContractorID=1 and EmployeeBranchId='" + BranchId + "' Order By EmployeeName");
                    ViewBag.GetEmployeeList = DapperORM.ReturnList<AllDropDownClass>("sp_QueryExcution", param1).ToList();
                }

                var paramp = new DynamicParameters();
                paramp.Add("@p_EmployeeId", Letter_EmployeeId);
                paramp.Add("@p_CompanyId", CmpId);
                paramp.Add("@p_LetterId", LetterId);

                paramp.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                paramp.Add("@p_icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 20);  // ← was 200, now 20 (correct)

                dynamic result = null;
                if (LetterName.Name.Contains("Appraisal Letter"))
                {
                     result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_AppraisalLetter", paramp).FirstOrDefault();
                }
                else if(LetterName.Name.Contains("Relieving"))
                {
                     result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_RelievingLetter", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Appointment"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_AppointmentLetter", paramp).FirstOrDefault();                 
                }
                else if (LetterName.Name.Contains("Letter of Intent"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_LetterOfIntent", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Confirmation Letter"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_LetterOfConfirmation", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Experience"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_ExperienceLetter", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Termination Letter"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_TerminationLetter", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Warning Letter"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_WarningLetter", paramp).FirstOrDefault();
                }
                else if (LetterName.Name.Contains("Caution Letter"))
                {
                    result = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_CautionLetter", paramp).FirstOrDefault();
                }
                string Letter = result?.Letter?.ToString() ?? "";
                // Letter = Regex.Replace(Letter, @"<tr>\s*<td.*?>\s*(<br>|&nbsp;)?\s*</td>\s*</tr>", "", RegexOptions.IgnoreCase);
                //Letter=  Regex.Replace(Letter, @"<tr>\s*(<td[^>]*>\s*(?:<br>|&nbsp;|\s)*\s*</td>\s*)+</tr>","",RegexOptions.IgnoreCase);
                // Letter = Letter.Replace("<tbody>", "").Replace("</tbody>", "");

                // Remove fully empty rows only
                Letter = Regex.Replace(Letter,
    @"<tr[^>]*>\s*(<td[^>]*>\s*(?:&nbsp;|<br>|<p><br></p>|\s)*</td>\s*)+</tr>",
    "",
    RegexOptions.IgnoreCase);

                // Now inject dynamic content
                var letterModel = new HRSpace_LetterDetails
                {
                    Description = Letter,
                    Letter_EmployeeId= Convert.ToInt32(Letter_EmployeeId),
                    Letter_DetailId_Encrypeted= Letter_DetailId_Encrypeted
                };
                return View("ESS_HRSpace_Issuance", letterModel);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region 
        
        public ActionResult Signature(string filePath)
        {
            try
            {
                byte[] imageBytes;
                string contentType;

                using (Image image = Image.FromFile(filePath))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        imageBytes = m.ToArray();

                        string fileName = Path.GetFileName(filePath);
                        contentType = MimeMapping.GetMimeMapping(fileName);
                    }
                }

                // Return the raw bytes of the image
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error opening file: " + ex.Message);
            }
        }

        #endregion

        #region SaveUpdate
        [ValidateInput(false)]
        public ActionResult SaveUpdate(string Letter_DetailId_Encrypeted, HRSpace_LetterDetails det)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(det.Letter_DetailId_Encrypeted) ? "Save" : "Update");
                param.Add("@p_Letter_DetailId_Encrypeted", det.Letter_DetailId_Encrypeted);
                param.Add("@p_CmpId", det.CmpId);
                param.Add("@p_BranchId", det.BranchId);
                param.Add("@p_LetterId", det.LetterId);
                param.Add("@p_Description", det.Description);
                param.Add("@p_Letter_EmployeeId", det.Letter_EmployeeId);
                param.Add("@p_ClosingDate", det.ClosingDate);
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_HRSpace_LetterDetails", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                //return RedirectToAction("ESS_HRSpace_Issuance", "ESS_HRSpace_Issuance");
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], Id = TempData["P_Id"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error: " + ex.Message, Icon = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion SaveUpdate

        #region GetList
        public ActionResult GetList(int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 684;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var GetComapnyName = new BulkAccessClass().GetCompanyName();
                ViewBag.CompanyName = GetComapnyName;
                param.Add("@p_Letter_Encrypted", "List");
                param.Add("@p_CompanyId", CmpId);
                var data = DapperORM.DynamicList("sp_List_HRSpace_LetterIssuance", param);
                ViewBag.GetLetterList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Print

        
        //public ActionResult PrintLetter1(int id)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }

        //        DynamicParameters param2 = new DynamicParameters();
        //        param2.Add("@query", "Select Letter_DetailId ,Description,LetterId from HRSpace_Letter_Details where Deactivate=0 and Letter_DetailId='" + id + "'");
        //        var data = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param2).FirstOrDefault();


        //        if (data == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        //string originalDescription = Convert.ToString(data.Description);

        //        //// THIS FIXES ALL IMAGES (1st, 2nd, 3rd, etc.)
        //        //string styledDescription = Regex.Replace(
        //        //    originalDescription,
        //        //    @"(<img\s*)([^>]*?)(\s*/?>)",
        //        //    "$1$2 style=\"max-width:80%; height:auto; display:block; margin:20px auto; border:none; box-shadow:none; padding:0;\" $3",
        //        //    RegexOptions.IgnoreCase | RegexOptions.Multiline
        //        //);

        //        //// Extra safety: also remove any existing width/height attributes
        //        //styledDescription = Regex.Replace(
        //        //    styledDescription,
        //        //    @"\s*(width|height)\s*=\s*[""'][^""']*[""']",
        //        //    "",
        //        //    RegexOptions.IgnoreCase
        //        //);

        //        //ViewBag.Description = styledDescription;

        //        string originalDescription = Convert.ToString(data.Description);

        //        /* =========================================================
        //           1️⃣ Convert WORD page break marker to HTML page break
        //           ========================================================= */
        //        string styledDescription = Regex.Replace(
        //            originalDescription,
        //            @"={3}\s*PAGE\s*BREAK\s*={3}",
        //            "<div class=\"page-break\"></div>",
        //            RegexOptions.IgnoreCase
        //        );

        //        /* =========================================================
        //           2️⃣ Fix ALL images (existing logic – unchanged)
        //           ========================================================= */
        //        styledDescription = Regex.Replace(
        //            styledDescription,
        //            @"(<img\s*)([^>]*?)(\s*/?>)",
        //            "$1$2 style=\"max-width:80%; height:auto; display:block; margin:20px auto; border:none; box-shadow:none; padding:0;\" $3",
        //            RegexOptions.IgnoreCase | RegexOptions.Multiline
        //        );

        //        /* =========================================================
        //           3️⃣ Remove width/height attributes (existing logic)
        //           ========================================================= */
        //        styledDescription = Regex.Replace(
        //            styledDescription,
        //            @"\s*(width|height)\s*=\s*[""'][^""']*[""']",
        //            "",
        //            RegexOptions.IgnoreCase
        //        );

        //        /* =========================================================
        //           4️⃣ OPTIONAL: Clean Word extra spacing (HIGHLY RECOMMENDED)
        //           ========================================================= */
        //        styledDescription = Regex.Replace(
        //            styledDescription,
        //            @"(<p[^>]*>)\s*(<br\s*/?>\s*){2,}",
        //            "$1",
        //            RegexOptions.IgnoreCase
        //        );

        //        ViewBag.Description = styledDescription;

        //        //ViewBag.LetterHeader = @"
        //        //<div style='width:100%; margin:0 auto 20px auto; padding:10px 0;
        //        //font-family:Times New Roman,Helvetica,sans-serif; line-height:1.4;'>
        //        //    <div style='display:flex; justify-content:space-between; width:100%;'>
        //        //        <div style='width:25%; display:flex; align-items:flex-start;'>
        //        //            <img src='/ESS_HRSpace_Issuance/Signature?filePath=D:\KompassHR_Document\KompassHR_Paragon\CompanyLogo\1\Paragon Final Logo.jpg'
        //        //                 style='height:80px; width:auto; max-width:160px;' />
        //        //        </div>

        //        //        <div style='width:50%; text-align:center;'>
        //        //            <div style='display:flex; align-items:center; justify-content:center;'>
        //        //                <strong style='font-size:22px; letter-spacing:1px; color:#8B0000;'>
        //        //                    ABC PVT LTD
        //        //                </strong>
        //        //            </div>
        //        //            <span style='font-size:14px; display:block; margin-top:4px; margin-bottom:28px;'>
        //        //                CIN No: (L12345MH2024)
        //        //            </span>
        //        //        </div>
        //        //        <div style='width:25%;'>&nbsp;</div>
        //        //    </div>
        //        //</div>";

        //        //ViewBag.LetterHeader = @"
        //        //<table style='width:100%; border-collapse:collapse; font-family:Times New Roman, serif;'>
        //        //    <tr>
        //        //        <!-- LOGO -->
        //        //        <td style='width:20%; vertical-align:middle;'>
        //        //            <img src='/ESS_HRSpace_Issuance/Signature?filePath=D:\KompassHR_Document\KompassHR_Paragon\CompanyLogo\1\Paragon Final Logo.jpg'
        //        //                 style='height:45px; max-width:100%; display:block;' />
        //        //        </td>

        //        //        <!-- COMPANY NAME -->
        //        //        <td style='width:60%; text-align:center; vertical-align:middle;'>
        //        //            <div style='font-size:18px; font-weight:bold; color:#8B0000;'>
        //        //                PARAGON SCM PRIVATE LIMITED 
        //        //            </div>
        //        //            <div style='font-size:11px; margin-top:2px;'>
        //        //                 CIN No: (L12345MH2024)
        //        //            </div>
        //        //        </td>

        //        //        <!-- EMPTY COLUMN -->
        //        //        <td style='width:20%;'></td>
        //        //    </tr>
        //        //</table>";

        //        DynamicParameters paramHeaderFooter = new DynamicParameters();
        //        paramHeaderFooter.Add("@query", "Select Letter_Header ,Letter_Footer  from HRSpace_Letter where Deactivate=0 and LetterId='" + data.LetterId + "'");
        //        var HeaderFooter = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramHeaderFooter).FirstOrDefault();

        //        ViewBag.LetterHeader = HeaderFooter.Letter_Header;
        //       // ViewBag.LetterFooter = @"<div style='text-align:center; margin: 40px 0 60px 0; font - family:Arial,Helvetica,sans - serif; font - size:13px; color:#444; line-height:1.6;'>
        //       // <strong> Office & factory :Gat No 213/1 & 214, A/P Mahalunge ,Talegoan -Chakan Road, Tal , Khed , Dist-Pune 410501</ strong >< br />
        //       //    Maharashtra India , tel- 9130097805, Email: nitin@paragonsolutions.co.in
        //       //</ div > ";

        //        HRSpace_LetterDetails model = new HRSpace_LetterDetails
        //        {
        //            Letter_DetailId = Convert.ToInt32(data.Letter_DetailId),
        //            Description = originalDescription
        //        };

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}

        public ActionResult PrintLetter(int id)
        {
            try
            {

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@LetterDetailId", id);
                param2.Add("@CompanyId", 1);
                var data = DapperORM.ExecuteSP<dynamic>("sp_HRSpace_Letter_PrintContent", param2).FirstOrDefault();
                

                
                string originalDescription = Convert.ToString(data.LetterBody);

                /* =========================================================
                   1️⃣ Convert WORD page break marker to HTML page break
                   ========================================================= */
                //string styledDescription = Regex.Replace(
                //    originalDescription,
                //    @"={3}\s*PAGE\s*BREAK\s*={3}",
                //    "<div class=\"page-break\"></div>",
                //    RegexOptions.IgnoreCase
                //);

                string styledDescription = Regex.Replace(
      originalDescription,
      @"={3}\s*PAGE\s*BREAK\s*={3}",
      "<div class=\"page-break\" style=\"break-after: page; page-break-after: always; height: 0; overflow: hidden; clear: both;\">&nbsp;</div>",
      RegexOptions.IgnoreCase | RegexOptions.Multiline
  );

                
                styledDescription = Regex.Replace(
                    styledDescription,
                    @"(<img\s*)([^>]*?)(\s*/?>)",
                    "$1$2 style=\"max-width:80%; height:auto; display:block; margin:20px auto; border:none; box-shadow:none; padding:0;\" $3",
                    RegexOptions.IgnoreCase | RegexOptions.Multiline
                );

                
                styledDescription = Regex.Replace(
                    styledDescription,
                    @"\s*(width|height)\s*=\s*[""'][^""']*[""']",
                    "",
                    RegexOptions.IgnoreCase
                );

                styledDescription = Regex.Replace(
                    styledDescription,
                    @"(<p[^>]*>)\s*(<br\s*/?>\s*){2,}",
                    "$1",
                    RegexOptions.IgnoreCase
                );

                ViewBag.Description = styledDescription;
                ViewBag.LetterHeader = data.LetterHeader;
                ViewBag.LetterFooter = data.LetterFooter;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion Print

        #region Delete
        public ActionResult Delete(string Letter_DetailId_Encrypeted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_Letter_DetailId_Encrypeted", Letter_DetailId_Encrypeted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_HRSpace_LetterDetails", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_HRSpace_Issuance");
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