using ClosedXML.Excel;
using Dapper;
using KompassHR.Areas.Dashboards.Models;
using KompassHR.Areas.ESS.Models.ESS_Recruitment;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_Recruitment
{
    public class ESS_Recruitment_BulkCandidateDataController : Controller
    {
        DynamicParameters param = new DynamicParameters();
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        clsCommonFunction objcon = new clsCommonFunction();
        DataTable dt_Qualifcation = new DataTable();
        DataTable dt_ResumeSource = new DataTable();
        // GET: ESS/ESS_Recruitment_BulkCandidateData
        #region BulkCandidateData MAin View
        [HttpGet]
        public ActionResult ESS_Recruitment_BulkCandidateData()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 130;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                DynamicParameters paramAgency = new DynamicParameters();
                paramAgency.Add("@query", "select AgencyId as Id,AgencyName as Name from Recruitment_Agency where Deactivate=0");
                var AgencyAssign = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramAgency).ToList();
                ViewBag.AgencyAssign = AgencyAssign;
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
        int List_Qualification_Id = 0;
        int Qualification_Id = 0;
        int List_ResumeSource_Id = 0;
        int ResumeSource_Id = 0;

        [HttpPost]
        public ActionResult SaveUpdate(List<BulkCandidateData> BulkObj, int AgencyAssignId) /*BulkCandidateData*/
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                dt_Qualifcation = objcon.GetDataTable("Select QualificationPFId as Id,QualificationPFName as Name from Mas_Qualification_PF where Deactivate=0");
                dt_ResumeSource = objcon.GetDataTable("Select ResumeSourceId as Id,ResumeSource as Name from Recruitment_ResumeSource where deactivate=0");
                var GetCurrectDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                StringBuilder strBuilder = new StringBuilder();
                string abc = "";
                int GetAgencyAssignId = 0;
                if (AgencyAssignId == 1)
                {
                    GetAgencyAssignId = Convert.ToInt32(Session["EmployeeId"]);
                }
                else
                {
                    GetAgencyAssignId = AgencyAssignId;
                }
                int i = 0;
                if (i == 0)
                {
                    strBuilder.Append("DECLARE @InsertedId TABLE(ID INT); ");
                }

                var EmployeeId = Session["EmployeeId"];

                foreach (var Data in BulkObj)
                {
                    List_Qualification_Id = 0;


                    Qualification_Id = 0;


                    if (Data.HighestQualification == null)
                    {
                        var Message = "Enter Qualification name in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    GetQualification_Id(Data.HighestQualification.ToString(), out Qualification_Id);
                    List_Qualification_Id = Qualification_Id;
                    if (List_Qualification_Id == 0)
                    {
                        var Message = "Qualification name not valid in SrNo " + Data.SrNo;
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                    }

                    if (Data.ResumeSource != null)
                    {
                        GetResumeSource_Id(Data.ResumeSource.ToString(), out ResumeSource_Id);
                        List_ResumeSource_Id = ResumeSource_Id;
                        if (List_Qualification_Id == 0)
                        {
                            var Message = "Resume name not valid in SrNo " + Data.SrNo;
                            var Icon = "error";
                            var rowno = Data.SrNo;
                            return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        List_ResumeSource_Id = 0;
                    }


                    int calculatedAge = 0;

                    try
                    {
                        if (Data.DateOfBirth != DateTime.MinValue)
                        {
                            var birthDate = Data.DateOfBirth;
                            var today = DateTime.Today;

                            calculatedAge = today.Year - birthDate.Year;

                            if (birthDate > today.AddYears(-calculatedAge))
                                calculatedAge--;

                            if (calculatedAge < 18 || calculatedAge > 58)
                            {
                                var Message = $"Candidate must be between 18 and 58 years old. Invalid Birthdate at SrNo {Data.SrNo}";
                                return Json(new { Message, Icon = "error", rowno = Data.SrNo }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            var Message = $"DOB is required in SrNo {Data.SrNo}";
                            return Json(new { Message, Icon = "error", rowno = Data.SrNo }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        var Message = "Error validating Birthdate at SrNo " + Data.SrNo;
                        return Json(new { Message, Icon = "error", rowno = Data.SrNo }, JsonRequestBehavior.AllowGet);
                    }
                    if (Data.Age == 0)
                    {

                        var Message = $"Enter Age in SrNo {Data.SrNo}";
                        var Icon = "error";
                        var rowno = Data.SrNo;
                        return Json(new { Message, Icon, rowno }, JsonRequestBehavior.AllowGet);

                    }


                    string currentlyWorking = Data.CurrentlyWorking == "No" ? "0" : "1"; // SQL expects bit value (0 or 1)
                    string balanceNoticePeriod = Data.BalanceNoticePeriod != null ? Data.BalanceNoticePeriod.ToString() : "NULL";
                    string suitableForDesignation = !string.IsNullOrWhiteSpace(Data.SutableForDesignation) ? Data.SutableForDesignation : "NULL";
                    string hrRanking = Data.HRRanking != null ? Data.HRRanking.ToString() : "NULL";

                    string SetUserRights = $@"
        INSERT INTO Recruitment_Resume (
            CreatedBy, CreatedDate, MachineName, DocDate, Resume_ResourceId,
            Salutation, CandidateName, MobileNo, AlternateMobileNo, EmailId,
            DOB, Age, Gender, CurrentCity, MaritalStatus, HighestQualification,
            QualificationRemark, Nationality, FamilyDetails, CurrentlyWorking,
            LastCompanyName, TotalExperience, RelevantExperience, LastDesignation,
            ServingNoticePeriod, NoticePeriodDays, SutableForDesignation,
            TechnicalDetails, RelevantSkill, CommunicationSkill, CTC, ExpectedCTC,
            HoldingAnyOffer, OfferRemark, ExpatLocal, ReadyToTraval, LeaglBoand,
            JobDescription, Origin, ResumeStatus, IsManualAuto, ResumeSource,
            HrSelection, HrRanking, HRComment, ResumeUploadEmployeeId, CandidateType,
            ResumeAgencyId
        )
        OUTPUT INSERTED.ResumeId INTO @InsertedId
        VALUES (
            '{Session["EmployeeName"]}', '{GetCurrectDate}', '{Dns.GetHostName()}',
            '{GetCurrectDate}', {Session["ResourceId"]}, '{Data.Salutation}',
            '{Data.CandidateName}', '{Data.MobileNo}', '{Data.AlternateMobileNo}',
            '{Data.EmailId}', '{Convert.ToDateTime(Data.DateOfBirth):yyyy-MM-dd}',
            {calculatedAge}, '{Data.Gender}', '{Data.CurrentCity}', '{Data.MaritalStatus}',
            '{List_Qualification_Id}', '{Data.SpecificQualification}',
            '{Data.Nationality}', '{Data.FamilyInfo}', {currentlyWorking},
            '{Data.CurrentCompanyName}', '{Data.TotalExperience}', '{Data.RelevantExperience}',
            '{Data.CurrentDesignation}', '{Data.NoticePeriodApplicable}',
            {balanceNoticePeriod}, {suitableForDesignation},
            '{Data.TechnicalDetails}', '{Data.RelevantSkill}', '{Data.CommunicationSkill}',
            '{Data.CurrentCTC}', '{Data.ExpectedCTC}', '{Data.HoldingAnyOffers}',
            '{Data.OfferRemark}', '{Data.LocalOrOutstation}', '{Data.ReadyToTravel}',
            '{Data.LegalBondApplicable}', '{Data.JobDescriptions}', 'BulkCandidateUpload',
            'Pending', 'Manual', {List_ResumeSource_Id}, '{Data.HRSelection}',
            {hrRanking}, '{Data.HRComment}', {EmployeeId},
            '{Data.CandidateType}', '{GetAgencyAssignId}'
        );

        UPDATE Recruitment_Resume
        SET 
            ResumeId_Encrypted = master.dbo.fn_varbintohexstr(HashBytes('SHA2_256', CONVERT(NVARCHAR(70), ID))),
            DocNo = ID
        FROM Recruitment_Resume RR
        INNER JOIN @InsertedId i ON RR.ResumeId = i.ID;

        DELETE FROM @InsertedId;
    ";

                    strBuilder.Append(SetUserRights);
                    i++;
                }


                if (objcon.SaveStringBuilder(strBuilder, out abc))
                {
                    TempData["Message"] = "Record Save successfully";
                    TempData["Icon"] = "success";
                }
                if (abc != "")
                {
                    TempData["Message"] = abc;
                    TempData["Icon"] = "error";
                }

                return Json(new { Message = TempData["Message"], Icon = TempData["Icon"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region ImportExcelFile
        [HttpPost]
        public ActionResult ESS_Recruitment_BulkCandidateData(HttpPostedFileBase AttachFile)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                DynamicParameters paramAgency = new DynamicParameters();
                paramAgency.Add("@query", "select AgencyId as Id,AgencyName as Name from Recruitment_Agency where Deactivate=0");
                var AgencyAssign = DapperORM.ExecuteSP<AllDropDownBind>("sp_QueryExcution", paramAgency).ToList();
                ViewBag.AgencyAssign = AgencyAssign;
                //List<Recruitment_Resume> excelDataList = new List<Recruitment_Resume>();
                ViewBag.GetExceldata = null;
                List<BulkCandidateData> excelDataList = new List<BulkCandidateData>();
                if (AttachFile.ContentLength > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var attachStream = AttachFile.InputStream)
                        {
                            attachStream.Position = 0;

                            XLWorkbook xlWorkwook = new XLWorkbook(attachStream);

                            xlWorkwook.SaveAs(stream);  // Save the workbook to memory stream

                            int row = 2; // Starting row
                            List<string> duplicateRows = new List<string>(); // To store details of duplicate rows

                            // Check if the first column is empty
                            if (xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString() == "")
                            {
                                TempData["Message"] = "Fill in missing information in the first column.";
                                TempData["Title"] = "Empty First Column Detected in Excel Sheet";
                                TempData["Icon"] = "error";
                                return RedirectToAction("ESS_Recruitment_BulkCandidateData", "ESS_Recruitment_BulkCandidateData");
                            }

                            while (!xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).IsEmpty())
                            {

                                // Create and populate the BulkInsert object

                                //Recruitment_Resume BulkInsert = new Recruitment_Resume
                                //{
                                //    SrNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetValue<int>(),
                                //    Salutation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString(),
                                //    CandidateName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString(),
                                //    EmailId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString(),
                                //    MobileNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString(),
                                //    AlternateMobileNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString(),
                                //    Gender = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetString(),
                                //    DOB = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).IsEmpty()
                                //        ? DateTime.Now.ToString("yyyy-MM-dd")
                                //        : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).GetDateTime().ToString("yyyy-MM-dd"),
                                //    Age = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9).GetValue<int>(),
                                //    CurrentCity = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 10).GetString(),
                                //    MaritalStatus = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 11).GetString(),
                                //    HighestQualification = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 12).GetString(),
                                //    QualificationRemark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 13).GetString(),
                                //    Nationality = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 14).GetString(),
                                //    TotalExperience = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 15).GetString(),
                                //    RelevantExperience = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 16).GetString(),
                                //    CTC = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 17).GetString(),
                                //    ExpectedCTC = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 18).GetString(),
                                //    OfferRemark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 19).GetString(),
                                //    CurrentlyWorking = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 20).GetString(),
                                //    LastWorkingDay = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 21).IsEmpty()
                                //        ? DateTime.Now.ToString("yyyy-MM-dd")
                                //        : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 21).GetDateTime().ToString("yyyy-MM-dd"),
                                //    LastCompanyName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 22).GetString(),
                                //    BondMonth = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 23).GetString(),
                                //    LeaglBoand = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 24).GetString(),
                                //    NoticePeriodDays = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 25).GetDouble(),
                                //    ServingNoticePeriod = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 26).GetString(),
                                //    HoldingAnyOffer = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 27).GetString(),
                                //    CommunicationSkill = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 28).GetString(),
                                //    JobDescription = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 29).GetString(),
                                //    ReadyToTraval = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 30).GetString(),
                                //    Refreance = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 31).GetString(),
                                //    HrSelection = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 32).GetString(),
                                //    HrRanking = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 33).GetString(),
                                //    HRComment = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 34).GetString()

                                //};

                                DateTime dob = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).IsEmpty()
    ? DateTime.MinValue
    : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetDateTime();

                                int age = 0;

                                if (dob != DateTime.MinValue)
                                {
                                    var today = DateTime.Today;
                                    age = today.Year - dob.Year;

                                    if (dob > today.AddYears(-age))
                                        age--;
                                }

                                BulkCandidateData BulkInsert = new BulkCandidateData
                                {
                                    //SrNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetValue<int>(),
                                    Salutation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 1).GetString(),
                                    CandidateName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 2).GetString(),
                                    MobileNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 3).GetString(),
                                    AlternateMobileNo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 4).GetString(),
                                    EmailId = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 5).GetString(),
                                    Gender = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 6).GetString(),
                                    //DateOfBirth = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetDateTime(),
                                    DateOfBirth = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).IsEmpty() ? DateTime.Now : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 7).GetDateTime(),
                                    //Age = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).IsEmpty() ? 18 : xlWorkwook.Worksheets.Worksheet(1).Cell(row, 8).GetValue<int>(),
                                    Age = age,
                                    CurrentCity = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 9).GetString(),
                                    MaritalStatus = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 10).GetString(),
                                    HighestQualification = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 11).GetString(),
                                    SpecificQualification = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 12).GetString(),
                                    Nationality = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 13).GetString(),
                                    FamilyInfo = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 14).GetString(),
                                    CandidateType = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 15).GetString(),
                                    CurrentlyWorking = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 16).GetString(),
                                    CurrentCompanyName = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 17).GetString(),
                                    TotalExperience = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 18).GetString(),
                                    RelevantExperience = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 19).GetString(),
                                    CurrentDesignation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 20).GetString(),
                                    SutableForDesignation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 21).GetString(),
                                    TechnicalDetails = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 22).GetString(),
                                    CommunicationSkill = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 23).GetString(),
                                    RelevantSkill = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 24).GetString(),
                                    ReadyToTravel = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 25).GetString(),
                                    CurrentCTC = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 26).GetString(),
                                    ExpectedCTC = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 27).GetString(),
                                    LocalOrOutstation = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 28).GetString(),
                                    JobDescriptions = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 29).GetString(),
                                    NoticePeriodApplicable = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 30).GetString(),
                                    BalanceNoticePeriod = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 31).GetString(),
                                    LegalBondApplicable = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 32).GetString(),
                                    BalanceLegalBondMonths = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 33).GetString(),
                                    HoldingAnyOffers = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 34).GetString(),
                                    OfferRemark = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 35).GetString(),
                                    HRSelection = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 36).GetString(),
                                    HRRanking = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 37).GetString(),
                                    ResumeSource = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 38).GetString(),
                                    HRComment = xlWorkwook.Worksheets.Worksheet(1).Cell(row, 39).GetString()
                                };
                                // Check for duplicates in the existing list
                                bool isDuplicate = excelDataList.Any(c =>
                                    c.EmailId == BulkInsert.EmailId ||
                                    c.MobileNo == BulkInsert.MobileNo);

                                if (isDuplicate)
                                {
                                    duplicateRows.Add($"Row {row}: Duplicate entry found for Email: {BulkInsert.EmailId}, Mobile: {BulkInsert.MobileNo}");
                                }
                                else
                                {
                                    excelDataList.Add(BulkInsert);
                                }

                                // If duplicates found, return error message
                                if (duplicateRows.Any())
                                {
                                    TempData["Message"] = $"Duplicate entries found:\n{string.Join("\n", duplicateRows)}";
                                    TempData["Title"] = "Duplicate Entries Detected";
                                    TempData["Icon"] = "error";
                                    return RedirectToAction("ESS_Recruitment_BulkCandidateData", "ESS_Recruitment_BulkCandidateData");
                                }
                                row++;
                            }
                        }
                    }

                    ViewBag.count = 1;
                    ViewBag.GetExceldata = excelDataList;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion

        #region GetQualification_Id
        void GetQualification_Id(string Name, out int Qualification_Id)
        {
            try
            {
                var matchingRows = from row in dt_Qualifcation.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varQualification_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varQualification_Id == null)
                {
                    Qualification_Id = 0;
                }
                else
                {
                    Qualification_Id = Convert.ToInt32(varQualification_Id["Id"]);
                }
            }
            catch
            {
                Qualification_Id = 0;
            }
        }


        void GetResumeSource_Id(string Name, out int ResumeSource_Id)
        {
            try
            {
                var matchingRows = from row in dt_ResumeSource.AsEnumerable()
                                   where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                                   select row;

                var varResumeSource_Id = matchingRows.LastOrDefault();

                //var varCompany_Id = (from row in dt_company.AsEnumerable()
                //                     where string.Equals(row.Field<string>("Name"), Name, StringComparison.OrdinalIgnoreCase)
                //                     select row).Last();
                if (varResumeSource_Id == null)
                {
                    ResumeSource_Id = 0;
                }
                else
                {
                    ResumeSource_Id = Convert.ToInt32(varResumeSource_Id["Id"]);
                }
            }
            catch
            {
                ResumeSource_Id = 0;
            }
        }
        #endregion

    }
}