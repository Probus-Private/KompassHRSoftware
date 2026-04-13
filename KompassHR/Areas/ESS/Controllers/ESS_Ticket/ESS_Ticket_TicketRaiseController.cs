using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KompassHR.Areas.ESS.Models.ESS_Ticket;
using Dapper;
using KompassHR.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.IO;
using System.Net.Mime;

namespace KompassHR.Areas.ESS.Controllers.ESS_Ticket
{
    public class ESS_Ticket_TicketRaiseController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_Ticket_TicketRaise

        #region ESS_Ticket_TicketRaise Main View 
        [HttpGet]
        public ActionResult ESS_Ticket_TicketRaise(string TicketRaiseID_Encrypted)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 200;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";

                Ticket_Raise TicketRaise = new Ticket_Raise();

                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param1 = new DynamicParameters();
                    var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Ticket_Raise";
                    var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                    ViewBag.DocNo = DocNo;

                    DynamicParameters param2 = new DynamicParameters();
                    param2.Add("@query", "SELECT TicketCategoryID,TicketCategoryName FROM Ticket_Category where Deactivate = 0 order by  TicketCategoryName");
                    var GetTicketCategory = DapperORM.ReturnList<Ticket_Category>("sp_QueryExcution", param2).ToList();
                    ViewBag.ListTicketCategory = GetTicketCategory;

                    DynamicParameters param3 = new DynamicParameters();
                    param3.Add("Query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name from Mas_Employee where  Deactivate=0 and employeeleft=0 and contractorid=1 and employeeid<>1");
                    var GetEmployeeIdName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param3).ToList();
                    ViewBag.EmployeeIdName = GetEmployeeIdName;


                    DynamicParameters param4 = new DynamicParameters();
                    param4.Add("Query", "select TicketSubCategoryID as Id,TicketSubCategoryName as Name  from Ticket_SubCategory where Deactivate=0");
                    var GetSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param4).ToList();
                    ViewBag.GetSubCategory = GetSubCategory;

                }

              

                if (TicketRaiseID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_TicketRaiseID_Encrypted", TicketRaiseID_Encrypted);
                    //param.Add("@p_Qry", " and Ticket_Raise.TicketRaiseID_Encrypted='" + TicketRaiseID_Encrypted + "' and Status='Pending'");
                    TicketRaise = DapperORM.ReturnList<Ticket_Raise>("sp_List_Ticket_Raise", param).FirstOrDefault();
                    
                    TempData["DocDate"] = TicketRaise.DocDate;
                    TempData["AttachFile"] = TicketRaise.AttachFile;
                    TempData["FilePath"] = TicketRaise.FilePath;
                  //  var SubCategoryId = TicketRaise.TicketSubCategoryID;
                  //  TempData["SubCategoryId"] = SubCategoryId;
                    Session["SelectedFile"] = TicketRaise.FilePath;
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        param = new DynamicParameters();
                        var GetVoucherNo = "Select DocNo from Ticket_Raise where TicketRaiseID_Encrypted='" + TicketRaiseID_Encrypted + "'";
                        var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                        ViewBag.DocNo = VoucherNo;
                    }
                   
                }


                return View(TicketRaise);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }
        }

        #endregion
        
        #region IsValidation
        //, HttpPostedFileBase file
        
        //public JsonResult IsTicketRaiseExists(DateTime DocDate, string TicketRaiseID_Encrypted, double TicketCategoryID, string TicketRaiseTitle, string Description, string FilePath, string Priority)
        //{
        //    try
        //    {
        //        using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
        //        {
        //            var EmployeeId = Session["EmployeeId"];

        //            param.Add("@p_process", "IsValidation");
        //            param.Add("@p_TicketRaiseID_Encrypted", TicketRaiseID_Encrypted);

        //            param.Add("@p_TicketRaiseEmployeeID", EmployeeId);

        //            param.Add("@p_DocDate", DocDate);
        //            param.Add("@p_TicketCategoryID", TicketCategoryID);
        //            param.Add("@p_TicketRaiseTitle", TicketRaiseTitle);
        //            param.Add("@p_Description", Description);
        //            param.Add("@p_FilePath", FilePath);
        //            param.Add("@p_Priority", Priority);

        //            param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            //var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_Ticket_Raise", param);
        //            var Message = param.Get<string>("@p_msg");
        //            var Icon = param.Get<string>("@p_Icon");
        //            if (Message != null)
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
        //    }
        //}

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Ticket_Raise Ticket_Raise, HttpPostedFileBase FilePath)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Ticket_Raise.TicketRaiseID_Encrypted) ? "Save" : "Update");
                param.Add("@p_TicketRaiseID", Ticket_Raise.TicketRaiseID);
                param.Add("@p_TicketRaiseID_Encrypted", Ticket_Raise.TicketRaiseID_Encrypted);
              //  param.Add("@p_CmpId", Session["CompanyId"]);
               // param.Add("@p_BranchID", Session["BranchId"]);
                param.Add("@p_TicketRaiseEmployeeID", EmployeeId);
              //  param.Add("@p_TicketRaiseTo", Ticket_Raise.TicketRaisedTo);
               // param.Add("@p_DocNo", Ticket_Raise.DocNo);
              //  param.Add("@p_DocDate", Ticket_Raise.DocDate);
                param.Add("@p_TicketCategoryID", Ticket_Raise.TicketCategoryID);
              //  param.Add("@p_Level1EmployeeID", Ticket_Raise.Level1EmployeeID);
              //  param.Add("@p_Level2EmployeeID", Ticket_Raise.Level2EmployeeID);
               // param.Add("@p_Level3EmployeeID", Ticket_Raise.Level3EmployeeID);
               // param.Add("@p_Level4EmployeeID", Ticket_Raise.Level4EmployeeID);
               // param.Add("@p_Level1Status", Ticket_Raise.Level1Status);
                param.Add("@p_TicketRaiseTitle", Ticket_Raise.TicketRaiseTitle);
                param.Add("@p_TicketSubCategoryID", Ticket_Raise.TicketSubCategoryID);
                param.Add("@p_Description", Ticket_Raise.Description);
                
               // param.Add("@p_TotalLevel", TotalLevel);

              //  param.Add("@p_CurrentLevel", Ticket_Raise.CurrentLevel);

                if (Ticket_Raise.TicketRaiseID_Encrypted != null && FilePath == null)
                {
                    param.Add("@p_FilePath",   Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                }
                param.Add("@p_Priority", Ticket_Raise.Priority);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Raise", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                if (FilePath != null)
                {
                    var docPathQuery = "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Raise'";
                    var getDocPath = DapperORM.DynamicQuerySingle(docPathQuery);

                    string basePath = getDocPath.DocInitialPath;
                    string folderPath = TempData["P_Id"] != null
                        ? Path.Combine(basePath, TempData["P_Id"].ToString())
                        : Path.Combine(basePath, Ticket_Raise.TicketRaiseID.ToString());

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Save the file
                    string fullFilePath = Path.Combine(folderPath, FilePath.FileName);
                    FilePath.SaveAs(fullFilePath);
                }

                //if(TempData["P_Id"]!=null)
                //{
                //    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Raise'");
                //    var GetFirstPath = GetDocPath.DocInitialPath;
                //    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";
                //    if (!Directory.Exists(FirstPath))
                //    {
                //        Directory.CreateDirectory(FirstPath);
                //    }
                //    if (FilePath != null)
                //    {
                //        string URLFileFullPath = "";
                //        URLFileFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                //        FilePath.SaveAs(URLFileFullPath); // This is use for Save image in folder full path

                //    }
                //}
                //else
                //{
                //    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Raise'");
                //    var GetFirstPath = GetDocPath.DocInitialPath;
                //    var FirstPath = GetFirstPath + Ticket_Raise.TicketRaiseID + "\\";
                //    if (!Directory.Exists(FirstPath))
                //    {
                //        Directory.CreateDirectory(FirstPath);
                //    }
                //    if (FilePath != null)
                //    {
                //        string URLFileFullPath = "";
                //        URLFileFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                //        FilePath.SaveAs(URLFileFullPath); // This is use for Save image in folder full path
                //    }
                //}

                return RedirectToAction("ESS_Ticket_TicketRaise", "ESS_Ticket_TicketRaise");
            }
            catch (Exception Ex)
            {

                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }

        }

        // public ActionResult GetRaisedToEmployee(int TicketCategoryID)
        //  {
        //    try
        //    {
        //        DynamicParameters param = new DynamicParameters();
        //        param.Add("@query", "select EmployeeId as Id,EmployeeName as Name  from Mas_Employee where Deactivate=0 and  EmployeeDepartmentID='"+ TicketCategoryID + "' ");
        //        var GetEmployeeIdName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();             
        //        var result = GetEmployeeIdName;
        //        var data = new { result };
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch(Exception ex)
        //    {
        //        return RedirectToAction(ex.Message.ToString(), "ESS_Ticket_TicketRaise");
        //    }
        //}
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 200;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_TicketRaiseID_Encrypted", "List");
                param.Add("@p_EmployeeId", EmployeeId);
                var data = DapperORM.DynamicList("sp_List_Ticket_Raise", param);
                ViewBag.GetTicketRaiseList = data;

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_TicketRaiseID_Encrypted", "List");
                param1.Add("@p_EmployeeId", EmployeeId);
                var InProcessdata = DapperORM.DynamicList("sp_List_InprocessTicket_Raise", param1);
                ViewBag.GetInProcessTicketRaiseList = InProcessdata;


                DynamicParameters param2 = new DynamicParameters();
                param2.Add("@p_TicketRaiseID_Encrypted", "List");
                param2.Add("@p_EmployeeId", EmployeeId);
                var CloseListdata = DapperORM.DynamicList("sp_List_CloseTicket_Raise", param2);
                ViewBag.GetCloseTicketRaiseList = CloseListdata;


                return View();
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }
        }

        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string TicketRaiseID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_TicketRaiseID_Encrypted", TicketRaiseID_Encrypted);
                param.Add("@p_CreatedupdateBy", "Admin");
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Raise", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Ticket_TicketRaise");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketRaise");
            }

        }

        #endregion

        #region Download File/Image
        public ActionResult DownloadFile(string TicketRaiseID)
        {            
            if (TicketRaiseID != null)
            {
                System.IO.File.ReadAllBytes(TicketRaiseID);
                return File(TicketRaiseID, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(TicketRaiseID));
            }
            else
            {
                return RedirectToAction("GetList", "ESS_Ticket_TicketRaise", new { Area = "ESS" });
            }
        }

        #endregion

        #region Download Attachment
        public ActionResult DownloadAttachment(string DownloadAttachment)
        {
            try
            {
               
                var APath = DownloadAttachment;

                var driveLetter = Path.GetPathRoot(APath);
                // Check if the drive exists
                if (string.IsNullOrEmpty(driveLetter) || !DriveInfo.GetDrives().Any(d => d.Name.Equals(driveLetter, StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Message"] = $"Drive {driveLetter} does not exist.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Ticket_TicketRaise", new { area = "ESS" });
                }
                // Construct full file path
                var fullPath = APath;
                // Check if the file exists
                if (!System.IO.File.Exists(fullPath))
                {
                    TempData["Message"] = "File not found on the server.";
                    TempData["Icon"] = "error";
                    return RedirectToAction("GetList", "ESS_Ticket_TicketRaise", new { area = "ESS" });

                }
                // Return the file for download
                var fileName = Path.GetFileName(fullPath);
                return File(fullPath, MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework here)
                return new HttpStatusCodeResult(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion


        #region Get Details
        public ActionResult GetCategoryDetails(int CategoryId, int SubCategoryId)
        {
            try
            {
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 200;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var GetDetails = "Select * from Ticket_Matrix Where TicketMatrixKPICategoryId ='" + CategoryId + "' and TicketSubCategoryId='" + SubCategoryId + "' AND Deactivate=0";
                var GetDetails1 = DapperORM.DynamicQueryList(GetDetails).FirstOrDefault();

                param.Add("@p_TicketMatrixId_Encrypted", GetDetails1.TicketMatrixId_Encrypted);
                var data = DapperORM.DynamicList("sp_List_Ticket_Matrix", param);

               // var KeypointsData = data.FirstOrDefault();

                 return Json(data, JsonRequestBehavior.AllowGet);
              

            }
            catch (Exception ex)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region ViewTicketSolutionHistoryList
        [HttpGet]
        public ActionResult ViewTicketSolutionHistoryList(int? TicketRaiseID)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();

                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_EmployeeId", EmployeeId);
                param.Add("@p_TicketRaiseID", TicketRaiseID);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);

                var data = DapperORM.DynamicList("sp_List_RaiseTicket_Solution", param);
                // ViewBag.GetTicketSolutionHistory = data;
                return Json(data , JsonRequestBehavior.AllowGet);

                // return Json(data, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region Sub category
        [HttpGet]
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
                var GetSubCategory = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", param).ToList();
                var result = GetSubCategory;
                var data = new { result };

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