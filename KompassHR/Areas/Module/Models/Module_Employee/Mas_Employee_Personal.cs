using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Personal
    {
        public double PersonalId { get; set; }
        public string PersonalId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PersonalEmployeeId { get; set; }
        public string AadhaarNo { get; set; }
        public string NameAsPerAadhaar { get; set; }
        public bool AadhaarNoMobileNoLink { get; set; }
        public string AadhaarNoMobileNo { get; set; }
        public string PAN { get; set; }
        public string NameAsPerPan { get; set; }
        public bool PANAadhaarLink { get; set; }
        public string PrimaryMobile { get; set; }
        public string SecondaryMobile { get; set; }
        public string WhatsAppNo { get; set; }
        public string PersonalEmailId { get; set; }
        public DateTime BirthdayDate { get; set; }
        public string AgeOfJoining { get; set; }
        public string BirthdayPlace { get; set; }
        public string BirthdayProofOfDocumentID { get; set; }
        public string BirthdayProofOfCertificateNo { get; set; }
        public bool IsDOBSpecial { get; set; }
        public double EmployeeQualificationID { get; set; }
        public string QualificationRemark { get; set; }
        public string Gender { get; set; }
        public string BloodGroup { get; set; }
        public string MaritalStatus { get; set; }
        public string AnniversaryDate { get; set; }
        public bool Ifyouwantdonotdisclosemygenderthentick { get; set; }
        public string EmployeeSpecificDegree { get; set; }
        public string EmployeeBirthProofEducation { get; set; }
        public string PhysicallyDisabled { get; set; }
        public string PhysicallyDisableType { get; set; }
        public string PhysicallyDisableRemark { get; set; }
        public string IdentificationMark { get; set; }
        public string DrivingLicenceNo { get; set; }
        public string DrivingLicenceExpiryDate { get; set; }
        public string PassportNo { get; set; }
        public string PassportExpiryDate { get; set; }
        public double EmployeeReligionID { get; set; }
        public double EmployeeCasteID { get; set; }
        public string EmployeeSubCategory { get; set; }
        public bool Ifyouwantdonotdisclosemyreligioncastthentick { get; set; }
    }
}