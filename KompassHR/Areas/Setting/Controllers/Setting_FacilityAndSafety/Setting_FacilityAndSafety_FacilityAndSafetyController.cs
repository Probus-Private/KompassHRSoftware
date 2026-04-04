using Dapper;
using KompassHR.Areas.Setting.Models.Setting_FacilityAndSafety;
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

namespace KompassHR.Areas.Setting.Controllers.Setting_FacilityAndSafety
{
    public class Setting_FacilityAndSafety_FacilityAndSafetyController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: FacilityAndSafety/FacilityAndSafety
        #region Facility And Safety Main View 
        [HttpGet]
        public ActionResult Setting_FacilityAndSafety_FacilityAndSafety(string FacilityID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                FacilitySafety_Master mas_FacilitySafety = new FacilitySafety_Master();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetDocNo = "Select isnull(Max(DocNo),0)+1 As DocNo from FacilitySafety_Master";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;
                }
                DynamicParameters paramDropDown = new DynamicParameters();
                paramDropDown.Add("@query", "Select FacilitySafetyCategoryID AS Id,FacilitySafetyCategoryName As [Name] from FacilitySafety_Category Where Deactivate=0");
                var FamilyCategory = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramDropDown).ToList();
                ViewBag.GetFamilyCategory = FamilyCategory;
                if (FacilityID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_FacilityID_Encrypted", FacilityID_Encrypted);
                    mas_FacilitySafety = DapperORM.ReturnList<FacilitySafety_Master>("sp_List_FacilitySafety_Master", param).FirstOrDefault();

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo As DocNo from FacilitySafety_Master where FacilityID_Encrypted='" + FacilityID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        ViewBag.DocNo = DocNo;
                    }
                }
                TempData["DocDate"] = mas_FacilitySafety.DocDate;
                TempData["FileName1"] = mas_FacilitySafety.FilePathPhoto1;
                TempData["FileName2"] = mas_FacilitySafety.FilePathPhoto2;
                TempData["FileName3"] = mas_FacilitySafety.FilePathPhoto3;
                Session["SelectedFile1"] = mas_FacilitySafety.FilePathPhoto1;
                Session["SelectedFile2"] = mas_FacilitySafety.FilePathPhoto2;
                Session["SelectedFile3"] = mas_FacilitySafety.FilePathPhoto3;
                return View(mas_FacilitySafety);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region IsVerification
        //[HttpGet]
        //public JsonResult IsFacilitySafetyExists(string FacilitySafetyCategoryID, string FacilityID_Encrypted, DateTime DocDate)
        //{
        //    try
        //    {
        //        using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
        //        {
        //            param.Add("@p_process", "IsValidation");
        //            param.Add("@p_FacilitySafetyCategoryID", FacilitySafetyCategoryID);
        //            param.Add("@p_DocDate", DocDate);
        //            param.Add("@p_FacilityID_Encrypted", FacilityID_Encrypted);
        //            param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            var Result = DapperORM.ExecuteReturn("[sp_SUD_FacilitySafety_Master]", param);
        //            var Message = param.Get<string>("@p_msg");
        //            var Icon = param.Get<string>("@p_Icon");
        //            if (Message != "")
        //            {
        //                return Json(new { Message, Icon }, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                return Json(true, JsonRequestBehavior.AllowGet);
        //            }
        //        }

        //    }
        //    catch (Exception Ex)
        //    {
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //        //return RedirectToAction(Ex.Message.ToString(), "Wage");
        //    }
        //}

        #endregion

        #region Facility SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(FacilitySafety_Master FacilitySafety, HttpPostedFileBase FilePathPhoto1, HttpPostedFileBase FilePathPhoto2, HttpPostedFileBase FilePathPhoto3)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(FacilitySafety.FacilityID_Encrypted) ? "Save" : "Update");
                param.Add("@p_FacilityID", FacilitySafety.FacilityID);
                param.Add("@p_FacilityID_Encrypted", FacilitySafety.FacilityID_Encrypted);
                param.Add("@p_FacilitySafetyCategoryID", FacilitySafety.FacilitySafetyCategoryID);
                param.Add("@p_DocNo", FacilitySafety.DocNo);
                param.Add("@p_DocDate", FacilitySafety.DocDate);
                param.Add("@p_Description", FacilitySafety.Description);
                param.Add("@p_FacilityEmployeeID", EmployeeId);
                if (FacilitySafety.FacilityID_Encrypted != null && FilePathPhoto1 == null)
                {
                    param.Add("@p_FilePathPhoto1", Session["SelectedFile1"]);
                }
                else
                {
                    param.Add("@p_FilePathPhoto1", FilePathPhoto1 == null ? "" : FilePathPhoto1.FileName);
                }
                //Second File Upload
                if (FacilitySafety.FacilityID_Encrypted != null && FilePathPhoto2 == null)
                {
                    param.Add("@p_FilePathPhoto2", Session["SelectedFile2"]);
                }
                else
                {
                    param.Add("@p_FilePathPhoto2", FilePathPhoto2 == null ? "" : FilePathPhoto2.FileName);
                }
                //Third File Upload
                if (FacilitySafety.FacilityID_Encrypted != null && FilePathPhoto3 == null)
                {
                    param.Add("@p_FilePathPhoto3", Session["SelectedFile3"]);
                }
                else
                {
                    param.Add("@p_FilePathPhoto3", FilePathPhoto3 == null ? "" : FilePathPhoto3.FileName);
                }

                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_FacilitySafety_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");

                TempData["P_Id"] = param.Get<string>("@p_Id");
                if (TempData["P_Id"] != null && FacilitySafety.FacilityID_Encrypted != null || FilePathPhoto1 != null || FilePathPhoto2 != null || FilePathPhoto3 != null)
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='FacilityAndSafety'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    //var FirstPath = GetFirstPath +"Event"+"\\";
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";// First path plus concat folder by Id
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    //First File Upload
                    if (FilePathPhoto1 != null)
                    {
                        string ImgFilePath1 = "";
                        ImgFilePath1 = FirstPath + FilePathPhoto1.FileName; //Concat Full Path and create New full Path
                        FilePathPhoto1.SaveAs(ImgFilePath1); // This is use for Save image in folder full path
                    }
                    //Second File Upload
                    if (FilePathPhoto2 != null)
                    {
                        string ImgFilePath2 = "";
                        ImgFilePath2 = FirstPath + FilePathPhoto2.FileName; //Concat Full Path and create New full Path
                        FilePathPhoto2.SaveAs(ImgFilePath2); // This is use for Save image in folder full path
                    }
                    //Third File Upload
                    if (FilePathPhoto3 != null)
                    {
                        string ImgFilePath3 = "";
                        ImgFilePath3 = FirstPath + FilePathPhoto3.FileName; //Concat Full Path and create New full Path
                        FilePathPhoto3.SaveAs(ImgFilePath3); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("Setting_FacilityAndSafety_FacilityAndSafety", "Setting_FacilityAndSafety_FacilityAndSafety");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Facility And Safety List View
        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_FacilityID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_FacilitySafety_Master", param);
                ViewBag.GetFacilityAndSafetyList = data;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Download Image 
        public ActionResult DownloadFile(string FilePathPhoto1, string FilePathPhoto2, string FilePathPhoto3)
        {
            try
            {
                if (FilePathPhoto1 != null)
                {
                    System.IO.File.ReadAllBytes(FilePathPhoto1);
                    return File(FilePathPhoto1, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePathPhoto1));
                }
                else if (FilePathPhoto2 != null)
                {
                    System.IO.File.ReadAllBytes(FilePathPhoto2);
                    return File(FilePathPhoto2, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePathPhoto2));
                }
                else if (FilePathPhoto3 != null)
                {
                    System.IO.File.ReadAllBytes(FilePathPhoto3);
                    return File(FilePathPhoto3, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(FilePathPhoto3));
                }
                else
                {
                    return RedirectToAction("GetList", "Setting_FacilityAndSafety_FacilityAndSafety", new { Area = "Setting" });
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
           
        }

        #endregion

        #region Delete Event
        public ActionResult Delete(string FacilityID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_FacilityID_Encrypted", FacilityID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_FacilitySafety_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "Setting_FacilityAndSafety_FacilityAndSafety", new { Area = "Setting" });
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