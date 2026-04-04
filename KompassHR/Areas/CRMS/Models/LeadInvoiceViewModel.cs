using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.CRMS.Models
{
    public class LeadInvoiceViewModel
    {
        public string LeadCreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string LeadSource { get; set; }
        public string Summary { get; set; }

        public string ContactPerson { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Stage { get; set; }

        public int LeadOwnerCount { get; set; }
        public int TransferByCount { get; set; }
        public int TotalCount => LeadOwnerCount + TransferByCount;

        public int opportunityCount { get; set; }
        public int opportunityTransferByCount { get; set; }
        public int opportunityTotalCount => opportunityCount + opportunityTransferByCount;

    }

    public class ACCMAIN
    {
        public string IndustryType { get; set; }
        public string LeadSource { get; set; }
        public string EmployeeName { get; set; }
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
        public string LeadPriority { get; set; }
        public string LeadName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyCategory { get; set; }
        public string OrganisationType { get; set; }
        public int MAS_LEADSOURCE_Fid { get; set; }
        public string LeadType { get; set; }
        public int MAS_INDUSTRYTYPE_Fid { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyContactNumber { get; set; }
        public string CINNo { get; set; }
        public string Summary { get; set; }
        public double CompanyRating { get; set; }
        public int MAS_STAGES_FID { get; set; }
        public string Stage { get; set; }
        public string LegalEntityName { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public bool RateRevision { get; set; }
        public int DurationInMonths { get; set; }
        public bool NeedsPatrolling { get; set; }
        public string PatrollingNotes { get; set; }
        public int PaymentTermsInDays { get; set; }
        public bool PenaltyApplicable { get; set; }
        public bool Deleted { get; set; }
        public int TranferBy_Fid { get; set; }
        public DateTime TransferDate { get; set; }
        public bool IsCoOwner { get; set; }
        public string VendorName { get; set; }
        public string VendorCategory { get; set; }
        public string VendorRegistrationNo { get; set; }
        public string PFCodeNo { get; set; }
        public string ESICNo { get; set; }
        public string PTRegistrationNo { get; set; }
        public string PTRCNo { get; set; }
        public string LWFNo { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public string ShopActNo { get; set; }
        public int ShopActManpowerCount { get; set; }

        public string BusinessUnit { get; set; }
        public string AccStage { get; set; }
        public bool IsDirectAccount { get; set; }
        public string ReferenceName { get; set; }

        public bool TenderApplicable { get; set; }
        public string TenderNumber { get; set; }
        public string Tendersource { get; set; }
        public string Website { get; set; }

        public string CountryCode { get; set; }

        public int ECVCurrency { get; set; }
        public int ExpectedContractValue { get; set; }

        public string CompanyLogo { get; set; }
        public HttpPostedFileBase CompanyLogoPic { get; set; }
        public int CmpId { get; set; }

    }

    public class ACCSITES
    {
        public double Fid { get; set; }
        public double ACCMAIN_Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public string SiteName { get; set; }
        public string BUCode { get; set; }
        public string SiteCode { get; set; }
        public string SiteStaus { get; set; }
        public string InterviewRequired { get; set; }
        public bool IsWeekOff { get; set; }
        public string SiteGrading { get; set; }
        public string SiteType { get; set; }

        public DateTime SiteOpenDate { get; set; }
        public DateTime SiteOJTDate { get; set; }
        public DateTime SiteCloseDate { get; set; }
        public string SiteCloseReason { get; set; }
        public int StatusChangeBy { get; set; }
        public DateTime SiteStatusChangeDate { get; set; }

        public bool IsInventory { get; set; }
        public string InventoryBy { get; set; }
        public string Sitelogo { get; set; }

    }

    public class MAS_LEADSOURCE
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string LeadSource { get; set; }
    }

    public class MAS_SERCATG
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string ServiceCategory { get; set; }
    }

    public class MAS_SERVICE
    {
        public int? Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public int MAS_SERCATG { get; set; }
        public string Service { get; set; }
        public string ServiceCategory { get; set; }
    }

    public class ACCSERVICES
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string ServiceCategory { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int BUCode { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int MAS_SERVICE { get; set; }
        public bool Deleted { get; set; }
        public int ACCLOCATION_Fid { get; set; }

        public int ServiceCount { get; set; }

    }

    public class MAS_DEPT
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string Department { get; set; }
        public bool Deleted { get; set; }
    }

    public class MAS_DESG
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string Designation { get; set; }
        public bool IsCRM { get; set; }
        public bool Deleted { get; set; }
    }

    public class MAS_COUNTRY
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string Currency { get; set; }
        public string Flag { get; set; }
        public HttpPostedFileBase FlagFile { get; set; }

    }

    public class MAS_STATE
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public int MAS_COUNTRY_Fid { get; set; }
        public int MAS_ZONE_Fid { get; set; }
        public string StateName { get; set; }
        public string Zones { get; set; }
        public string CountryName { get; set; }
        public string Origin { get; set; }
    }

    public class ACCCONTACT
    {
        public string Encrypted_Id_User { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Designation_name { get; set; }
        public string Department_name { get; set; }
        public string CompanyName { get; set; }
        public string LeadName { get; set; }
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public string BUCode { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public string ContactPerson { get; set; }
        public string MobileNo { get; set; }
        public string WhatsappNo { get; set; }
        public string EmailId { get; set; }
        public int Designation { get; set; }
        public int Department { get; set; }
        public bool Deleted { get; set; }
        public bool IsPrimaryContact { get; set; }

        public int ContactCount { get; set; }
        public string CountryCode { get; set; }
        public string LinkedInProfile { get; set; }
        public int Sitelist { get; set; }

    }

    public class MAS_STAGES
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string Stage { get; set; }
        public string Origin { get; set; }
        public string Level { get; set; }
        public bool Deleted { get; set; }
    }

    public class MAS_EMPLOYEES
    {
        public string TrackTimeInSec { get; set; }
        public string TrackTimeInMin { get; set; }
        public int BUCode { get; set; }
        public string CompanyName { get; set; }
        public string VendorName { get; set; }
        public string SiteName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string EmployeeCategory { get; set; }
        public string EmployeeName { get; set; }
        public string MobileNo { get; set; }
        public string AlternateMobNo { get; set; }
        public string BloodGroup { get; set; }
        public string Gender { get; set; }
        public string EmailId { get; set; }
        public DateTime JoiningDate { get; set; }
        public int ACCSITES_Fid { get; set; }
        public string EmployeeId { get; set; }
        public int MAS_DEPT { get; set; }
        public int MAS_DESG { get; set; }
        public int ATD_SHIFTGROUP_Fid { get; set; }
        public int ATD_SHIFTRULES { get; set; }
        public int ReportingLevel { get; set; }
        public bool IsTrack { get; set; }
        public TimeSpan TrackTime { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public string ProfilePic { get; set; }

        public string BiometricId { get; set; }
        public string ESICNo { get; set; }
        public string UANNo { get; set; }
        public bool PoliceVerificationStatus { get; set; }
        public string PoliceVerificationNo { get; set; }
        public DateTime? PoliceVerificationValidDate { get; set; }
        public long? BankAccNo { get; set; }
        public bool MedicalHealthCheckup { get; set; }
        public DateTime? MedicalHealthCheckupDate { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }


        public long? AadharNo { get; set; }
        public string NameAsPerAadhar { get; set; }
        public string PANNo { get; set; }
        public DateTime? BirthDate { get; set; }


        public string PermanentAddress { get; set; }
        public int PermanentPincode { get; set; }
        public string CurrentAddress { get; set; }
        public int CurrentPincode { get; set; }
        public string ActiveStatus { get; set; }
        public string SiteStatus { get; set; }

        public string WeekOff { get; set; }

        public int MAS_BUSITES_Fid { get; set; }

        public int ReportingManager { get; set; }
    }

    public class ACCDMS
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserId { get; set; }
        public int? ACCMAIN_Fid { get; set; }
        public string Origin { get; set; }
        public string DocumentName { get; set; }
        public string Remark { get; set; }
        public string AttachmentURL { get; set; }
        public string Stage { get; set; }
        public int? Deleted { get; set; }
    }

    public class ACCSTAGES
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserId { get; set; }
        public int? ACCMAIN_Fid { get; set; }
        public int? MAS_STAGES_Fid { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool? Deleted { get; set; }
        public string Stage { get; set; }
    }

    public class ACCCOMM
    {
        public double Fid { get; set; }
        public int BUCode { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int ACCCONTACT_Fid { get; set; }
        public DateTime CommunicationDate { get; set; }
        public string CommunicationType { get; set; }
        public string HeadLine { get; set; }
        public string Description { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public string FollowUpRemark { get; set; }
        public string FollowUpStatus { get; set; }
        public bool Deleted { get; set; }

        public int CommunicationCount { get; set; }

    }

    public class ACCLOCATION
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int BUCode { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int MAS_COUNTRY_Fid { get; set; }
        public int MAS_STATE_Fid { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
        public bool IsHeadOffice { get; set; }
        public bool Deleted { get; set; }

        public string Pincode { get; set; }
        public string MAS_COUNTRY_Fid1 { get; set; }
        public string MAS_STATE_Fid1 { get; set; }
        public int LocationCount { get; set; }
    }

    public class ACCACTIVITY
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int MyProperty { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int? ACCCONTACT_Fid { get; set; }
        public string Origin { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public string ActivityDate { get; set; }
        public string Status { get; set; }
        public string Attachment { get; set; }
        public bool Deleted { get; set; }

        public int ActivityCount { get; set; }
        public string ShareAcctEmployee { get; set; }
        public bool ShareAcctivityToOperation { get; set; }

        public bool TodoCheck { get; set; }

    }

    public class PINCODES
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public int UserId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PIN { get; set; }
        public string Area { get; set; }
        public bool Deleted { get; set; }
        public double PinId { get; set; }
        public string PinCode { get; set; }
        public string OfficeName { get; set; }
        public string StateName { get; set; }
    }

    public class ACCPROPOSAL
    {

        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int BUCode { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public string ProposalNo { get; set; }
        public int RevisionNo { get; set; }
        public DateTime? ProposalSubmissionDate { get; set; }
        public DateTime? ExpectedClosingDate { get; set; }
        public DateTime? RevisedProposalDate { get; set; }
        public string TypeOfContract { get; set; }
        public int ManpowerCount { get; set; }
        public int ContractTenure { get; set; }
        public float TotalContractValue { get; set; }
        public float MonthlyContractValue { get; set; }
        public float AnnualContractValue { get; set; }
        public bool TaxApplicable { get; set; }
        public float ACVInclusiveOfTax { get; set; }
        public float ManagementFeePer { get; set; }
        public float ManagementFeeValue { get; set; }
        public string ProposalFileAttachement { get; set; }
        public bool Deleted { get; set; }

        public string TaxPercentage { get; set; }

        public int Currency { get; set; }
    }

    public class MAS_INDUSTRYTYPE
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string IndustryType { get; set; }
    }

    public class Ocean
    {

        public int BranchId { get; set; }

        public string BranchName { get; set; }

        public int Fid { get; set; }

        public string Encrypted_Id { get; set; }

        public DateTime FormDate { get; set; }

        public string MachineId { get; set; }

        public string MachineIp { get; set; }

        public int OceanFMId { get; set; }

        public string ContactPerson { get; set; }

        public string CompanyName { get; set; }

        public string LegalEntityName { get; set; }


        public string ShortName { get; set; }

        public string Address { get; set; }

        public string TINNo { get; set; }

        public string GST_VATNo_ { get; set; }

        public string CINNo { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string NatureOfBusiness { get; set; }

        public string Website { get; set; }

        public DateTime EstablishDate { get; set; }

        public string Logo { get; set; }

        public HttpPostedFileBase File { get; set; }

        public bool Active { get; set; }
        public bool Deactivate { get; set; }
    }

    public class ACCCONTRACT
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public string LegalEntityName { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public int ContractTenure { get; set; }
        public float TotalContractValue { get; set; }
        public float MonthlyContractValue { get; set; }
        public float AnnualContractValue { get; set; }
        public float ACVAmountIncludingTax { get; set; }
        public float ManagementFeeInPercent { get; set; }
        public float ManagementFee { get; set; }
        //public DateTime InvoiceGenDate { get; set; }
        //public DateTime InvoiceDispatchDate { get; set; }
        public bool CustomerAcknowledge { get; set; }
        public DateTime OJTDate { get; set; }
        public DateTime SiteMobilizationDate { get; set; }
        public bool RateRevision { get; set; }
        public int DurationInMonths { get; set; }
        public int PaymentTermsInDays { get; set; }
        public bool PenaltyApplicable { get; set; }
        public bool IsContractExtended { get; set; }
        public DateTime? ContractExtendedDate { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string ACCSERVICES_Fid { get; set; }
        public string ACCLOCATION_Fid { get; set; }
        public int ContractNo { get; set; }
        public int InventoryAmount { get; set; }
        public int MonthlyBillingManpower { get; set; }
        public int AreainSquareFeet { get; set; }
        public int GSTRate { get; set; }
        public int ManpowerCount { get; set; }
        public bool InventoryIncluded { get; set; }

        public decimal MonthlyBillingAmount { get; set; }

        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public string SiteStaus { get; set; }
        public bool InterviewRequired { get; set; }
        public bool IsWeekOff { get; set; }
        public string SiteGrading { get; set; }
        public string SiteType { get; set; }

        public DateTime SiteOpenDate { get; set; }
        public DateTime SiteOJTDate { get; set; }
        public DateTime SiteCloseDate { get; set; }
        public string SiteCloseReason { get; set; }
        public DateTime StatusChangeBy { get; set; }
        public DateTime SiteStatusChangeDate { get; set; }
        public float TaxPercentage { get; set; }
        public string Site_Encrypted_Id { get; set; }
        public string Contract_Encrypted_Id { get; set; }
        public int ACCCONTRACT_Fid { get; set; }


        public int LocationCount { get; set; }
        public int InvoiceGenDate { get; set; }
        public int InvoiceDispatchDate { get; set; }

        public int Currency { get; set; }
        public string Attachment { get; set; }
        public HttpPostedFileBase AttachmentFile { get; set; }
        public string ContractAttachment { get; set; }
        public HttpPostedFileBase ContractAttachmentFile { get; set; }

        public int MAS_BUSITES { get; set; }

        public int? AssociateTraining { get; set; }
        public string TrainingFrequency { get; set; }
        public bool TrainingPenalty { get; set; }
        public bool IsInventory { get; set; }
        public string InventoryBy { get; set; }
    }

    public class ACCMANPOWER
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int ACCCONTRACT_Fid { get; set; }
        public int MAS_DESG_Fid { get; set; }
        public int ManpowerCount { get; set; }
        public Boolean Deleted { get; set; }


        public int MaleManpowerCount { get; set; }
        public int FemaleManpowerCount { get; set; }
        public int MaleEarning { get; set; }
        public int FemaleDeduction { get; set; }
        public int FemaleEarning { get; set; }
        public int MaleDeduction { get; set; }
        public int NoOfDaysInMonth { get; set; }
        public int MaxAllowOT { get; set; }

        public int MAS_BUSITES_Fid { get; set; }

    }

    public class ACCOWNERS
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserId { get; set; }
        public int? ACCMAIN_Fid { get; set; }
        public int? LeadOwner_Fid { get; set; }
        public DateTime? OwnershipDate { get; set; }
        public int? TranferBy_Fid { get; set; }
        public DateTime? TransferDate { get; set; }
        public bool IsCoOwner { get; set; }
        public bool? Deleted { get; set; }

        public string CompanyName { get; set; }
        public string AccFid { get; set; }
        public string TransferRemark { get; set; }
    }

    public class MAS_REPORT
    {
        public int Fid { get; set; }
        public Nullable<System.DateTime> Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string Encrypted_Id { get; set; }
        public Nullable<int> BusinessNameId { get; set; }
        public string Module { get; set; }
        public string ReportName { get; set; }
        public string ViewName { get; set; }
        public string Description { get; set; }
        public Nullable<int> Set_Module_Fid { get; set; }
        public Nullable<bool> Deleted { get; set; }

        public string SubModule { get; set; }
        public string RptMapEncId { get; set; }


    }

    public class ACCSITELOCT
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public string LocationName { get; set; }
        public string GPSLattitude { get; set; }
        public string GPSLongitude { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string UserId { get; set; }
        public int Ocean_Fid { get; set; }
        public int MAS_BUSITES_Fid { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public bool Active { get; set; }

        public string Address { get; set; }
        public int ACCSITES_Fid { get; set; }
        public bool IsAttendanceLocation { get; set; }
        public string SiteName { get; set; }

    }

    public class ACCINVOICE
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public string SiteName { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUCode { get; set; }
        public int UserID { get; set; }
        public int ACCSITES_Fid { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public DateTime? BillMonth { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double InvoiceBasicAmount { get; set; }
        public double InvoiceTaxAmount { get; set; }
        public double InvoicePaidAmount { get; set; }
        public double InvoiceBalanceAmount { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime AcknowledgedDate { get; set; }
        public string AcknowledgedRemark { get; set; }
        public string InvoiceCopy { get; set; }
        public string SupportingDocument { get; set; }
        public HttpPostedFileBase InvoiceCopyFile { get; set; }
        public HttpPostedFileBase SupportingDocumentFile { get; set; }
        public bool Deleted { get; set; }

        public string InvoiceBasicAmountstring { get; set; }
        public string InvoiceTaxAmountstring { get; set; }
        public string InvoicePaidAmountstring { get; set; }
        public string InvoiceBalanceAmountstring { get; set; }
        public string InvoiceTaxPercentagestring { get; set; }
        public int? MAS_BUSITES { get; set; }
    }

    public class ACCSERVICEREQ
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string BUCode { get; set; }
        public string MyProperty { get; set; }
        public int UserId { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public int TypeOfRequirement { get; set; }
        public DateTime RequiredOn { get; set; }
        public DateTime RequiredTill { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
        public int StatusChangeBy { get; set; }
        public DateTime StatusDate { get; set; }
        public string FeedbackRating { get; set; }
        public string FeedbackRemark { get; set; }
        public string Encrypted_Id { get; set; }
        public Boolean Deleted { get; set; }
        public int ACCSITE_Fid { get; set; }
        public string TypeOfServiceRequ { get; set; }
        public string Priority { get; set; }
        public string SiteName { get; set; }

        public string Description { get; set; }
        public int MAS_BUSITES_Fid { get; set; }

    }

    public class MAS_EMPINDUSTYPE
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
        public int MAS_Employee_Fid { get; set; }
        public int MAS_INDUSTRYTYPE { get; set; }
        public bool Deleted { get; set; }
    }

    public class USERS
    {
        public string Encrypted_Id { get; set; }

        public bool IsAttendance { get; set; }

        public bool IsComplaint { get; set; }

        public bool IsPetrolling { get; set; }

        public int Fid { get; set; }

        public DateTime Fdate { get; set; }

        public string Mid { get; set; }

        public string Mip { get; set; }

        public string MainRole { get; set; }

        public int ACCMAIN_Fid { get; set; }

        public int ACCCONTACT_Fid { get; set; }

        public string EmployeeId { get; set; }

        public string Username { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }



        public string Password { get; set; }

        public bool IsComplaintManager { get; set; }

        public bool IsComplaintResponder { get; set; }

        public int IsCRM { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        public int BUCode { get; set; }
    }

    public class BUCodeViewModel
    {
        public int Fid { get; set; }
        public string CompanyName { get; set; }
    }

    public class COMPLAINTSM
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUCode { get; set; }


        public int ComplaintNo { get; set; }
        public string ComplaintBy { get; set; }
        public int Userid { get; set; }
        public int ACCSITES_Fid { get; set; }
        public int ACCMAIN_Fid { get; set; }
        public string ComplaintType { get; set; }
        public string ComplaintTitle { get; set; }
        public string ComplaintDescription { get; set; }
        public string Priority { get; set; }
        public DateTime? ExpectedResolutionTime { get; set; }
        public string ComplaineeName { get; set; }
        public string ComplaineeMobile { get; set; }
        public string ComplaineeAddress { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string FileAttachment1 { get; set; }
        public string FileAttachment2 { get; set; }
        public DateTime TurnAroundDate { get; set; }
        public string TurnAroundTime { get; set; }
        public string TurnAroundTimeUnit { get; set; }
        public string ComplaintLatestStatus { get; set; }
        public DateTime ComplaintLatestStatusDate { get; set; }
        public string Comptype { get; set; }
        public string SiteName { get; set; }
        public string VendorName { get; set; }
        public string message { get; set; }
        public string combinedMessage { get; set; }
        public int ACCCONTACT_Fid { get; set; }

        public string Response { get; set; }
        public int MAS_BUSITES_Fid { get; set; }

        public DateTime ComplaintDate { get; set; }
        public int TotalOpenCount { get; set; }
        public int TotalResolvedCount { get; set; }
        public string ComplaintLocation { get; set; }
    }

    public class EmployeeDto
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class INV_MASTER
    {
        public int Fid { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? UserID { get; set; }
        public string InventoryGroup { get; set; }
        public string ItemName { get; set; }
        public string UOM { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool? Deleted { get; set; }
    }

    public class INV_IN
    {
        public int Fid { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? UserID { get; set; }
        public int? SiteCode { get; set; }
        public int? MasterID { get; set; }
        public string InwardQuantity { get; set; }
        public DateTime? InwardDate { get; set; }
        public string OutwardQuantity { get; set; }
        public DateTime? OutwardDate { get; set; }
        public string BatchNo { get; set; }
        public bool? Active { get; set; }
    }

    public class CHECKLISTR
    {
        public string EmployeeName { get; set; }
        public string ActivityItem { get; set; }
        public string SiteName { get; set; }
        public int Fid { get; set; }
        public DateTime? FDate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? Users_Fid { get; set; }
        public int? AccSites_Fid { get; set; }
        public int? Mas_ChkOps { get; set; }
        public string Marking { get; set; }
        public string Attachment1 { get; set; }
        public string Attachment2 { get; set; }
        public string Attachment3 { get; set; }
        public string Attachment4 { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public bool WithinPerimeter { get; set; }
        public string Status { get; set; }
        public bool ChecklistStatus { get; set; }
        public string Remark { get; set; }
        public DateTime? CompleteDate { get; set; }
        public bool? CompleteStatus { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }


        public double FeedbackRating { get; set; }
        public string Feedback { get; set; }
        public string Topic { get; set; }
        public string TopicNames { get; set; }
        public bool? IsEMPAttendance { get; set; }
        public int? Mas_ChkMops { get; set; }
        public string ActivityName { get; set; }
        public string SiteInTime { get; set; }
        public string SiteOutTime { get; set; }
        public int Mas_ChkMops_Fid { get; set; }
    }

    public class MAS_CHKEMPCHK
    {
        public int Fid { get; set; } // Primary key
        public string Encrypted_Id { get; set; } // nvarchar(70)
        public DateTime? Fdate { get; set; } // datetime
        public string Mid { get; set; } // nvarchar(50)
        public string Mip { get; set; } // nvarchar(50)
        public int? BUCode { get; set; } // int
        public int? UserId { get; set; } // int
        public int? MAS_CHKOPS { get; set; } // int (Foreign Key)
        public bool? EmpOrSite { get; set; } // int (Foreign Key)
        public string ChecklistItem { get; set; } // nvarchar(50)
        public bool? Deleted { get; set; } // bit (boolean)
        public string ActivityItem { get; set; }
        public string ActivityName { get; set; }
    }

    public class OPS_CHKEMPCHKT
    {
        public string EmployeeName { get; set; }

        public string SiteName { get; set; }

        public string ChecklistItem { get; set; }

        public int Fid { get; set; } // Primary key

        public string Encrypted_Id { get; set; } // nvarchar(70)

        public DateTime? Fdate { get; set; } // datetime

        public string Mid { get; set; } // nvarchar(50)

        public string Mip { get; set; } // nvarchar(50)

        public int? BUCode { get; set; } // int

        public int? UserId { get; set; } // int

        public int? MAS_CHKMOPS { get; set; } // int (Foreign Key)

        public int? ACCSITES_Fid { get; set; } // int (Foreign Key)

        public int? CHECKLISTR { get; set; } // int (Foreign Key)

        public int? Mas_Employee_Fid { get; set; } // int (Foreign Key)

        public int? MAS_CHKEMPCHK { get; set; } // int (Foreign Key)

        public string Remark { get; set; }

        public string CustomerRemark { get; set; }

        public int? Rating { get; set; } // int (Foreign Key)

        public bool? Deleted { get; set; } // bit (boolean)
    }

    public class MAS_CHKMOPS
    {
        public int Fid { get; set; }

        public string Encrypted_Id { get; set; }

        public DateTime Fdate { get; set; }

        public string Mid { get; set; }

        public string Mip { get; set; }

        public int UserId { get; set; }

        public int MasActivityGroupFid { get; set; }

        public string ActivityName { get; set; }

        public bool ApplicableOnSite { get; set; }

        public bool ApplicableForPerformanceAppraisal { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        public bool IsEMPAttendance { get; set; }
    }

    public class ATD_OPSATD
    {
        public string SiteName { get; set; }
        public string EmployeeName { get; set; }
        public int Fid { get; set; }
        public string EncryptedId { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? MAS_Employee_Fid { get; set; }
        public int? ACCSITES_Fid { get; set; }
        public int? ACCMAIN_Fid { get; set; }
        public string AttendanceDate { get; set; }
        public string InDate { get; set; }
        public string InTime { get; set; }
        public string InLat { get; set; }
        public string InLong { get; set; }
        public string OutDate { get; set; }
        public string OutTime { get; set; }
        public string OutLat { get; set; }
        public string OutLong { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Selfie { get; set; }
        public int? BatteryStatus { get; set; }
        public bool? Deleted { get; set; }
    }

    public class ATD_ATTENDANCE
    {
        public string SiteName { get; set; }
        public int SiteCode { get; set; }
        public string EmployeeName { get; set; }
        public int Fid { get; set; }
        public string EncryptedId { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? EmployeeNo { get; set; }
        public int? ACCSITES_Fid { get; set; }
        public string AttendanceDate { get; set; }
        public string InDate { get; set; }
        public string InTime { get; set; }
        public string Designation { get; set; }
        public string InLat { get; set; }
        public string InLong { get; set; }
        public string OutDate { get; set; }
        public string OutTime { get; set; }
        public string OutLat { get; set; }
        public string OutLong { get; set; }
        public float TotalHours { get; set; }
        public string Status { get; set; }

        public string Selfie { get; set; }

    }

    public class MAS_OPSSITES
    {
        public int NoOfSiteVisit { get; set; }
        public int UnitOfSiteVisit { get; set; }
        public string SiteName { get; set; }
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? ACCSITE_Fid { get; set; }
        public int? MAS_EMployee_Fid { get; set; }
        public int? Level { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime ActiveTill { get; set; }
        public bool? Active { get; set; }
        public bool? Deleted { get; set; }

    }

    public class MAS_CHKMEASUR
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string SiteName { get; set; }
        public string ActivityName { get; set; }
        public string ReportingRole { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public int ACCSITES_Fid { get; set; }
        public int MAS_CHKMOPS_Fid { get; set; }
        public int MAS_REPORLEVEL_Fid { get; set; }
        public int ActivityCount { get; set; }
        public int ActivityUnit { get; set; }

        public bool Deleted { get; set; }
    }

    public class MAS_REPORLEVEL
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public int ReportingLever { get; set; }
        public string ReportingRole { get; set; }

        public bool Deleted { get; set; }
    }

    public class MAS_COMPLAINTTYPE
    {
        public int? Fid { get; set; }
        public string ComplaintType { get; set; }
        public string ComplaintTypeCount { get; set; }
    }

    public class MAS_CLASSTRGEMP
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserID { get; set; }
        public string CandidateName { get; set; }
        public int? Designation_Fid { get; set; }
        public double? Height { get; set; }
        public int? Weight { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
        public string Qualification { get; set; }
        public int? Experience { get; set; }
        public string MobileNo { get; set; }
        public int? NoOfTrainingDay { get; set; }
        public DateTime? TrainingCompletDate { get; set; }
        public string TrainingStatus { get; set; }
        public int? Rating { get; set; }
        public string RecidentLocation { get; set; }
        public int? ACCSITES_Fid { get; set; }
        public int? ACCSITELOCT_Fid { get; set; }
        public int? AreaManager { get; set; }
        public TimeSpan? ReportedTime { get; set; }
        public string RemarkOrObservation { get; set; }
        public bool? Deleted { get; set; }
        public string Gender { get; set; }
        public string AdharNo { get; set; }
        public string Flag { get; set; }
        public string DesignationName { get; set; }
        public string AreaName { get; set; }
        public DateTime REG_Date { get; set; }
        public int Mas_Area_Fid { get; set; }


    }

    public class MAS_CLASSTRGATD
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserId { get; set; }
        public DateTime? TrainingDate { get; set; }
        public int? MAS_CLASSTRGEMP_Fid { get; set; }
        public int? TrainingCount { get; set; }
        public string TrainingTopic { get; set; }
        public int? Rating { get; set; }
        public string Remark { get; set; }
        public bool? Deleted { get; set; }
    }

    public class MAS_Area
    {
        public double Fid { get; set; }

        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string AreaName { get; set; }

        public bool Deleted { get; set; }
    }

    public class OPS_CHKEMPATT
    {
        public int Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime? Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int? BUCode { get; set; }
        public int? UserId { get; set; }
        public int? MAS_CHKMOPS { get; set; }
        public int? ACCSITES_Fid { get; set; }
        public int? CHECKLISTR { get; set; }
        public int? Mas_Employee_Fid { get; set; }
        public bool? Attendance { get; set; }
        public int? Rating { get; set; }
        public string Remark { get; set; }
        public bool? Deleted { get; set; }
        public string EmployeeName { get; set; }
    }

    public class ATD_OTDETAILS
    {
        public int Fid { get; set; }
        public int ACCMAINID { get; set; }
        public int SiteLocationId { get; set; }
        public int EmployeeId { get; set; }
        public string BioMetricId { get; set; }
        public DateTime OtDate { get; set; }
        public DateTime Fdate { get; set; }
        public TimeSpan Intime { get; set; }
        public TimeSpan OutTime { get; set; }
        public decimal TotalOt { get; set; }
        public bool Approval { get; set; }
        public string Remark { get; set; }
        public string VendorName { get; set; }

    }

    public class NameValue
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class ToDoListModel
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int BUcode { get; set; }
        public int AssignedBy { get; set; }

        public int AssignedTo { get; set; }
        public string PointOut { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public DateTime? AssignDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string File1_Url { get; set; }
        public string File2_Url { get; set; }
        public bool Deleted { get; set; }
        public string TotalTime { get; set; }
        public string EmployeeName { get; set; }
        public string ActivityTime { get; set; }
        public string Encrypted_Id { get; set; }

        public HttpPostedFileBase Img1 { get; set; }
        public HttpPostedFileBase Img2 { get; set; }
    }


    public class SET_EMAILSETTINGS
    {
        public int Fid { get; set; }
        public string Origin { get; set; }
        public string SmtpServerName { get; set; }
        public string PortNo { get; set; }
        public bool SSL { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public string HREmailID { get; set; }
        public string Link { get; set; }

    }

}

