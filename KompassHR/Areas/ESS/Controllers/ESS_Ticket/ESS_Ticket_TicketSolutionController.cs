using Dapper;
using KompassHR.Models;
using KompassHR.Areas.ESS.Models.ESS_Ticket;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Net;
using System.IO;

namespace KompassHR.Areas.ESS.Controllers.ESS_Ticket
{
    public class ESS_Ticket_TicketSolutionController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_Ticket_TicketSolution
        #region ESS_Ticket_TicketSolution Main View
        [HttpGet]
        public ActionResult ESS_Ticket_TicketSolution(int? TicketCategoryID, int? TicketSubCategoryID, string TicketRaiseID, string TicketSolutionID_Encrypted)
        {
            try
            {
                Session["TicketRaiseID"] = TicketRaiseID;
                Session["TicketCategoryID"] = TicketCategoryID;
                ViewBag.AddUpdateTitle = "Add";
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }

                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 805;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }

                Ticket_Solution Ticket_Solution = new Ticket_Solution();
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    DynamicParameters param1 = new DynamicParameters();
                    //var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Ticket_Solution";
                    var DocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Ticket_Solution");
                    ViewBag.DocNo = DocNo;
                }

                DynamicParameters param3 = new DynamicParameters();
                param3.Add("@p_TicketRaiseID_Encrypted", "Resolve");
                param3.Add("@p_TicketRaiseID", TicketRaiseID);
                var result = DapperORM.ReturnList<dynamic>("sp_List_Ticket_Raise", param3).ToList();
                ViewBag.GetTicketList = result;
                if (TicketSolutionID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    var EmployeeId = Session["EmployeeId"];
                    param.Add("@p_TicketSolutionID_Encrypted", TicketSolutionID_Encrypted);
                    param.Add("@P_TicketResponserEmployeeID", EmployeeId);
                    Ticket_Solution = DapperORM.ReturnList<Ticket_Solution>("sp_List_Ticket_Solution", param).FirstOrDefault();
                    TempData["DocDate"] = Ticket_Solution.DocDate;
                    TempData["AttachFile"] = Ticket_Solution.AttachFile;
                    TempData["FilePath"] = Ticket_Solution.FilePath;
                    Session["SelectedFile"] = Ticket_Solution.FilePath;
 
                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {
                        param = new DynamicParameters();
                        var GetVoucherNo = "Select DocNo from Ticket_Solution where TicketSolutionID_Encrypted='" + TicketSolutionID_Encrypted + "'";
                        var VoucherNo = DapperORM.DynamicQuerySingle(GetVoucherNo);
                        ViewBag.DocNo = VoucherNo;
                    }
                }
                return View(Ticket_Solution);
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketSolution");
            }
        }
        #endregion

        #region IsValidation
   
        //public JsonResult IsTicketSolutionExists(DateTime DocDate, string TicketSolutionID_Encrypted, string Solution, string Status, string CompeletedPercentage, string FilePath)
        //{
        //    try
        //    {
        //        using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
        //        {
        //            var EmployeeId = Session["EmployeeId"];

        //            param.Add("@p_process", "IsValidation");
        //            param.Add("@p_TicketSolutionID_Encrypted", TicketSolutionID_Encrypted);

        //            param.Add("@p_TicketResponserEmployeeID", EmployeeId);

        //            param.Add("@p_DocDate", DocDate);
        //            param.Add("@p_Solution", Solution);
        //            param.Add("@p_Status", Status);
        //            param.Add("@p_CompeletedPercentage", CompeletedPercentage);
        //            param.Add("@p_FilePath", FilePath);


        //            param.Add("@p_MachineName", Dns.GetHostName().ToString());
        //            param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
        //            param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
        //            //var Result = DapperORM.ExecuteReturn("sp_SUD_Claim_Ticket_Solution", param);
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
        //    }
        //}

        #endregion

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Ticket_Solution Ticket_Solution, HttpPostedFileBase FilePath)
        {
            try
            {
                //string FileName = "";
                //string ContentType = "";
                //int ContentLength = 0;
                //string FileExtention = "";
                //string FilepathForDocSave = "";
                //string FilepathForSaveInDirectory = "";
                //var DirectoryLocation = "";

                //var EmpName = Session["EmployeeName"];
                //EmpName = EmpName.ToString().Trim().Replace(" ", "_");

                //HttpPostedFileWrapper file = Request.Files[0] as HttpPostedFileWrapper;
                //if (Request.Files.Count > 0)
                //{
                //    FileName = file.FileName;
                //    ContentType = file.ContentType;
                //    ContentLength = file.ContentLength;
                //    FileExtention = System.IO.Path.GetExtension(file.FileName);
                //}

                #region Attachment save

                //if (FileName != null && FileName != "")
                //{
                //    SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                //    var GetPath = "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'";
                //    var GetDocPath = DapperORM.DynamicQuerySingle(GetPath);
                //    DirectoryLocation = GetDocPath.DocInitialPath;
                //    if (!Directory.Exists(DirectoryLocation))
                //    {
                //        Directory.CreateDirectory(DirectoryLocation);
                //    }
                //    FilepathForDocSave = EmpName + FileExtention;
                //}

                #endregion

                DynamicParameters param = new DynamicParameters(); 
                var EmployeeId = Session["EmployeeId"];
                param.Add("@p_process", string.IsNullOrEmpty(Ticket_Solution.TicketSolutionID_Encrypted) ? "Save" : "Update");
                param.Add("@p_TicketSolutionID", Ticket_Solution.TicketSolutionID);
                param.Add("@p_TicketSolutionID_Encrypted", Ticket_Solution.TicketSolutionID_Encrypted);
                param.Add("@p_TicketRaiseID", Session["TicketRaiseID"]);
                param.Add("@p_CmpId", Session["CompanyId"]);
                param.Add("@p_BranchID", Session["BranchId"]);
                param.Add("@p_TicketResponserEmployeeID", EmployeeId);              
                param.Add("@p_TicketRaiseToID", Session["EmployeeId"]);
                param.Add("@p_DocNo", Ticket_Solution.DocNo);
                param.Add("@p_DocDate", Ticket_Solution.DocDate);
                param.Add("@p_Solution", Ticket_Solution.Solution);
                param.Add("@p_Status", Ticket_Solution.Status);
                param.Add("@p_TicketCategoryID", Session["TicketCategoryID"]);
                param.Add("@p_CompeletedPercentage", Ticket_Solution.CompeletedPercentage);
                if (Ticket_Solution.TicketSolutionID_Encrypted != null && FilePath == null)
                {
                    param.Add("@p_FilePath", Session["SelectedFile"]);
                }
                else
                {
                    param.Add("@p_FilePath", FilePath == null ? "" : FilePath.FileName);
                }
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);              
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Solution", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                TempData["P_Id"] = param.Get<string>("@p_Id");

                // For Save
                if (TempData["P_Id"] != null)
                {                 
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + TempData["P_Id"] + "\\";   
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (FilePath != null)
                    { 
                        string URLFileFullPath = "";
                        URLFileFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                        FilePath.SaveAs(URLFileFullPath); // This is use for Save image in folder full path
                    }
                }
                else
                {
                    var GetDocPath = DapperORM.DynamicQuerySingle("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'");
                    var GetFirstPath = GetDocPath.DocInitialPath;
                    var FirstPath = GetFirstPath + Ticket_Solution.TicketRaiseID + "\\";
                    if (!Directory.Exists(FirstPath))
                    {
                        Directory.CreateDirectory(FirstPath);
                    }
                    if (FilePath != null)
                    {
                        string URLFileFullPath = "";
                        URLFileFullPath = FirstPath + FilePath.FileName; //Concat Full Path and create New full Path
                        FilePath.SaveAs(URLFileFullPath); // This is use for Save image in folder full path
                    }
                }
                return RedirectToAction("GetList", "ESS_Ticket_TicketSolution");

            }
            catch (Exception Ex)
            {

                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketSolution");
            }
        }

        #endregion

        public ActionResult ViewTicketSolutionHistoryList(string TicketRaiseID)
        {
            try
            {                
                DynamicParameters param = new DynamicParameters();
                param.Add("@p_TicketSolutionID_Encrypted", "List");           
                param.Add("@p_Qry", " and Ticket_Solution.TicketRaiseID='" + TicketRaiseID + "'");
                var data = DapperORM.DynamicList("sp_List_Ticket_Solution", param);
                ViewBag.GetTicketSolutionHistory = data;

                return View();
            }
            catch(Exception ex)
            {
                return RedirectToAction(ex.Message.ToString(), "ESS_Ticket_TicketSolution");
            }
         
        }

        #region GetList
        [HttpGet]
        public ActionResult GetList()
        {
            if (Session["EmployeeId"] == null)
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 805;
            bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
            if (!CheckAccess)
            {
                Session["AccessCheck"] = "False";
                return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
            }

            var EmployeeId = Session["EmployeeId"];

            DynamicParameters param = new DynamicParameters();
            param.Add("@p_TicketSolutionID_Encrypted", "List");
            param.Add("@p_Qry", " AND TicketResponserEmployeeID='" + EmployeeId + "'");

            var data = DapperORM.DynamicList("sp_List_Ticket_Solution", param);
            ViewBag.GetTicketSolutionList = data;

            return View();
        }

        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(int? TicketSolutionID)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_TicketSolutionID", TicketSolutionID);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Ticket_Solution", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_Ticket_TicketSolution");
            }
            catch (Exception Ex)
            {
                return RedirectToAction(Ex.Message.ToString(), "ESS_Ticket_TicketSolution");
            }

        }
        #endregion

        #region DownloadAttachment

        public FileResult DownloadAttachment(string TicketSolutionID_Encrypted)
        {
            try
            {
                var EmployeeId = Session["EmployeeId"];
                if (EmployeeId != null)
                {

                    using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                    {

                        var SolutionId = "select TicketSolutionID from Ticket_Solution where TicketSolutionID_Encrypted='"+ TicketSolutionID_Encrypted + "'";
                        var id = DapperORM.DynamicQuerySingle(SolutionId);
                        var GetAttachmentPath = "Select FilePath from Ticket_Solution where TicketSolutionID_Encrypted='" + TicketSolutionID_Encrypted + "'";
                        var AttachmentPath = DapperORM.DynamicQuerySingle(GetAttachmentPath);

                        var GetPath = "Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='Ticket_Solution'";
                        var GetDocPath = DapperORM.DynamicQuerySingle(GetPath);

                        var APath = GetDocPath.DocInitialPath   + id.TicketSolutionID + "\\"+ AttachmentPath.FilePath;

                        if (AttachmentPath.FilePath != null)
                        {
                            return File(APath, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(APath));

                        }
                        return null;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

      
        public ActionResult DownloadFile(string AttachFile)
        {
            if (AttachFile != null)
            {
                System.IO.File.ReadAllBytes(AttachFile);
                return File(AttachFile, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(AttachFile));
            }
            else
            {
                return RedirectToAction("GetList", "ESS_Ticket_TicketRaise", new { Area = "ESS" });
            }
        }
    }
}