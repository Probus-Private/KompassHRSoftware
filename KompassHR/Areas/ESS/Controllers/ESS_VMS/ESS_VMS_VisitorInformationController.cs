using Dapper;
using KompassHR.Areas.ESS.Models.ESS_VMS;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static KompassHR.Areas.ESS.Models.ESS_VMS.Visitor_Tra_Master;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_VisitorInformationController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_VMS_VisitorInformation
        #region ESS_VMS_VisitorInformation Main View 

        public ActionResult ESS_VMS_VisitorInformation(string VisitorId_Encrypted, string HostEmployeeId, string VisitorPurposeId, string VisitorName, string Designation, string CompanyName, string MobileNo, int? VisitorAppointmentID, int? Additional, int? VisitorType)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 188;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                ViewBag.AddUpdateTitle = "Add";
                Session["VisitorAppointmentID"] = VisitorAppointmentID;

                var results = DapperORM.DynamicQueryMultiple(@"SELECT PId as Id, Purpose as Name FROM Visitor_Mas_Purpose WHERE Deactivate = 0 order by Purpose
                                                    SELECT DocumentId as Id, DocumentName as Name FROM Visitor_Mas_Document WHERE Deactivate = 0 order by DocumentName                                              
                                                    select VisitorTypeId as Id,VisitorType as Name from Visitor_Mas_VisitorType where Deactivate=0 order by VisitorType");
                //ViewBag.GetEmployeeName = results.Read<AllDropDownClass>().ToList();
                ViewBag.GetPurposeName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetVerificationName = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.VisitorType = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.GetPurposeName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetVerificationName = results.Read<AllDropDownClass>().ToList();
                //ViewBag.VisitorType = results.Read<AllDropDownClass>().ToList();

                // SELECT EmployeeId as Id, Concat(EmployeeName, ' - ', EmployeeNo) as Name FROM Mas_Employee WHERE Deactivate = 0 and Mas_Employee.ContractorID = 1 ORDER BY Name

                DynamicParameters paramemp = new DynamicParameters();
                paramemp.Add("@p_BranchId", null);
                paramemp.Add("@p_EmployeeId", Session["EmployeeId"]);
                ViewBag.GetEmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_GetEmployeeDropdown", paramemp).ToList();


                if (HostEmployeeId != null)
                {
                    TempData["HostEmployeeId"] = HostEmployeeId;
                    TempData["VisitorType"] = VisitorType;

                    int EmpId = Convert.ToInt32(HostEmployeeId);
                    var getDepartment = DapperORM.DynamicQueryList(@" select DepartmentName from Mas_Employee join Mas_Department on Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId where Mas_Employee.EmployeeId = '" + EmpId + "'").FirstOrDefault();
                    ViewBag.GetDepartment = getDepartment.DepartmentName;
                }
                var DocNo = DapperORM.DynamicQuerySingle("Select Isnull(Max(DocNo),0)+1 As DocNo from Visitor_Tra_Master");
                ViewBag.DocNo = DocNo != null ? DocNo.DocNo : null;

                ViewBag.VisitorMaster1 = TempData["VisitorMaster"];
                return View();
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
        public ActionResult SaveUpdate(string imageData, Visitor_Tra_Master VisitorMaster)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", string.IsNullOrEmpty(VisitorMaster.VisitorId_Encrypted) ? "Save" : "Update");
                param.Add("@p_VisitorId", VisitorMaster.VisitorId);
                param.Add("@p_VisitorId_Encrypted", VisitorMaster.VisitorId_Encrypted);
                param.Add("@p_VisitorAppointmentID", Session["VisitorAppointmentID"]);
                param.Add("@p_HostEmployeeId", VisitorMaster.HostEmployeeId);
                param.Add("@p_DocNo", VisitorMaster.DocNo);
                param.Add("@p_MobileNo", VisitorMaster.MobileNo);
                param.Add("@p_VisitorName", VisitorMaster.VisitorName);
                param.Add("@p_VisitorTypeId", VisitorMaster.VisitorTypeId);
                param.Add("@p_Additional", VisitorMaster.Additional);
                param.Add("@p_Batchno", VisitorMaster.Batchno);
                param.Add("@p_VisitorPurposeId", VisitorMaster.VisitorPurposeId);
                param.Add("@p_CompanyName", VisitorMaster.CompanyName);
                param.Add("@p_VechileNo", VisitorMaster.VechileNo);
                param.Add("@p_VisitorIdendtityId", VisitorMaster.VisitorIdendtityId);
                param.Add("@p_DocumentNo", VisitorMaster.DocumentNo);
                param.Add("@p_Address", VisitorMaster.Address);
                param.Add("@p_Temperature", VisitorMaster.Temperature);
                param.Add("@p_VisitingBranchID", Session["BranchId"]);
                param.Add("@p_Remark", VisitorMaster.Remark);
                param.Add("@p_VisitorInBy", Session["EmployeeId"]);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Id", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Visitor_Tra_Master", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                var p_Id = param.Get<string>("@p_Id");
                if (p_Id != null)
                {
                    if (imageData != null && imageData != "") //Base64 Check
                    {
                        var imageString = string.Empty;
                        string ImgStr = String.Empty;

                        imageString = imageData.Split('/')[1].Split(';')[0];
                        ImgStr = imageData.Replace($"data:image/{imageString};base64,", String.Empty);
                        var Setdate = DateTime.Now.Date.ToString("dd-MM-yyyy");
                        string imagename = p_Id.ToString() + ".Jpeg";
                        Base64ToImage(ImgStr, imagename);
                        DapperORM.ExecuteQuery("Update Visitor_Tra_Master set PhotoPath='" + imagename + "' where VisitorId='" + p_Id + "'");
                    }
                }

                var GetGetVisitorId_Encrypted = DapperORM.DynamicQueryList("Select VisitorId_Encrypted from Visitor_Tra_Master where VisitorId=" + p_Id + "").FirstOrDefault();
                var GetVisitorId_Encrypted = GetGetVisitorId_Encrypted.VisitorId_Encrypted;
                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"], GetVisitorId_Encrypted = GetVisitorId_Encrypted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Convert Base64 to image
        private void Base64ToImage(string PhotoPath, string imagename)
        {
            // Convert base 64 string to byte[] 
            byte[] imageBytes = Convert.FromBase64String(PhotoPath);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            //geting server folder path 
            var GetDocPath = DapperORM.DynamicQueryList("Select DocInitialPath from Tool_Documnet_DirectoryPath where DocOrigin='VMS'").FirstOrDefault();
            var GetFirstPath = GetDocPath.DocInitialPath;
            // Extract drive letter (e.g., "E:\")
            string driveRoot = Path.GetPathRoot(GetFirstPath);

            // Check if drive exists
            if (!DriveInfo.GetDrives().Any(d => d.IsReady && d.Name.Equals(driveRoot, StringComparison.OrdinalIgnoreCase)))
            {
                //throw new Exception($"Drive {driveRoot} is not available.");
            }
            else
            {
                if (!Directory.Exists(GetFirstPath))
                {
                    Directory.CreateDirectory(GetFirstPath);
                }
                string path = GetFirstPath + imagename;
                //Save Image to server folder
                image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
        #endregion  

        #region GetVisitorDetails
        public ActionResult GetVisitorDetails(string MobileNo)
        {
            try
            {
                var VisitorDetails = DapperORM.DynamicQuerySingle("select top 1 VisitorName, VisitorPurposeId,Additional, CompanyName ,Address from Visitor_Tra_Master where Deactivate=0 and MobileNo = '" + MobileNo + "'").FirstOrDefault();
                return Json(VisitorDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetDepartment
        [HttpGet]
        public ActionResult GetDepartment(int EmployeeId)
        {
            try
            {
                var GetDepartmentName = DapperORM.DynamicQuerySingle("select DepartmentName from Mas_Employee join Mas_Department on Mas_Employee.EmployeeDepartmentID = Mas_Department.DepartmentId where Mas_Employee.EmployeeId = '" + EmployeeId + "'");
                return Json(GetDepartmentName.DepartmentName, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion     

        #region GetAllAllowandDeposite
        public ActionResult AllAllowedandDepositeItem(string VisitorName, int VisitorId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                var results = DapperORM.DynamicQueryMultiple(@"SELECT ARDId as Id, ARDItemName as Name FROM Visitor_Mas_ARD WHERE Deactivate = 0 AND ARDItemType = 1  order by Name 
                                                    SELECT ARDId as Id, ARDItemName as Name FROM Visitor_Mas_ARD WHERE Deactivate = 0 AND ARDItemType = 2  order by Name
                                                    SELECT ARDId as Id, ARDItemName as Name FROM Visitor_Mas_ARD WHERE Deactivate = 0 AND ARDItemType = 3  order by Name
                                                    SELECT LockerId as Id, LockerNo as Name FROM Visitor_Mas_Locker WHERE Deactivate = 0 order by Name");
                ViewBag.GetAllowVisitorItem = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetReturnVisitorItem = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetDepositeVisitorItem = results[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetVisitorLocker = results[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                //ViewBag.GetAllowVisitorItem = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetDepositeVisitorItem = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetReturnVisitorItem = results.Read<AllDropDownClass>().ToList();
                //ViewBag.GetVisitorLocker = results.Read<AllDropDownClass>().ToList();

                var Getalldata = DapperORM.DynamicQueryList("select AdrName,Remark,ItemID,VisitorId,AdrVisitorName,VisitorId  from Visitor_Tra_Master_ADR where Origin ='Allow Item' and VisitorId =" + VisitorId + "").ToList();
                ViewBag.Getallowanddeposite = Getalldata;

                var GetallDepositedata = DapperORM.DynamicQueryList("select AdrName,LockerNo,Remark , ItemID,LockerID,VisitorId,AdrVisitorName from Visitor_Tra_Master_ADR where Origin ='Deposite Item'  and VisitorId =" + VisitorId + "").ToList();
                ViewBag.GetallDepositedata = GetallDepositedata;

                var GetallReturndata = DapperORM.DynamicQueryList("select AdrName,Remark,ItemID,VisitorId,AdrVisitorName from Visitor_Tra_Master_ADR where Origin ='Return Item' and VisitorId =" + VisitorId + "").ToList();
                ViewBag.GetallReturndata = GetallReturndata;
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #endregion

        #region AllowDepositSave              

        public ActionResult AllowItemSaveUpdate(Visitor_Tra_Master_ADR AllowVisitorADR, string AllowItemID, string AllowItemName, string AllowRemark, string VisitorId, string VisitorName)
        {
            try
            {
                using (var connection = new SqlConnection(DapperORM.connectionString))
                {
                    List<Visitor_Tra_Master_ADR> VisitorTraMasterSaveList = new List<Visitor_Tra_Master_ADR>();
                    var query = "SELECT COUNT(VisitorId) AS VisitorId FROM Visitor_Tra_Master_ADR WHERE Origin = @Origin and AdrName = @AdrName AND VisitorId = @VisitorId";
                    var parameters = new { Origin = "Allow Item", AdrName = AllowItemName, VisitorId = VisitorId };
                    var IfExist = connection.QueryFirstOrDefault<int>(query, parameters);

                    if (IfExist == 0)
                    {
                        VisitorTraMasterSaveList.Add(new Visitor_Tra_Master_ADR()
                        {
                            AdrName = AllowItemName,
                            Remark = AllowRemark,
                            ItemID = Convert.ToInt32(AllowItemID),
                            Origin = "Allow Item",
                            VisitorId = Convert.ToInt32(VisitorId),
                            AdrVisitorName = VisitorName,
                        });
                        var savedRowsAffected = connection.Execute("INSERT INTO Visitor_Tra_Master_ADR (AdrName, LockerNo, Remark, ItemID, Origin, VisitorId, AdrVisitorName) VALUES (@AdrName, @LockerNo, @Remark, @ItemID, @Origin, @VisitorId, @AdrVisitorName)",
                            VisitorTraMasterSaveList);

                        return Json(savedRowsAffected, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Message = "Already exist this record", Icon = "warning" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        [HttpPost]
        public ActionResult DepositeSaveUpdate(Visitor_Tra_Master_ADR VisitorADR, string DepositItem, string LockerNo, string DepositeRemark, string DepositItemID, string LockerNoID, string VisitorId, string VisitorName)
        {
            try
            {
                using (var connection = new SqlConnection(DapperORM.connectionString))
                {

                    List<Visitor_Tra_Master_ADR> VisitorTraMasterSaveList = new List<Visitor_Tra_Master_ADR>();

                    //var query = "SELECT COUNT(VisitorId) AS VisitorId FROM Visitor_Tra_Master_ADR WHERE Origin = @Origin and AdrName = @AdrName AND VisitorId = @VisitorId";
                    //var parameters = new { Origin = "Deposite Item", AdrName = DepositItem, VisitorId = VisitorId };
                    var IfExist = DapperORM.DynamicQueryList("SELECT COUNT(VisitorId) AS VisitorId FROM Visitor_Tra_Master_ADR WHERE Origin = 'Deposite Item' and AdrName = '"+ DepositItem + "' AND VisitorId = "+ VisitorId + "").FirstOrDefault();

                    if (IfExist.VisitorId == 0)
                    {
                        VisitorTraMasterSaveList.Add(new Visitor_Tra_Master_ADR()
                        {
                            AdrName = DepositItem,
                            LockerNo = LockerNo,
                            Remark = DepositeRemark,
                            ItemID = Convert.ToInt32(DepositItemID),
                            LockerID = Convert.ToInt32(LockerNoID),
                            Origin = "Deposite Item",
                            VisitorId = Convert.ToInt32(VisitorId),
                            AdrVisitorName = VisitorName
                        });

                        var rowsAffected = connection.Execute("INSERT INTO Visitor_Tra_Master_ADR (AdrName, LockerNo, Remark, ItemID, LockerID, Origin, VisitorId, AdrVisitorName) VALUES (@AdrName, @LockerNo, @Remark, @ItemID, @LockerID, @Origin, @VisitorId, @AdrVisitorName)",
                         VisitorTraMasterSaveList);
                        return Json(rowsAffected, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { Message = "Already exist this record", Icon = "warning" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult RetunItemSaveUpdate(Visitor_Tra_Master_ADR RetrnVisitorADR, string ReturnItem, string Remark, string ReturnItemID, string VisitorId, string VisitorName)
        {

            try
            {
                using (var connection = new SqlConnection(DapperORM.connectionString))
                {
                    List<Visitor_Tra_Master_ADR> VisitorTraMasterSaveList = new List<Visitor_Tra_Master_ADR>();
                    //var query = "SELECT COUNT(VisitorId) AS VisitorId FROM Visitor_Tra_Master_ADR WHERE Origin = @Origin and AdrName = @AdrName AND VisitorId = @VisitorId";
                    //var parameters = new { Origin = "Return Item", AdrName = ReturnItem, VisitorId = VisitorId };
                    var IfExist = DapperORM.DynamicQuerySingle("SELECT COUNT(VisitorId) AS VisitorId FROM Visitor_Tra_Master_ADR WHERE Origin = 'Return Item' and AdrName ='"+ ReturnItem + "' AND VisitorId = "+ VisitorId + "").FirstOrDefault();

                    if (IfExist?.VisitorId == 0 || IfExist?.VisitorId == null)
                    {
                        VisitorTraMasterSaveList.Add(new Visitor_Tra_Master_ADR()
                        {
                            AdrName = ReturnItem,
                            Remark = Remark,
                            ItemID = Convert.ToInt32(ReturnItemID),
                            Origin = "Return Item",
                            VisitorId = Convert.ToInt32(VisitorId),
                            AdrVisitorName = VisitorName,
                        });
                        var rowsAffected = connection.Execute("INSERT INTO Visitor_Tra_Master_ADR (AdrName, LockerNo, Remark, ItemID, LockerID, Origin ,VisitorId,AdrVisitorName) VALUES (@AdrName, @LockerNo, @Remark, @ItemID, @LockerID, @Origin,@VisitorId,@AdrVisitorName)",
                        VisitorTraMasterSaveList);
                        return Json(rowsAffected, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Message = "Already exist this record", Icon = "warning" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region DeleteAllowanddepositItem

        public ActionResult DeleteAllowItem(int ItemID, int VisitorId)
        {
            try
            {
                var DeleteAllow = DapperORM.DynamicQuerySingle("delete Visitor_Tra_Master_ADR where ItemID = " + ItemID + " and VisitorId =" + VisitorId + "").FirstOrDefault();
                return Json(DeleteAllow, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }


        public ActionResult DeleteDepositeItem(int ItemID, int VisitorId)
        {
            try
            {
                var DeleteAllow = DapperORM.DynamicQuerySingle("delete Visitor_Tra_Master_ADR where ItemID = " + ItemID + " and VisitorId =" + VisitorId + "").FirstOrDefault();
                return Json(DeleteAllow, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorPage", "Login", new { area = "" });
            }
        }

        public ActionResult DeleteReturnItem(int ItemID, int VisitorId)
        {
            try
            {
                var DeleteAllow = DapperORM.DynamicQuerySingle("delete Visitor_Tra_Master_ADR where ItemID = " + ItemID + " and VisitorId =" + VisitorId + "").FirstOrDefault();
                return Json(DeleteAllow, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region MacthBatchNo
        [HttpGet]
        public ActionResult MacthBatchNo(string Batchno)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                var GetBathno = DapperORM.DynamicQuerySingle("SELECT Batchno FROM Visitor_Tra_Master WHERE Deactivate =0 and Batchno='" + Batchno + "' and Status = 'In' and CONVERT(DATE, InDateTime) = CONVERT(DATE, GETDATE()) ");
                if (GetBathno != null)
                {
                    return Json(GetBathno, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region VisitorInformationPassInvoice
        public ActionResult VisitorInformationPassInvoice(string VisitorId_Encrypted)
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
                var instructions = DapperORM.DynamicQuerySingle("SELECT Instruction FROM Visitor_Mas_Instruction WHERE Deactivate = 0").ToList();
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
                return Json(new { instructions = instructions, VisitorList = VisitorList, fullPath = fullPath }, JsonRequestBehavior.AllowGet);
                // return View();
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