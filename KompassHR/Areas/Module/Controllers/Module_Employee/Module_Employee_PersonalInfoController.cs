using Dapper;
using KompassHR.Areas.Module.Models.Module_Employee;
using KompassHR.Areas.Setting.Models.Setting_Onboarding;
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
using System.Threading.Tasks;
using System.IO;

namespace KompassHR.Areas.Module.Controllers.Module_Employee
{
    public class Module_Employee_PersonalInfoController : Controller
    {
        // GET: Module/Module_Employee_PersonalInfo
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();

        #region PersonalInfo main View
        [HttpGet]
        public ActionResult Module_Employee_PersonalInfo(string Status)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 414;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                TempData["OnboardEmployeeName"] = Session["OnboardEmployeeName"];
                ViewBag.AddUpdateTitle = "Add";

                ViewBag.Nationality = DapperORM.DynamicQuerySingle("Select EmployeeCountryId from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");

                var countEMP = DapperORM.DynamicQueryList("Select count(PersonalEmployeeId)  as CountPersonalEmployeeId  from Mas_Employee_Personal where Mas_Employee_Personal.PersonalEmployeeId=" + Session["OnboardEmployeeId"] + "").FirstOrDefault();
                TempData["CountPersonalEmployeeId"] = countEMP?.CountPersonalEmployeeId;

                var countPreBoard = DapperORM.DynamicQuerySingle("select PreboardingFid from Mas_Employee wheRE EmployeeId=" + Session["OnboardEmployeeId"] + "");
                TempData["PreBoardFid"] = countPreBoard?.PreboardingFid;

                Mas_Employee_Personal mas_employeePersonal = new Mas_Employee_Personal();
                var ResultList = DapperORM.DynamicQueryMultiple(@"Select ReligionId as Id,ReligionName as Name from Mas_Religion Where Deactivate=0 order by ReligionName;
                                                       Select CasteID As Id,CasteName AS Name from Mas_Caste Where Deactivate=0 order by CasteName;
                                                       Select QualificationPFId as Id,QualificationPFName as Name from Mas_Qualification_PF Where Deactivate=0 order by Name;
                                                        Select DocumentId As Id,DocumentName As Name from Mas_Document Where Deactivate=0  and ProofForBirth=1 order by Name");
                ViewBag.GetReligionName = ResultList[0].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetCastName = ResultList[1].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetQualificationName = ResultList[2].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();
                ViewBag.GetVerificationName = ResultList[3].Select(x => new AllDropDownBind { Id = (double)x.Id, Name = (string)x.Name }).ToList();

                if (Status == "Pre")
                {
                    var GetPreboardingFid = DapperORM.DynamicQuerySingle("Select PreboardingFid from Mas_Employee where employeeid=" + Session["OnboardEmployeeId"] + "");
                    var PreboardingFid = GetPreboardingFid?.PreboardingFid;

                    param = new DynamicParameters();
                    param.Add("@p_FId", PreboardingFid);
                    mas_employeePersonal = DapperORM.ReturnList<Mas_Employee_Personal>("sp_GetList_Mas_PreboardEmployeeDetails", param).FirstOrDefault();
                    ViewBag.GetEmployeePersonal = mas_employeePersonal;
                    TempData["BirthdayDate"] = mas_employeePersonal?.BirthdayDate;
                    return View(mas_employeePersonal);
                }

                param = new DynamicParameters();
                param.Add("@p_PersonalEmployeeId", Session["OnboardEmployeeId"]);
                mas_employeePersonal = DapperORM.ReturnList<Mas_Employee_Personal>("sp_List_Mas_Employee_Personal", param).FirstOrDefault();
                ViewBag.GetEmployeePersonal = mas_employeePersonal;

                if (mas_employeePersonal != null)
                {
                    TempData["BirthdayDate"] = mas_employeePersonal.BirthdayDate;
                    ViewBag.AddUpdateTitle = "Update";
                }

                var CheckExist = DapperORM.DynamicQueryList(@"SELECT EmployeeId, IsAadhar, IsPAN, IsPassport, IsVoter,  
                            IsVehicleRC, IsDrivingLicence, IsEmployeement, IsGeoLocation FROM Tool_EmployeeVerifyApiSetting  
                            WHERE Deactivate = 0 AND EmployeeId = " + Session["EmployeeId"] + "").FirstOrDefault();
                if (CheckExist != null)
                {
                    TempData["IsAadhar"] = CheckExist.IsAadhar;
                    TempData["IsPAN"] = CheckExist.IsPAN;
                }


                return View(mas_employeePersonal);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        [HttpGet]
        public ActionResult IsPersonalExists(string PAN, string PersonalId_Encrypted, string AadhaarNo,string PersonalEmailId,string PrimaryMobile)
        {

            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "IsValidation");
                param.Add("@p_PAN", PAN);
                param.Add("@p_PersonalEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_PersonalId_Encrypted", PersonalId_Encrypted);
                param.Add("@p_AadhaarNo", AadhaarNo);
                param.Add("@p_PersonalEmailId", PersonalEmailId);
                param.Add("@p_PrimaryMobile", PrimaryMobile);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                var Result = DapperORM.ExecuteReturn("sp_SUD_Mas_Employee_Personal", param);
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
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }

        }

        #region SaveUpdate
        [HttpPost]
        public ActionResult SaveUpdate(Mas_Employee_Personal EmployeePersonal)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                param.Add("@p_process", "Save");
                param.Add("@p_PersonalId", EmployeePersonal.PersonalId);
                param.Add("@p_PersonalId_Encrypted", EmployeePersonal.PersonalId_Encrypted);
                param.Add("@p_PersonalEmployeeId", Session["OnboardEmployeeId"]);
                param.Add("@p_AadhaarNo", EmployeePersonal.AadhaarNo);
                param.Add("@p_NameAsPerAadhaar", EmployeePersonal.NameAsPerAadhaar);
                param.Add("@p_AadhaarNoMobileNoLink", EmployeePersonal.AadhaarNoMobileNoLink);
                param.Add("@p_AadhaarNoMobileNo", EmployeePersonal.AadhaarNoMobileNo);
                param.Add("@p_PAN", EmployeePersonal.PAN);
                param.Add("@p_NameAsPerPan", EmployeePersonal.NameAsPerPan);
                param.Add("@p_PANAadhaarLink", EmployeePersonal.PANAadhaarLink);
                param.Add("@p_PrimaryMobile", EmployeePersonal.PrimaryMobile);
                param.Add("@p_SecondaryMobile", EmployeePersonal.SecondaryMobile);
                param.Add("@p_WhatsAppNo", EmployeePersonal.WhatsAppNo);
                param.Add("@p_PersonalEmailId", EmployeePersonal.PersonalEmailId);
                param.Add("@p_BirthdayDate", EmployeePersonal.BirthdayDate);
                param.Add("@p_AgeOfJoining", EmployeePersonal.AgeOfJoining);
                param.Add("@p_BirthdayPlace", EmployeePersonal.BirthdayPlace);
                param.Add("@p_BirthdayProofOfDocumentID", EmployeePersonal.BirthdayProofOfDocumentID);
                param.Add("@p_BirthdayProofOfCertificateNo", EmployeePersonal.BirthdayProofOfCertificateNo);
                param.Add("@p_IsDOBSpecial", EmployeePersonal.IsDOBSpecial);
                param.Add("@p_EmployeeQualificationID", EmployeePersonal.EmployeeQualificationID);
                param.Add("@p_QualificationRemark", EmployeePersonal.QualificationRemark);
                param.Add("@p_Gender", EmployeePersonal.Gender);
                param.Add("@p_BloodGroup", EmployeePersonal.BloodGroup);
                param.Add("@p_MaritalStatus", EmployeePersonal.MaritalStatus);
                param.Add("@p_AnniversaryDate", EmployeePersonal.AnniversaryDate);
                param.Add("@p_Ifyouwantdonotdisclosemygenderthentick", EmployeePersonal.Ifyouwantdonotdisclosemygenderthentick);
                param.Add("@p_PhysicallyDisabled", EmployeePersonal.PhysicallyDisabled);
                param.Add("@p_PhysicallyDisableType", EmployeePersonal.PhysicallyDisableType);
                param.Add("@p_PhysicallyDisableRemark", EmployeePersonal.PhysicallyDisableRemark);
                param.Add("@p_IdentificationMark", EmployeePersonal.IdentificationMark);
                param.Add("@p_DrivingLicenceNo", EmployeePersonal.DrivingLicenceNo);
                param.Add("@p_DrivingLicenceExpiryDate", EmployeePersonal.DrivingLicenceExpiryDate);
                param.Add("@p_PassportNo", EmployeePersonal.PassportNo);
                param.Add("@p_PassportExpiryDate", EmployeePersonal.PassportExpiryDate);
                param.Add("@p_EmployeeReligionID", EmployeePersonal.EmployeeReligionID);
                param.Add("@p_Ifyouwantdonotdisclosemyreligioncastthentick", EmployeePersonal.Ifyouwantdonotdisclosemyreligioncastthentick);
                param.Add("@p_EmployeeCasteID", EmployeePersonal.EmployeeCasteID);
                param.Add("@p_EmployeeSpecificDegree", EmployeePersonal.EmployeeSpecificDegree);
                param.Add("@p_EmployeeBirthProofEducation", EmployeePersonal.EmployeeBirthProofEducation);
                param.Add("@p_EmployeeSubCategory", EmployeePersonal.EmployeeSubCategory);

                param.Add("@p_CreatedUpdateBy", Session["EmployeeName"]);
                param.Add("@p_MachineName", Dns.GetHostName().ToString());
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                param.Add("@p_Icon", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);//OutPut Parameter
                var data = DapperORM.ExecuteReturn("[sp_SUD_Mas_Employee_Personal]", param);
                var message = param.Get<string>("@p_msg");
                var Icon = param.Get<string>("@p_Icon");
                TempData["Message"] = message;
                TempData["Icon"] = Icon.ToString();
                return RedirectToAction("Module_Employee_PersonalInfo", "Module_Employee_PersonalInfo");
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        #endregion

        #region GetPersonalAsPrebording
        [HttpGet]
        public ActionResult GetPersonalAsPrebording()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramPersonal = new DynamicParameters();
                paramPersonal.Add("@p_DocOrigin", "Personal");
                paramPersonal.Add("@p_EmployeeId", Session["OnboardEmployeeId"]);
                var data = DapperORM.ExecuteSP<dynamic>("sp_GetPreboardingEmployeeInfo", paramPersonal).FirstOrDefault();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Partial View 
        [HttpGet]
        public PartialViewResult Onboarding_SidebarMenu()
        {
            try
            {

                var OnboardEmployeeId = Session["OnboardEmployeeId"];
                DynamicParameters paramList = new DynamicParameters();
                paramList.Add("@p_EmployeeId", OnboardEmployeeId);
                var StatusCheck = DapperORM.DynamicList("sp_List_Mas_Employee_StatusCheck", paramList);
                ViewBag.GetStatusCheckList = StatusCheck;

                var id = Session["ModuleId"];
                var ScreenId = Session["ScreenId"];
                var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(id), Convert.ToInt32(ScreenId), "SubForm", "Transation");
                ViewBag.GetUserMenuList = GetMenuList;

                return PartialView("_Onboarding_SidebarMenu");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get Aadhar Details
        public class AadharOtpBody
        {
            public string verification_id { get; set; }
            public string aadhaar_number { get; set; }
        }

        public class OTPVerifyBody
        {
            public string otp { get; set; }
            public string ref_id { get; set; }
        }
        [HttpPost]
        public async Task<ActionResult> GetAadharOtp(string AadharNo)
        {
            string randomCode = GenerateRandomCode();
            try
            {
                string clientId = "CF97970CC4DSJMQ7GBOSABHJ640";
                string clientSecret = "4eaf8da590c624c6e23e5d0799fc3ddf9fa63337";
                string VerificationUrl = "https://api.cashfree.com/verification/offline-aadhaar/otp";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-client-secret", clientSecret);
                    // Create JSON payload from parameters

                    var requestData = new
                    {
                        verification_id = randomCode,
                        aadhaar_number = AadharNo
                    };

                    dynamic FinalData;
                    string GetOTP = "";

                    string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                    HttpResponseMessage response = await client.PostAsync(VerificationUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        DateTime parsedDate;
                        var EmployeeId = Session["EmployeeId"];
                        var EmployeeName = Session["EmployeeName"];

                        string responseContent = await response.Content.ReadAsStringAsync();
                        FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        if (FinalData != null)
                        {
                            GetOTP = JsonConvert.SerializeObject(FinalData, Formatting.Indented);
                        }
                        sqlcon.Execute(@"
                                    INSERT INTO Mas_Aadhar_API (
                                        CreatedBy, CreatedDate, MachineName, EmployeeId, APIType, Ref_Id, Status, Message
                                    ) VALUES (
                                        @CreatedBy, GETDATE(), @MachineName, @EmployeeId, 'AADHAAR OTP', @RefId, @Status, @Message
                                    )", new
                        {
                            CreatedBy = EmployeeName,
                            MachineName = Dns.GetHostName(),
                            EmployeeId = EmployeeId,
                            RefId = FinalData?.ref_id?.ToString() ?? string.Empty,
                            Status = FinalData?.status?.ToString() ?? string.Empty,
                            Message = FinalData?.message?.ToString() ?? string.Empty,
                        });
                    }
                    return Json(new { GetOTP }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error is: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> VerifyOTP(string CheckOTP, string AadharRefId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // Create the request payload
                string clientId = "CF97970CC4DSJMQ7GBOSABHJ640";
                string clientSecret = "4eaf8da590c624c6e23e5d0799fc3ddf9fa63337";
                string VerificationUrl = "https://api.cashfree.com/verification/offline-aadhaar/verify";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-client-secret", clientSecret);
                    // Create JSON payload from parameters

                    var requestData = new
                    {
                        ref_id = AadharRefId,
                        otp = CheckOTP
                    };

                    string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                    //var FinalData = JsonConvert.DeserializeObject<dynamic>();
                    dynamic FinalData;
                    string GetOTP = "";
                    // Make a POST request to Cashfree API
                    HttpResponseMessage response = await client.PostAsync(VerificationUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        DateTime parsedDate;
                        var EmployeeId = Session["EmployeeId"];
                        var EmployeeName = Session["EmployeeName"];
                        string GetAadharDetails = "";
                        string responseContent = await response.Content.ReadAsStringAsync();
                        FinalData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (FinalData != null)
                        {
                            GetAadharDetails = JsonConvert.SerializeObject(FinalData, Formatting.Indented);
                        }
                        sqlcon.Execute(@"
                                    INSERT INTO Mas_Aadhar_API (
                                        CreatedBy, CreatedDate, MachineName, EmployeeId, APIType, Ref_Id, Status, Message, Care_Of, 
                                        Address, Dob, Email, Gender, Name, Country, First_Name, Dist, House, Landmark, Pincode, PO, Name_Provided, 
                                        State, Street, SubDist, VTC, Locality, Year_Of_Birth, Mobile_Hash, Photo_Link, Share_Code, Xml_File
                                    ) VALUES (
                                        @CreatedBy, GETDATE(), @MachineName, @EmployeeId, 'AADHAAR', @RefId, @Status, @Message, @CareOf, 
                                        @Address, @Dob, @Email, @Gender, @Name, @Country, @FirstName, @Dist, @House, @Landmark, @Pincode, @PO, @NameProvided, 
                                        @State, @Street, @SubDist, @VTC, @Locality, @YearOfBirth, @MobileHash, @PhotoLink, @ShareCode, @XmlFile
                                    )", new
                        {
                            CreatedBy = EmployeeName,
                            MachineName = Dns.GetHostName(),
                            EmployeeId = EmployeeId,
                            RefId = FinalData?.ref_id?.ToString() ?? string.Empty,
                            Status = FinalData?.status?.ToString() ?? string.Empty,
                            Message = FinalData?.message?.ToString() ?? string.Empty,
                            CareOf = FinalData?.care_of?.ToString() ?? string.Empty,
                            Address = FinalData?.address?.ToString() ?? string.Empty,
                            Dob = DateTime.TryParse(FinalData?.dob?.ToString(), out parsedDate) ? parsedDate : (DateTime?)null,
                            Email = FinalData?.email?.ToString() ?? string.Empty,
                            Gender = FinalData?.gender?.ToString() ?? string.Empty,
                            Name = FinalData?.name?.ToString() ?? string.Empty,
                            Country = FinalData?.country?.ToString() ?? string.Empty,
                            FirstName = FinalData?.first_name?.ToString() ?? string.Empty,
                            Dist = FinalData?.dist?.ToString() ?? string.Empty,
                            House = FinalData?.house?.ToString() ?? string.Empty,
                            Landmark = FinalData?.landmark?.ToString() ?? string.Empty,
                            Pincode = FinalData?.pincode?.ToString() ?? string.Empty,
                            PO = FinalData?.po?.ToString() ?? string.Empty,
                            NameProvided = FinalData?.name_provided?.ToString() ?? string.Empty,
                            State = FinalData?.state?.ToString() ?? string.Empty,
                            Street = FinalData?.street?.ToString() ?? string.Empty,
                            SubDist = FinalData?.sub_dist?.ToString() ?? string.Empty,
                            VTC = FinalData?.vtc?.ToString() ?? string.Empty,
                            Locality = FinalData?.locality?.ToString() ?? string.Empty,
                            YearOfBirth = FinalData?.year_of_birth?.ToString() ?? string.Empty,
                            MobileHash = FinalData?.mobile_hash?.ToString() ?? string.Empty,
                            PhotoLink = FinalData?.photo_link?.ToString() ?? string.Empty,
                            ShareCode = FinalData?.share_code?.ToString() ?? string.Empty,
                            XmlFile = FinalData?.xml_file?.ToString() ?? string.Empty
                        });

                        // Let me know if you need anything else tweaked or added! 🚀

                        Console.WriteLine("Response Content: " + responseContent);
                        return Json(new { GetAadharDetails }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { GetOTP }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region Get PAN Details
        public class PANBody
        {
            public string verification_id { get; set; }
            public string Pan { get; set; }
            public string name { get; set; }
        }


        [HttpPost]
        public async Task<ActionResult> GetPANDetails(string PAN)
        {

            string randomCode = GenerateRandomCode();
            try
            {
                string clientId = "CF97970CC4DSJMQ7GBOSABHJ640";
                string clientSecret = "4eaf8da590c624c6e23e5d0799fc3ddf9fa63337";
                string VerificationUrl = "https://api.cashfree.com/verification/pan/advance";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-client-id", clientId);
                    client.DefaultRequestHeaders.Add("x-client-secret", clientSecret);
                    // Create JSON payload from parameters
                    var requestData = new
                    {
                        verification_id = randomCode,
                        pan = PAN,
                        name = ""
                    };
                    string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                    //var FinalData = JsonConvert.DeserializeObject<dynamic>();
                    dynamic FinalData;
                    string serializedData = "";
                    // Make a POST request to Cashfree API
                    HttpResponseMessage response = await client.PostAsync(VerificationUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        FinalData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                        DateTime parsedDate;
                        var EmployeeId = Session["EmployeeId"];
                        var EmployeeName = Session["EmployeeName"];
                        serializedData = JsonConvert.SerializeObject(FinalData, Formatting.Indented);

                        sqlcon.Execute(@"
                                    INSERT INTO Mas_PAN_API (
                                        CreatedBy, CreatedDate, MachineName, EmployeeId, APIType, Refrance_Id, Verification_Id, 
                                        Status, Message, Name_Provided, Pan, Registered_Name, Name_Pan_Card, First_Name, Last_Name, 
                                        Type, Gender, Date_Of_Birth, Masked_Aadhaar_Number, Email, Mobile_Number, Aadhaar_Linked, 
                                        Full_Address, Street, City, State, Pincode, Country
                                    ) VALUES (
                                        @CreatedBy, GETDATE(), @MachineName, @EmployeeId, 'PAN', 
                                        @ReferenceId, @VerificationId, @Status, @Message, @NameProvided, 
                                        @Pan, @RegisteredName, @NamePanCard, @FirstName, @LastName, 
                                        @Type, @Gender, @DateOfBirth, @MaskedAadhaarNumber, @Email, 
                                        @MobileNumber, @AadhaarLinked, @FullAddress, @Street, 
                                        @City, @State, @Pincode, @Country
                                    )", new
                        {
                            CreatedBy = EmployeeName,
                            MachineName = Dns.GetHostName(),
                            EmployeeId = EmployeeId,
                            ReferenceId = FinalData?.reference_id?.ToString() ?? string.Empty,
                            VerificationId = FinalData?.verification_id?.ToString() ?? string.Empty,
                            Status = FinalData?.status?.ToString() ?? string.Empty,
                            Message = FinalData?.message?.ToString() ?? string.Empty,
                            NameProvided = FinalData?.name_provided?.ToString() ?? string.Empty,
                            Pan = FinalData?.pan?.ToString() ?? string.Empty,
                            RegisteredName = FinalData?.registered_name?.ToString() ?? string.Empty,
                            NamePanCard = FinalData?.name_pan_card?.ToString() ?? string.Empty,
                            FirstName = FinalData?.first_name?.ToString() ?? string.Empty,
                            LastName = FinalData?.last_name?.ToString() ?? string.Empty,
                            Type = FinalData?.type?.ToString() ?? string.Empty,
                            Gender = FinalData?.gender?.ToString() ?? string.Empty,
                            DateOfBirth = DateTime.TryParse(FinalData?.date_of_birth?.ToString(), out parsedDate)
                                ? parsedDate : (DateTime?)null,
                            MaskedAadhaarNumber = FinalData?.masked_aadhaar_number?.ToString() ?? string.Empty,
                            Email = FinalData?.email?.ToString() ?? string.Empty,
                            MobileNumber = FinalData?.mobile_number?.ToString() ?? string.Empty,
                            AadhaarLinked = FinalData?.aadhaar_linked?.ToString() ?? string.Empty,
                            FullAddress = FinalData?.full_address?.ToString() ?? string.Empty,
                            Street = FinalData?.street?.ToString() ?? string.Empty,
                            City = FinalData?.city?.ToString() ?? string.Empty,
                            State = FinalData?.state?.ToString() ?? string.Empty,
                            Pincode = FinalData?.pincode?.ToString() ?? string.Empty,
                            Country = FinalData?.country?.ToString() ?? string.Empty
                        });
                    }
                    return Json(new { success = true, data = serializedData }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error is: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public async Task<ActionResult> GetPANDetailsOld(string PAN)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // Create the request payload
                string randomCode = GenerateRandomCode();
                var PANbody = new
                {
                    verification_id = randomCode,
                    Pan = PAN,
                };

                using (HttpClient client = new HttpClient())
                {
                    string requestUri = "http://115.124.123.180:8096/verification/Pan";
                    //string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3MDU5OTA0MTcsImlzcyI6Imh0dHA6Ly9Lb21wYXNzSHIuY29tIiwiYXVkIjoiaHR0cDovL0tvbXBhc3NIci5jb20ifQ.dNzkz5oSazVHXML2l4nOqNfkwmm4hezgZk9YlW54RgQ";

                    HttpContent body = new StringContent(JsonConvert.SerializeObject(PANbody), Encoding.UTF8, "application/json");

                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri))
                    {
                        request.Content = body;
                        //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        try
                        {
                            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                            using (HttpResponseMessage response = await client.SendAsync(request))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    string errorResponse = await response.Content.ReadAsStringAsync();
                                    return Json(new { success = false, message = "API Error: " + errorResponse }, JsonRequestBehavior.AllowGet);
                                }

                                string responseContent = await response.Content.ReadAsStringAsync();
                                var FinalData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                                if (FinalData == null)
                                {
                                    return Json(new { success = false, message = "Invalid API response." }, JsonRequestBehavior.AllowGet);
                                }

                                try
                                {
                                    DateTime parsedDate;
                                    var EmployeeId = Session["EmployeeId"];
                                    var EmployeeName = Session["EmployeeName"];
                                    string serializedData = JsonConvert.SerializeObject(FinalData, Formatting.Indented);

                                    sqlcon.Execute(@"
                                    INSERT INTO Mas_PAN_API (
                                        CreatedBy, CreatedDate, MachineName, EmployeeId, APIType, Refrance_Id, Verification_Id, 
                                        Status, Message, Name_Provided, Pan, Registered_Name, Name_Pan_Card, First_Name, Last_Name, 
                                        Type, Gender, Date_Of_Birth, Masked_Aadhaar_Number, Email, Mobile_Number, Aadhaar_Linked, 
                                        Full_Address, Street, City, State, Pincode, Country
                                    ) VALUES (
                                        @CreatedBy, GETDATE(), @MachineName, @EmployeeId, 'PAN', 
                                        @ReferenceId, @VerificationId, @Status, @Message, @NameProvided, 
                                        @Pan, @RegisteredName, @NamePanCard, @FirstName, @LastName, 
                                        @Type, @Gender, @DateOfBirth, @MaskedAadhaarNumber, @Email, 
                                        @MobileNumber, @AadhaarLinked, @FullAddress, @Street, 
                                        @City, @State, @Pincode, @Country
                                    )", new
                                    {
                                        CreatedBy = EmployeeName,
                                        MachineName = Dns.GetHostName(),
                                        EmployeeId = EmployeeId,
                                        ReferenceId = FinalData?.reference_id?.ToString() ?? string.Empty,
                                        VerificationId = FinalData?.verification_id?.ToString() ?? string.Empty,
                                        Status = FinalData?.status?.ToString() ?? string.Empty,
                                        Message = FinalData?.message?.ToString() ?? string.Empty,
                                        NameProvided = FinalData?.name_provided?.ToString() ?? string.Empty,
                                        Pan = FinalData?.pan?.ToString() ?? string.Empty,
                                        RegisteredName = FinalData?.registered_name?.ToString() ?? string.Empty,
                                        NamePanCard = FinalData?.name_pan_card?.ToString() ?? string.Empty,
                                        FirstName = FinalData?.first_name?.ToString() ?? string.Empty,
                                        LastName = FinalData?.last_name?.ToString() ?? string.Empty,
                                        Type = FinalData?.type?.ToString() ?? string.Empty,
                                        Gender = FinalData?.gender?.ToString() ?? string.Empty,
                                        DateOfBirth = DateTime.TryParse(FinalData?.date_of_birth?.ToString(), out parsedDate)
                                            ? parsedDate : (DateTime?)null,
                                        MaskedAadhaarNumber = FinalData?.masked_aadhaar_number?.ToString() ?? string.Empty,
                                        Email = FinalData?.email?.ToString() ?? string.Empty,
                                        MobileNumber = FinalData?.mobile_number?.ToString() ?? string.Empty,
                                        AadhaarLinked = FinalData?.aadhaar_linked?.ToString() ?? string.Empty,
                                        FullAddress = FinalData?.full_address?.ToString() ?? string.Empty,
                                        Street = FinalData?.street?.ToString() ?? string.Empty,
                                        City = FinalData?.city?.ToString() ?? string.Empty,
                                        State = FinalData?.state?.ToString() ?? string.Empty,
                                        Pincode = FinalData?.pincode?.ToString() ?? string.Empty,
                                        Country = FinalData?.country?.ToString() ?? string.Empty
                                    });

                                    return Json(new { success = true, data = serializedData }, JsonRequestBehavior.AllowGet);
                                }
                                catch (SqlException sqlEx)
                                {
                                    return Json(new { success = false, message = "Database Error: " + sqlEx.Message }, JsonRequestBehavior.AllowGet);
                                }
                                catch (Exception ex)
                                {
                                    return Json(new { success = false, message = "Unexpected Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        catch (HttpRequestException httpEx)
                        {
                            return Json(new { success = false, message = "HTTP Request Error: " + httpEx.Message }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, message = "Unexpected Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "General Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> GetPANDetails(string PAN)
        //{
        //    try
        //    {
        //        if (Session["EmployeeId"] == null)
        //        {
        //            return RedirectToAction("Login", "Login", new { Area = "" });
        //        }
        //        // Create the request payload
        //        string randomCode = GenerateRandomCode();
        //        var PANbody = new
        //        {
        //            verification_id= randomCode,
        //            Pan = PAN,
        //        };

        //        using (HttpClient client = new HttpClient())
        //        {
        //            // Define the request URI
        //            string requestUri = "http://115.124.123.180:8096/verification/Pan";

        //            // Authorization token
        //            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3MDU5OTA0MTcsImlzcyI6Imh0dHA6Ly9Lb21wYXNzSHIuY29tIiwiYXVkIjoiaHR0cDovL0tvbXBhc3NIci5jb20ifQ.dNzkz5oSazVHXML2l4nOqNfkwmm4hezgZk9YlW54RgQ";

        //            // Create the request content (e.g., JSON payload)
        //            HttpContent body = new StringContent(JsonConvert.SerializeObject(PANbody), Encoding.UTF8, "application/json");

        //            // Create an instance of HttpRequestMessage
        //            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri))
        //            {
        //                // Set the request content
        //                request.Content = body;
        //                // Add the Authorization header with the token
        //                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        //                try
        //                {
        //                    // Send the request and get the response
        //                    using (HttpResponseMessage response = await client.SendAsync(request))
        //                    {
        //                        // Check if the response status code is successful
        //                        if (response.IsSuccessStatusCode)
        //                        {
        //                            // Read the response content
        //                            try
        //                            {
        //                                DateTime parsedDate;
        //                                var EmployeeId = Session["EmployeeId"];
        //                                var EmployeeName = Session["EmployeeName"];
        //                                string serializedData = "";
        //                                string responseContent = await response.Content.ReadAsStringAsync();
        //                                var FinalData = JsonConvert.DeserializeObject<dynamic>(responseContent);
        //                                if (FinalData != null)
        //                                {
        //                                    serializedData = JsonConvert.SerializeObject(FinalData, Formatting.Indented);
        //                                }

        //                                sqlcon.Execute(@"
        //                            INSERT INTO Mas_PAN_API (
        //                                CreatedBy, CreatedDate, MachineName, EmployeeId, APIType, Refrance_Id, Verification_Id, 
        //                                Status, Message, Name_Provided, Pan, Registered_Name, Name_Pan_Card, First_Name, Last_Name, 
        //                                Type, Gender, Date_Of_Birth, Masked_Aadhaar_Number, Email, Mobile_Number, Aadhaar_Linked, 
        //                                Full_Address, Street, City, State, Pincode, Country
        //                            ) VALUES (
        //                                @CreatedBy, GETDATE(), @MachineName, @EmployeeId, 'PAN', 
        //                                @ReferenceId, @VerificationId, @Status, @Message, @NameProvided, 
        //                                @Pan, @RegisteredName, @NamePanCard, @FirstName, @LastName, 
        //                                @Type, @Gender, @DateOfBirth, @MaskedAadhaarNumber, @Email, 
        //                                @MobileNumber, @AadhaarLinked, @FullAddress, @Street, 
        //                                @City, @State, @Pincode, @Country
        //                            )", new
        //                                {
        //                                    CreatedBy = EmployeeName,
        //                                    MachineName = Dns.GetHostName(),
        //                                    EmployeeId = EmployeeId,
        //                                    ReferenceId = FinalData?.reference_id?.ToString() ?? string.Empty,
        //                                    VerificationId = FinalData?.verification_id?.ToString() ?? string.Empty,
        //                                    Status = FinalData?.status?.ToString() ?? string.Empty,
        //                                    Message = FinalData?.message?.ToString() ?? string.Empty,
        //                                    NameProvided = FinalData?.name_provided?.ToString() ?? string.Empty,
        //                                    Pan = FinalData?.pan?.ToString() ?? string.Empty,
        //                                    RegisteredName = FinalData?.registered_name?.ToString() ?? string.Empty,
        //                                    NamePanCard = FinalData?.name_pan_card?.ToString() ?? string.Empty,
        //                                    FirstName = FinalData?.first_name?.ToString() ?? string.Empty,
        //                                    LastName = FinalData?.last_name?.ToString() ?? string.Empty,
        //                                    Type = FinalData?.type?.ToString() ?? string.Empty,
        //                                    Gender = FinalData?.gender?.ToString() ?? string.Empty,
        //                                    DateOfBirth = DateTime.TryParse(FinalData?.date_of_birth?.ToString(), out parsedDate)
        //                                    ? parsedDate : (DateTime?)null,
        //                                    //DateOfBirth = FinalData?.date_of_birth?.ToString() ?? string.Empty,
        //                                    MaskedAadhaarNumber = FinalData?.masked_aadhaar_number?.ToString() ?? string.Empty,
        //                                    Email = FinalData?.email?.ToString() ?? string.Empty,
        //                                    MobileNumber = FinalData?.mobile_number?.ToString() ?? string.Empty,
        //                                    AadhaarLinked = FinalData?.aadhaar_linked?.ToString() ?? string.Empty,
        //                                    FullAddress = FinalData?.full_address?.ToString() ?? string.Empty,
        //                                    Street = FinalData?.street?.ToString() ?? string.Empty,
        //                                    City = FinalData?.city?.ToString() ?? string.Empty,
        //                                    State = FinalData?.state?.ToString() ?? string.Empty,
        //                                    Pincode = FinalData?.pincode?.ToString() ?? string.Empty,
        //                                    Country = FinalData?.country?.ToString() ?? string.Empty
        //                                });

        //                                return Json(new { serializedData }, JsonRequestBehavior.AllowGet);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Session["GetErrorMessage"] = ex.Message;
        //                                return RedirectToAction("Module_Employee_PersonalInfo", "Module_Employee_PersonalInfo", new { Area = "Module" });
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (SqlException sqlEx)
        //                {
        //                    // Handle SQL-related exceptions
        //                    Console.WriteLine("SQL Error: " + sqlEx.Message);
        //                    return RedirectToAction("Module_Employee_PersonalInfo", "Module_Employee_PersonalInfo", new { Area = "Module" });
        //                }
        //                catch (Exception ex)
        //                {
        //                    // Handle any other unexpected exceptions
        //                    Console.WriteLine("General Error: " + ex.Message);
        //                    return RedirectToAction("Module_Employee_PersonalInfo", "Module_Employee_PersonalInfo", new { Area = "Module" });
        //                }
        //            }
        //        }
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Session["GetErrorMessage"] = ex.Message;
        //        return RedirectToAction("ErrorPage", "Login");
        //    }
        //}

        public static string GenerateRandomCode(int minLength = 5, int maxLength = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            int length = random.Next(minLength, maxLength + 1);

            char[] code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }

            return new string(code);
        }

        #endregion

    }
}