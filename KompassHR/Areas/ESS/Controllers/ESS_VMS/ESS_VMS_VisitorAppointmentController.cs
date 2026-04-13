using Dapper;
using KompassHR.Areas.ESS.Models.ESS_VMS;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Leave;
using KompassHR.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_VMS
{
    public class ESS_VMS_VisitorAppointmentController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        // GET: ESS/ESS_VMS_VisitorAppointment
        #region VisitorAppointment Main View
        [HttpGet]
        public ActionResult ESS_VMS_VisitorAppointment(string VisitorAppointmentID_Encrypted, int? CmpId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 185;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                //int CmpId;
                ViewBag.AddUpdateTitle = "Add";
                Visitor_Appointment Visitor_appointment = new Visitor_Appointment();
                DynamicParameters paramCompany = new DynamicParameters();
                paramCompany.Add("@p_employeeid", Session["EmployeeId"]);
                var GetComapnyName = DapperORM.ReturnList<AllDropDownBind>("sp_GetCompanyDropdown", paramCompany).ToList();

                DynamicParameters paramBranch = new DynamicParameters();
                paramBranch.Add("@query", "Select BranchId as Id,BranchName as Name from Mas_Branch where Deactivate=0");
                ViewBag.VisitingBranchName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramBranch).ToList();

                if (GetComapnyName.Count > 0)
                {
                    ViewBag.ComapnyName = GetComapnyName;
                    var CmpID = GetComapnyName[0].Id;
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_employeeid", Session["EmployeeId"]);
                    param.Add("@p_CmpId", Session["CompanyId"]);
                    var BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();
                    if (BranchName.Count > 0)
                    {
                        ViewBag.BranchName = BranchName;
                        var BUID = BranchName[0].Id;
                        DynamicParameters paramEmpName = new DynamicParameters();
                        paramEmpName.Add("@query", "Select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and CmpID= " + Session["CompanyId"] + " and Mas_Employee.EmployeeLeft=0 and EmployeeBranchId=" + Session["BranchId"] + " and Mas_Employee.ContractorID=1 ORDER BY Name");
                        ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                        DynamicParameters param3 = new DynamicParameters();
                        param3.Add("@query", "select BranchAddress, BranchId from Mas_Branch  where BranchId = " + Session["BranchId"] + " and Deactivate=0");
                        var data = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param3).ToList();
                        ViewBag.BranchAddress = data;

                    }
                    else
                    {
                        ViewBag.BranchName = "";
                        // ViewBag.EmployeeName = "";

                    }
                }
                else
                {
                    ViewBag.ComapnyName = "";
                    ViewBag.BranchName = "";
                    ViewBag.EmployeeName = "";
                }
                var GetDocNo = "Select Isnull(Max(DocNo),0)+1 As DocNo from Visitor_Appointment";
                var DocNo = DapperORM.DynamicQuerySingle(GetDocNo);
                ViewBag.DocNo = DocNo;

                var results = DapperORM.DynamicQueryMultiple(@"select PId as Id ,Purpose as Name from Visitor_Mas_Purpose where Deactivate=0 order by Name;
                                                  select VisitorTypeId as Id ,VisitorType as Name from Visitor_Mas_VisitorType  where deactivate= 0 order by Name");
                ViewBag.PurposeName = results[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.VisitorType = results[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                
                if (VisitorAppointmentID_Encrypted != null)
                {
                    ViewBag.AddUpdateTitle = "Update";
                    param = new DynamicParameters();
                    param.Add("@p_VisitorAppointmentID_Encrypted", VisitorAppointmentID_Encrypted);
                    Visitor_appointment = DapperORM.ReturnList<Visitor_Appointment>("sp_List_Visitor_UpdateAppointment", param).FirstOrDefault();
                    SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
                    {
                        var GetDocNo2 = "Select  DocNo As DocNo  from Visitor_Appointment where VisitorAppointmentID_Encrypted= '" + VisitorAppointmentID_Encrypted + "'";
                        var DocNo2 = DapperORM.DynamicQuerySingle(GetDocNo2);
                        ViewBag.DocNo = DocNo2;
                    }
                    TempData["AppointmentDate"] = Visitor_appointment.AppointmentDate;

                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_employeeid", Session["EmployeeId"]);
                    param1.Add("@p_CmpId", Visitor_appointment.CmpId);
                    ViewBag.BranchName = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param1).ToList();

                    DynamicParameters paramEmpName1 = new DynamicParameters();
                    paramEmpName1.Add("@query", "Select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and Mas_Employee.EmployeeLeft=0 and EmployeeBranchId=" + Visitor_appointment.VisitorAppointmentBranchID + " and Mas_Employee.ContractorID=1 ORDER BY Name");
                    ViewBag.EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName1).ToList();
                }
                TempData["IsSpecial"] = Visitor_appointment.IsSpecial;

                return View(Visitor_appointment);
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
        public ActionResult SaveUpdate(Visitor_Appointment Visitor)
        {
            try
            {

                param.Add("@p_process", string.IsNullOrEmpty(Visitor.VisitorAppointmentID_Encrypted) ? "Save" : "Update");
                param.Add("@p_VisitorAppointmentID", Visitor.VisitorAppointmentID);
                param.Add("@p_VisitorAppointmentID_Encrypted", Visitor.VisitorAppointmentID_Encrypted);
                param.Add("@p_CmpId", Visitor.CmpId);
                param.Add("@p_VisitorAppointmentBranchID", Visitor.VisitorAppointmentBranchID);
                param.Add("@p_HostEntryEmployeeId", Session["EmployeeId"]);
                param.Add("@p_VisitorAppointmentEmployeeID", Visitor.VisitorAppointmentEmployeeID);
                param.Add("@p_DocNo", Visitor.DocNo);
                param.Add("@p_AppointmentDate", Visitor.AppointmentDate);
                param.Add("@p_AppointmentTime", Visitor.AppointmentTime);
                param.Add("@p_VisitorName", Visitor.VisitorName);
                param.Add("@p_AdditionPerson", Visitor.AdditionPerson);
                param.Add("@p_MobileNo", Visitor.MobileNo);
                param.Add("@p_EmailID", Visitor.EmailID);
                param.Add("@p_Designation", Visitor.Designation);
                param.Add("@p_Company", Visitor.Company);
                param.Add("@p_IsSpecial", Visitor.IsSpecial);
                param.Add("@p_VisitorPuposeId", Visitor.VisitorPuposeId);
                param.Add("@p_VisitorRemark", Visitor.VisitorRemark);
                param.Add("@p_ConferenceId", Visitor.ConferenceId);
                param.Add("@p_VisitorTypeId", Visitor.VisitorTypeId);
                param.Add("@p_MeetingLocation", Visitor.MeetingLocation);
                param.Add("@p_VisitingBranchID", Visitor.VisitingBranchID);
                param.Add("@p_CreatedupdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("sp_SUD_Visitor_Appointment", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");


                DynamicParameters paramHost = new DynamicParameters();
                paramHost.Add("@query", "Select EmployeeName from Mas_Employee where EmployeeId=" + Visitor.VisitorAppointmentEmployeeID + " ");
                var Hostdata = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramHost).FirstOrDefault();
                var HostName = Hostdata.EmployeeName;

                //DynamicParameters paramCompany = new DynamicParameters();
                //paramCompany.Add("@query", "Select CompanyName  from Mas_CompanyProfile where  CompanyId=" + Visitor.CmpId + " ");
                //var CompanyName = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramCompany).ToList();

                DynamicParameters paramVMS = new DynamicParameters();
                paramVMS.Add("@query", "Select Top 1 IsActive from Tool_WhatApp where Origin='VMS'");
                var VMSNotification = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramVMS).FirstOrDefault();
               
                if (VMSNotification!=null)
                {
                    var IsNotificationSend = VMSNotification.IsActive;

                    if (IsNotificationSend == true)
                    {
                        try
                        {
                            string url = "http://bhashsms.com/api/sendmsg.php" +
                       "?user=Probus_BW" +
                       "&pass=Djraaz@1985@" +
                       "&sender=BUZWAP" +
                       "&phone=" + Visitor.MobileNo + "" +
                       "&text=new_001" +
                       "&priority=wa" +
                       "&stype=normal" +
                       "&Params=" + Visitor.VisitorName + "," + HostName + "," + Visitor.Company + "," + Convert.ToDateTime(Visitor.AppointmentDate).ToString("dd/MMM/yyyy") + "," + Visitor.AppointmentTime + ", " + Visitor.MeetingLocation + ", 3423, " + "7219707273" + ", www.google.com/maps, Thanks." +
                       "&htype=image" +
                       "&url=https://kompasshr.com/KompassHR_Document/KompassHR_SterlingHospital/1.png";

                            using (HttpClient client = new HttpClient())
                            {
                                HttpResponseMessage response = client.GetAsync(url).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    string result = response.Content.ReadAsStringAsync().Result;
                                    Console.WriteLine("Response: " + result);
                                }
                                else
                                {
                                    //MessageBox.Show("Error: " + response.StatusCode);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return RedirectToAction("ESS_VMS_VisitorAppointment", "ESS_VMS_VisitorAppointment");
                        }
                    }
                }
               
               
                return RedirectToAction("ESS_VMS_VisitorAppointment", "ESS_VMS_VisitorAppointment");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion


        #region GetAllInformation
        [HttpGet]
        public ActionResult GetAllInformation(string Moblieno)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "SELECT top 1 VisitorName,AdditionPerson,Designation,MobileNo,EmailID,Company,IsSpecial,VisitorPuposeId,VisitorRemark,MeetingLocation,ConferenceId,AppointmentDate FROM Visitor_Appointment where MobileNo = '" + Moblieno + "'  and Deactivate =0  order by VisitorAppointmentID desc");
                var GetData = DapperORM.ReturnList<dynamic>("sp_QueryExcution", param).ToList();
                return Json(GetData, JsonRequestBehavior.AllowGet);

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
                var Branch = DapperORM.ReturnList<AllDropDownBind>("sp_GetBusinessUnitDropdown", param).ToList();

                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + Branch[0].Id + " and Mas_Employee.EmployeeLeft=0 and ContractorID=1 and VMSApplicable = 1 and IsVMSMultipleLocation=1 ORDER BY Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters paramName = new DynamicParameters();
                paramName.Add("@query", "select BranchAddress, BranchId from Mas_Branch  where BranchId = " + Branch[0].Id + " and Deactivate=0");
                var AddressBranch = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramName).ToList();

                return Json(new { Branch = Branch, EmployeeName = EmployeeName, AddressBranch = AddressBranch }, JsonRequestBehavior.AllowGet);
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
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 185;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                var EmployeeId = Session["EmployeeId"];
                param.Add("@P_HostEntryEmployeeId", EmployeeId);
                var VisitorAppointmentList = DapperORM.ReturnList<dynamic>("sp_List_Visitor_MyAppointment", param).ToList();
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

        #region Pending Delete
        [HttpGet]
        public ActionResult PendingDelete(string VisitorAppointmentID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_VisitorAppointmentID_Encrypted", VisitorAppointmentID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Visitor_Appointment", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("ESS_VMS_VisitorAppointment", "ESS_VMS_VisitorAppointment");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Delete
        [HttpGet]
        public ActionResult Delete(string VisitorAppointmentID_Encrypted)
        {
            try
            {
                param.Add("@p_process", "Delete");
                param.Add("@p_VisitorAppointmentID_Encrypted", VisitorAppointmentID_Encrypted);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Visitor_Appointment", param);
                TempData["Message"] = param.Get<string>("@p_msg");
                TempData["Icon"] = param.Get<string>("@p_Icon");
                return RedirectToAction("GetList", "ESS_VMS_VisitorAppointment");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetLocation
        [HttpGet]
        public ActionResult GetLocation()
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var data1 = DapperORM.DynamicQuerySingle("Select BranchAddress from Mas_Branch, Mas_Employee where Mas_Branch.BranchId=EmployeeBranchId and EmployeeId = " + Session["EmployeeId"] + "").FirstOrDefault();
                    var data = data1.BranchAddress;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }
        #endregion

        #region GetConferanceName
        [HttpGet]
        public ActionResult GetConferanceName(int CompanyId, int BranchId)
        {
            try
            {
                DynamicParameters param = new DynamicParameters();
                param.Add("@query", "select ConferenceListID as Id,ConferenceName As Name  from Conference_List where CmpID = " + CompanyId + " and BranchID = " + BranchId + " and Deactivate = 0");
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

        #region GetVisitorDetails
        [HttpGet]
        public ActionResult GetVisitorDetails(string MoblieNo)
        {
            try
            {
                using (SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString))
                {
                    var GetVisitorDetails = DapperORM.DynamicQueryList("select AppointmentDate,VisitorName ,Designation,AdditionPerson,EmailID,Company from Visitor_Appointment where MobileNo =" + MoblieNo + "").ToList();
                    var VisitorDetails = GetVisitorDetails;
                    return Json(VisitorDetails, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region SuggetionOfDesignation
        public ActionResult SuggetionOfDesignation(string DesignationName)
        {
            try
            {
                var SuggeationDesignation = DapperORM.ExecuteQuery("SELECT DesignationName FROM Mas_Designation WHERE DesignationName LIKE "+ DesignationName + "%" + "").ToList();
                var SuggeationDesignations = SuggeationDesignation;
                return Json(SuggeationDesignations, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ChcekVisitorIn/Out
        public ActionResult CheckVisitorInOut(string VisitorAppointmentID_Encrypted)
        {
            try
            {
                var Visitor_Tra_Master_VisitorId = DapperORM.DynamicQuerySingle("Select Visitor_Tra_Master_VisitorId from Visitor_Appointment where Deactivate=0 and VisitorAppointmentID_Encrypted='" + VisitorAppointmentID_Encrypted + "'").FirstOrDefault();
                if (Visitor_Tra_Master_VisitorId != null)
                {
                    return Json(Visitor_Tra_Master_VisitorId, JsonRequestBehavior.AllowGet);
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

        #region GetBusinessUnit
        [HttpGet]
        public ActionResult GetEmployee(int BranchId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }


                DynamicParameters paramEmpName = new DynamicParameters();
                paramEmpName.Add("@query", "select EmployeeId as Id, Concat(EmployeeName ,' - ', EmployeeNo) as Name  from Mas_Employee where Deactivate=0 and EmployeeBranchId= " + BranchId + " and Mas_Employee.EmployeeLeft=0 and ContractorID=1 and VMSApplicable = 1 and IsVMSMultipleLocation=1 ORDER BY Name");
                var EmployeeName = DapperORM.ReturnList<AllDropDownBind>("sp_QueryExcution", paramEmpName).ToList();

                DynamicParameters paramName = new DynamicParameters();
                paramName.Add("@query", "select BranchAddress, BranchId from Mas_Branch  where BranchId = " + BranchId + " and Deactivate=0");
                var AddressBranch = DapperORM.ReturnList<dynamic>("sp_QueryExcution", paramName).ToList();

                return Json(new { EmployeeName = EmployeeName, AddressBranch = AddressBranch }, JsonRequestBehavior.AllowGet);
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