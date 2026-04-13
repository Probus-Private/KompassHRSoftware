using Dapper;
using KompassHR.Areas.ESS.Models.ESS_Marketplace;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Marketplace;
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

namespace KompassHR.Areas.ESS.Controllers.ESS_Marketplace
{
    public class ESS_Marketplace_MarketplaceController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: MarketPlaceSetting/MarketPlace

        public ActionResult ESS_Marketplace_Marketplace(string MarketPlacePostID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 203;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var EmpId = Session["EmployeeId"];
                ViewBag.AddUpdateTitle = "Add";
                MarketPlace_Post MarketPlacePost = new MarketPlace_Post();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    param.Add("@query", "SELECT MarketPlaceCategoryID, MarketPlaceCategory FROM MarketPlace_Category WHERE Deactivate = 0 ORDER BY MarketPlaceCategory");
                    var MarketCategory = DapperORM.ReturnList<MarketPlace_Category>("sp_QueryExcution", param);
                    ViewBag.GetMarketCategory = MarketCategory;

                    param.Add("@query", "SELECT * FROM MarketPlace_Setting WHERE Deactivate = 0");
                    var MarketPlaceSetting = DapperORM.ReturnList<MarketPlace_Setting>("sp_QueryExcution", param).FirstOrDefault();
                    ViewBag.GetTotalPostCount = MarketPlaceSetting.MonthlyLimit;

                    param.Add("@query", "SELECT * FROM MarketPlace_Post WHERE MarketPlaceEmployeeID='" + EmpId + "' and Deactivate = 0");
                    var EmployeePost = DapperORM.ReturnList<MarketPlace_Post>("sp_QueryExcution", param).Count();
                    ViewBag.EmployeePostCount = EmployeePost;

                    var RemainingCount = Convert.ToInt32(MarketPlaceSetting.MonthlyLimit) - Convert.ToInt32(EmployeePost);
                    ViewBag.RemainingCount = RemainingCount;

                    var Id = Session["EmployeeId"];
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@query", "select PrimaryMobile from Mas_Employee_Personal where PersonalEmployeeId='" + Id + "' and Deactivate = 0");
                    var ContactNo = DapperORM.ReturnList<Mas_Employee_Personal>("sp_QueryExcution", param1).FirstOrDefault();
                    ViewBag.GetContactNo = ContactNo?.PrimaryMobile;

                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from MarketPlace_Post";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    TempData["DocNo"] = DocNo.DocNo;
                    TempData["p_Id"] = MarketPlacePost.MarketPlacePostID;
                }

                if (!string.IsNullOrEmpty(MarketPlacePostID_Encrypted))
                {
                    param = new DynamicParameters();
                    ViewBag.AddUpdateTitle = "Update";
                    param.Add("@p_MarketPlacePostID_Encrypted", MarketPlacePostID_Encrypted);
                    MarketPlacePost = DapperORM.ReturnList<MarketPlace_Post>("sp_List_MarketPlace_Post", param).FirstOrDefault();
                    ViewBag.GetContactNo = MarketPlacePost.ContactNo;
                    ViewBag.Photo1FileName = MarketPlacePost.Photo1;
                    ViewBag.Photo2FileName = MarketPlacePost.Photo2;
                    ViewBag.Photo3FileName = MarketPlacePost.Photo3;
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        var GetDocNo = "Select DocNo from MarketPlace_Post where MarketPlacePostID_Encrypted='" + MarketPlacePostID_Encrypted + "'";
                        var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                        TempData["p_Id"] = MarketPlacePost.MarketPlacePostID;
                        TempData["DocNo"] = DocNo.DocNo;
                    }
                }
                TempData["DocDate"] = MarketPlacePost.DocDate;
                return View(MarketPlacePost);
            }
            catch (Exception Ex)
            {
                return RedirectToAction("ESS_Marketplace_Marketplace", new { area = "ESS" });
            }
        }

        [HttpPost]
        public ActionResult SaveUpdate(MarketPlace_Post MarketPlacePost, HttpPostedFileBase Photo1, HttpPostedFileBase Photo2, HttpPostedFileBase Photo3)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_process", string.IsNullOrEmpty(MarketPlacePost.MarketPlacePostID_Encrypted) ? "Save" : "Update");
                param.Add("@p_MarketPlacePostID", MarketPlacePost.MarketPlacePostID);
                param.Add("@p_MarketPlaceCategoryID", MarketPlacePost.MarketPlaceCategoryID);
                param.Add("@p_MarketPlacePostID_Encrypted", MarketPlacePost.MarketPlacePostID_Encrypted);
                param.Add("@p_BranchID", MarketPlacePost.BranchID);
                param.Add("@p_DocNo", MarketPlacePost.DocNo);
                param.Add("@p_MarketPlacePostTitle", MarketPlacePost.MarketPlacePostTitle);
                param.Add("@p_MarketPlacePostDesciption", MarketPlacePost.MarketPlacePostDesciption);
                param.Add("@p_MarketPlacePostPrice", MarketPlacePost.MarketPlacePostPrice);
                param.Add("@p_ContactNo", MarketPlacePost.ContactNo);
                param.Add("@p_IsActive", MarketPlacePost.IsActive);
                param.Add("@p_DocDate", MarketPlacePost.DocDate);
                param.Add("@p_MarketPlaceEmployeeID", EmployeeId);

                // Handle Photo1
                if (Photo1 != null)
                {
                    param.Add("@p_Photo1", Photo1.FileName);
                }
                else if (!string.IsNullOrEmpty(MarketPlacePost.MarketPlacePostID_Encrypted))
                {
                    param.Add("@p_Photo1", MarketPlacePost.Photo1); // Retain existing file name
                }
                else
                {
                    param.Add("@p_Photo1", "");
                }

                // Handle Photo2
                if (Photo2 != null)
                {
                    param.Add("@p_Photo2", Photo2.FileName);
                }
                else if (!string.IsNullOrEmpty(MarketPlacePost.MarketPlacePostID_Encrypted))
                {
                    param.Add("@p_Photo2", MarketPlacePost.Photo2); // Retain existing file name
                }
                else
                {
                    param.Add("@p_Photo2", "");
                }

                // Handle Photo3
                if (Photo3 != null)
                {
                    param.Add("@p_Photo3", Photo3.FileName);
                }
                else if (!string.IsNullOrEmpty(MarketPlacePost.MarketPlacePostID_Encrypted))
                {
                    param.Add("@p_Photo3", MarketPlacePost.Photo3); // Retain existing file name
                }
                else
                {
                    param.Add("@p_Photo3", "");
                }

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                using (var sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var Result = DapperORM.ExecuteReturn("sp_SUD_MarketPlace_Post", param);
                    TempData["Message"] = param.Get<string>("@p_msg");
                    TempData["Icon"] = param.Get<string>("@p_Icon");
                    var P_Id = param.Get<string>("@p_Id");

                    if (P_Id != null)
                    {
                        var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin = 'MarketPlace'");
                        var GetFirstPath = GetDocPath.DocInitialPath;
                        var FirstPath = Path.Combine(GetFirstPath, P_Id.ToString());

                        if (!Directory.Exists(FirstPath))
                        {
                            Directory.CreateDirectory(FirstPath);
                        }

                        if (Photo1 != null)
                        {
                            string Image1 = Path.Combine(FirstPath, Photo1.FileName);
                            Photo1.SaveAs(Image1);
                        }
                        if (Photo2 != null)
                        {
                            string Image2 = Path.Combine(FirstPath, Photo2.FileName);
                            Photo2.SaveAs(Image2);
                        }
                        if (Photo3 != null)
                        {
                            string Image3 = Path.Combine(FirstPath, Photo3.FileName);
                            Photo3.SaveAs(Image3);
                        }
                    }
                }

                return RedirectToAction("ESS_Marketplace_Marketplace", "ESS_Marketplace_Marketplace");
            }
            catch (Exception Ex)
            {
                TempData["Message"] = Ex.Message;
                return RedirectToAction("ESS_Marketplace_Marketplace", new { area = "ESS" });
            }
        }


        public ActionResult ViewFile(string postId, string fileName)
        {
            try
            {
                var docPath = new SqlConnection(DapperORM.connectionString)
                    .QuerySingle("SELECT DocInitialPath FROM Tool_Documnet_DirectoryPath WHERE DocOrigin = 'MarketPlace'")
                    .DocInitialPath;

                var filePath = Path.Combine(docPath, postId, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    var contentType = MimeMapping.GetMimeMapping(fileName);
                    return File(System.IO.File.ReadAllBytes(filePath), contentType);
                }
                else
                {
                    return HttpNotFound("File not found.");
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        [HttpGet]
        public ActionResult GetList()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 203;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_MarketPlaceEmployeeID", EmployeeId);
                param.Add("@p_MarketPlacePostID_Encrypted", "List");
                var data = DapperORM.DynamicList("sp_List_MarketPlace_Post", param);
                ViewBag.GetMarketPlace = data;

                DynamicParameters param12 = new DynamicParameters();
                param12.Add("@query", "Select EmployeeId as Id , concat(Mas_Employee.EmployeeName, ' - ', Mas_Employee.EmployeeNo) as Name  from Mas_Employee where deactivate = 0 and EmployeeLeft = 0 and ContractorID = 1");
                var EmployeeList = DapperORM.ReturnList<EmployeeDto>("sp_QueryExcution", param12);
                ViewBag.EmployeeList = EmployeeList;


                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Marketplace_Marketplace");
            }

        }

        public ActionResult Delete(string MarketPlacePostID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_MarketPlacePostID_Encrypted", MarketPlacePostID_Encrypted);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_MarketPlace_Post", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Marketplace_Marketplace");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Marketplace_Marketplace");
            }

        }

        public ActionResult DownloadFile(string filePath)
        {
            // Ensure filePath is not null and the file exists
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                // Return the file as a download response
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
            {
                // If file not found, redirect to GetList or show an error
                TempData["Message"] = "File not found!";
                TempData["Icon"] = "error";
                return RedirectToAction("GetList", "ESS_Marketplace_Marketplace", new { Area = "ESS" });
            }
        }



        [HttpPost]
        public JsonResult MarkAsSold(string MarketPlacePostID_Encrypted, string BuyerName, DateTime SoldDate, string Remark)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "UPDATE MarketPlace_Post SET IsActive = 0 WHERE MarketPlacePostID_Encrypted = '" + MarketPlacePostID_Encrypted + "'");

                DynamicParameters paramSold = new DynamicParameters();
                paramSold.Add("@query", "UPDATE MarketPlace_Post SET BuyerName = '" + BuyerName + "' , BuyerDate='" + SoldDate + "' , Remark='" + Remark + "' WHERE MarketPlacePostID_Encrypted = '" + MarketPlacePostID_Encrypted + "'");

                DapperORM.ExecuteReturn("sp_QueryExcution", param);
                DapperORM.ExecuteReturn("sp_QueryExcution", paramSold);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}