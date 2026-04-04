using Dapper;
using KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary;
using KompassHR.Areas.Setting.Models.Setting_TimeOffice;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_PolicyAndLibrary
{
    public class Setting_PolicyAndLibrary_PolicyLibraryController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        #region Setting_PolicyAndLibrary_PolicyLibrary
        [HttpGet]
        public ActionResult Setting_PolicyAndLibrary_PolicyLibrary(string PolicyLibraryId_Encrypted, int? CmpID)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";


                Mas_PolicyLibrary mas_policyLiabrary = new Mas_PolicyLibrary();

                param.Add("@query", "Select  CompanyId, CompanyName from Mas_CompanyProfile where Deactivate=0 order by CompanyName");
                var GetCompanyName = DapperORM.ReturnList<Models.Setting_TimeOffice.Mas_CompanyProfile>("sp_QueryExcution", param).ToList();
                ViewBag.CompanyName = GetCompanyName;

                param.Add("@query", "Select PolicyID,PolicyName From Mas_Policy where Deactivate=0");
                var maspolicy = DapperORM.ReturnList<Mas_Policy>("sp_QueryExcution", param).ToList();
                ViewBag.maspolicyList = maspolicy;

                param = new DynamicParameters();
                if (CmpID != null)
                {
                    param.Add("@query", "Select  BranchId , BranchName from Mas_Branch where Deactivate=0 and CmpId=" + CmpID + "");
                    var GetsLocation = DapperORM.ReturnList<Mas_Branch>("sp_QueryExcution", param).ToList();
                    ViewBag.Location = GetsLocation;
                }
                else
                {
                    ViewBag.Location = "";
                }
                if (PolicyLibraryId_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_PolicyLibraryId_Encrypted", PolicyLibraryId_Encrypted);
                    mas_policyLiabrary = DapperORM.ReturnList<Mas_PolicyLibrary>("sp_List_Mas_PolicyLibrary", param).FirstOrDefault();
                    TempData["FileType"] = mas_policyLiabrary.FileType;
                    TempData["FileName"] = mas_policyLiabrary.DocumentPath;
                    Session["SelectedFile"] = mas_policyLiabrary.DocumentPath;
                }

                return View(mas_policyLiabrary);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsExist
        [HttpGet]
        public ActionResult IsExist(int CmpId, int PolicyId, int BusinessUnit, string Remark, string URL, string PolicyLiabraryId_Encrypted,string FileType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                try
                {

                    if(FileType == "URL")
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(URL);
                    }
                }
                catch
                {
                    return Json(new { Message = "Please enter valid URL", Icon= "error" }, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@p_process", "IsValidation");
                    param.Add("@p_CmpId", CmpId);
                    param.Add("@p_PolicyLiabraryPolicyId", PolicyId);
                    param.Add("@p_PolicyLiabraryBranchId", BusinessUnit);
                    param.Add("@p_Remark", Remark);
                    param.Add("@p_PolicyLiabraryId_Encrypted", PolicyLiabraryId_Encrypted);
                    param.Add("@p_MachineName", Dns.GetHostName().ToString());
                    param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                    param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyLibrary", param);
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

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_PolicyLibrary Mas_PolicyLibrary, HttpPostedFileBase Image)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var EmpId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Mas_PolicyLibrary.PolicyLibraryId_Encrypted) ? "Save" : "Update");
                param.Add("@p_PolicyLiabraryId", Mas_PolicyLibrary.PolicyLibraryId);
                param.Add("@p_PolicyLiabraryId_Encrypted", Mas_PolicyLibrary.PolicyLibraryId_Encrypted);
                param.Add("@p_CmpID", Mas_PolicyLibrary.CmpID);
                param.Add("@p_PolicyLiabraryBranchId", Mas_PolicyLibrary.PolicyLibraryBranchId);
                param.Add("@p_PolicyLiabraryPolicyId", Mas_PolicyLibrary.PolicyLibraryPolicyId);
                param.Add("@p_FileType", Mas_PolicyLibrary.FileType);
                param.Add("@p_Remark", Mas_PolicyLibrary.Remark);
                if (Mas_PolicyLibrary.FileType == "URL")
                {
                    param.Add("@p_DocumentPath", Mas_PolicyLibrary.DocumentPath);
                }
                else
                {
                    if (Mas_PolicyLibrary.DocumentPath != null && Image == null)
                    {
                        param.Add("@p_DocumentPath", Session["SelectedFile"]);
                    }
                    else
                    {
                        param.Add("@p_DocumentPath", Image == null ? "" : Image.FileName);
                    }
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyLibrary", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && Mas_PolicyLibrary.PolicyLibraryId_Encrypted != null || Image != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='PolicyLibrary'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\"; // First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (Image != null)
                    {
                        string ImgTotalKMFullPath = "";
                        ImgTotalKMFullPath = FirstPath + Image.FileName; //Concat Full Path and create New full Path
                        Image.SaveAs(ImgTotalKMFullPath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("Setting_PolicyAndLibrary_PolicyLibrary", "Setting_PolicyAndLibrary_PolicyLibrary", new { Area = "Setting" });
            }

            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
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
                param.Add("@p_PolicyLibraryId_Encrypted", "List");
                ViewBag.CompanyPolicyList = DapperORM.DynamicList("sp_List_Mas_PolicyLibrary", param);

                //Comming Form Download Action
                ViewBag.CheckFilePath = TempData["Message"];

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(string PolicyLibraryId_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Delete");
                param.Add("@p_PolicyLiabraryId_Encrypted", PolicyLibraryId_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_PolicyLibrary", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyLibrary");
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
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
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

        #region Download Image 
        public ActionResult DownloadFile(string EventFile, int? PolicyLibraryId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                //var GetPathCount = DapperORM.DynamicQuerySingle("Select DocumentPath from Mas_PolicyLibrary where PolicyLibraryId=" + PolicyLibraryId + "");

                var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='PolicyLibrary'");
                var GetFirstPath = GetDocPath.DocInitialPath; // e.g., "C:\\ProbusSoftwarePvtLtd\\Documents\\PolicyLibrary\\"
                var FullPath = Path.Combine(GetFirstPath, PolicyLibraryId.ToString(), EventFile);

                if (!System.IO.File.Exists(FullPath))
                {
                    TempData["Message"] = "File Not Found";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyLibrary", new { Area = "Setting" });
                }
                else
                {
                    var fileBytes = System.IO.File.ReadAllBytes(FullPath);
                    var fileName = Path.GetFileName(FullPath);

                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }



                //if (FullPath != "")
                //{
                //    if (EventFile != null)
                //    {
                //        System.IO.File.ReadAllBytes(EventFile);
                //        return File(EventFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(EventFile));
                //    }
                //    else
                //    {
                //        return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyLibrary", new { Area = "Setting" });
                //    }
                //}
                //else
                //{
                //    TempData["Message"] = "File not available";
                //    //return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyLibrary", new { Area = "Setting" });
                //    return RedirectToAction("GetList", "Setting_PolicyAndLibrary_PolicyLibrary");
                //}
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