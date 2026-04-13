using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting
{
    public class Mas_Employee_Import
    {
        //=============================== Start Mas_Employee====================================

        public int EmployeeId { get; set; }
            public string EmployeeId_Encrypted { get; set; }
            public bool Deactivate { get; set; }
            public bool UseBy { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string MachineName { get; set; }
            public double CmpID { get; set; }
            public double EmployeeBranchId { get; set; }
            public string EmployeeOrigin { get; set; }
            public string EmployeeSeries { get; set; }
            public string EmployeeNo { get; set; }
            public string EmployeeCardNo { get; set; }
            public bool IsNRI { get; set; }
            public string Salutation { get; set; }
            public string EmployeeName { get; set; }
            public double EmployeeLevelID { get; set; }
            public double EmployeeWageID { get; set; }
            public double EmployeeDepartmentID { get; set; }
            public double EmployeeSubDepartmentName { get; set; }
            public double EmployeeDesignationID { get; set; }
            public double EmployeeGradeID { get; set; }
            public double EmployeeGroupID { get; set; }
            public double EmployeeTypeID { get; set; }
            public double EmployeeCostCenterID { get; set; }
            public double EmployeeZoneID { get; set; }
            public double EmployeeUnitID { get; set; }
            public double ContractorID { get; set; }
            public double EmployeeLineID { get; set; }
            public DateTime? JoiningDate { get; set; }
            public bool IsJoiningSpecial { get; set; }
            public string JoiningStatus { get; set; }
            public DateTime? TraineeDueDate { get; set; }
            public DateTime? ProbationDueDate { get; set; }
            public DateTime? ConfirmationDate { get; set; }
            public bool IsConfirmation { get; set; }
            public string ConfirmationBy { get; set; }
            public string CompanyMobileNo { get; set; }
            public string CompanyMailID { get; set; }
            public double ReportingHR { get; set; }
            public double ReportingManager1 { get; set; }
            public double ReportingManager2 { get; set; }
            public double ReportingAccount { get; set; }
            public double NoOfBranchTransfer { get; set; }
            public bool IsReasigned { get; set; }
            public DateTime? ResignationDate { get; set; }
            public int NoticePeriodDays { get; set; }
            public bool IsExit { get; set; }
            public DateTime? ExitDate { get; set; }
            public bool EmployeeLeft { get; set; }
            public DateTime? LeavingDate { get; set; }
            public string LeavingReason { get; set; }
            public string LeavingReasonPF { get; set; }
            public string LeavingReasonESI { get; set; }
            public DateTime? EM_PayrollLastDate { get; set; }
            public string LocalExpat { get; set; }
            public double DocNo { get; set; }
            public DateTime? DocDate { get; set; }
            public string FID_Encrypted { get; set; }
            public double PreboardingFid { get; set; }
            public string HiddenEmployeeNo { get; set; }
            public string HiddenEmployeeSeries { get; set; }
            public bool? IsCriticalStageApplicable { get; set; }
            public double EmployeeCriticalStageID { get; set; }
            public double EmployeeAllocationCategoryId { get; set; }
            public string EM_Atten_DailyMonthly { get; set; }

        //=============================== END Mas_Employee====================================

        //=============================== Start Mas_Employee_ESS====================================

        public double ESSId { get; set; }
            public string ESSId_Encrypted { get; set; }
            //public bool Deactivate { get; set; }
            //public bool UseBy { get; set; }
            //public string CreatedBy { get; set; }
            //public DateTime CreatedDate { get; set; }
            //public string ModifiedBy { get; set; }
            //public string ModifiedDate { get; set; }
            //public string MachineName { get; set; }
            public double ESSEmployeeId { get; set; }
            public double ESSLoginID { get; set; }
            public string ESSPassword { get; set; }
            public string ESSSecurityQuestion { get; set; }
            public string ESSAnswer { get; set; }
            public bool ESSIsActive { get; set; }
            public bool ESSIsLock { get; set; }
            public int ESSLoginAttemptCount { get; set; }
            public DateTime ESSLastLoginTime { get; set; }
            public DateTime ESSLastPasswordChange { get; set; }
            //public bool IsExit { get; set; }
            public bool IsAdmin { get; set; }
            public double UserAccessPolicyId { get; set; }
            public string NewPassword { get; set; }
            public string OldPassword { get; set; }
            public string Answer { get; set; }
            public bool? IsMFA { get; set; }
            public string MFAToken { get; set; }
            public bool? MFAEnabled { get; set; }
            public bool IsApp { get; set; }
        //=============================== END Mas_Employee_ESS====================================
        //=============================== Start Mas_Employee_Personal====================================

        public double PersonalId { get; set; }
        public string PersonalId_Encrypted { get; set; }
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
        public DateTime? BirthdayDate { get; set; }
        public string AgeOfJoining { get; set; }
        public string BirthdayPlace { get; set; }
        public double BirthdayProofOfDocumentID { get; set; }
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
        //=============================== END Mas_Employee_Personal====================================
        //=============================== Start Mas_Employee_Address====================================


        public double AddressId { get; set; }
        public string AddressId_Encrypted { get; set; }
        public double AddressEmployeeId { get; set; }
        public string PresentPin { get; set; }
        public string PresentState { get; set; }
        public string PresentDistrict { get; set; }
        public string PresentTaluka { get; set; }
        public string PresentPO { get; set; }
        public string PresentCity { get; set; }
        public string PresentPostelAddress { get; set; }
        public string PermanentPin { get; set; }
        public string PermanentState { get; set; }
        public string PermanentDistrict { get; set; }
        public string PermanentTaluka { get; set; }
        public string PermanentPO { get; set; }
        public string PermanentCity { get; set; }
        public string PermanentPostelAddress { get; set; }
        public bool PermanentAddressSameAsCurrentAddress { get; set; }

        //=============================== END Mas_Employee_Address====================================
        //=============================== Start Mas_Employee_Statutory====================================



        public double StatutoryId { get; set; }
        public string StatutoryId_Encrypted { get; set; }
        public double StatutoryEmployeeId { get; set; }
        public bool ESIC_Applicable { get; set; }
        public double ESIC_CodeId { get; set; }
        public double ESIC_SettingID  { get; set; }
        public string ESIC_NO { get; set; }
        public string ESIC_ClosingDate { get; set; }
        public bool ESIC_IS_OldESICNo { get; set; }
        public string ESIC_PreviousESICNo { get; set; }
        public bool ESIC_IS_LinkWithESIC { get; set; }
        public double PT_SettingID { get; set; }
        public double LWF_SettingID { get; set; }
        public double PF_SettingID { get; set; }
        public bool PT_Applicable { get; set; }
        public double PT_CodeId { get; set; }
        public bool LWF_Applicable { get; set; }
        public double LWF_CodeId { get; set; }
        public string LWF_LIN { get; set; }
        public bool Gratuity_Applicable { get; set; }
        public string Gratuity_No { get; set; }
        public bool PF_Applicable { get; set; }
        public double PF_CodeId { get; set; }
        public bool? PF_Limit { get; set; }
        public bool? PF_EPS { get; set; }
        public string PF_VPF { get; set; }
        public string PF_FSType { get; set; }
        public string PF_FS_Name { get; set; }
        public string PF_UAN { get; set; }
        public string PF_NO { get; set; }
        public string PF_Nominee1 { get; set; }
        public string PF_Reletion1 { get; set; }
        public string PF_DOB1 { get; set; }
        public string PF_Share1 { get; set; }
        public string PF_Address1 { get; set; }
        public string PF_GuardianName1 { get; set; }
        public string PF_Nominee2 { get; set; }
        public string PF_Reletion2 { get; set; }
        public string PF_DOB2 { get; set; }
        public string PF_Share2 { get; set; }
        public string PF_Address2 { get; set; }
        public string PF_GuardianName2 { get; set; }
        public string PF_Nominee3 { get; set; }
        public string PF_Reletion3 { get; set; }
        public string PF_DOB3 { get; set; }
        public string PF_Share3 { get; set; }
        public string PF_Address3 { get; set; }
        public string PF_GuardianName3 { get; set; }
        public string PF_MobileNo { get; set; }
        public string PF_BankName { get; set; }
        public string PF_BankIFSC { get; set; }
        public string PF_Account { get; set; }
        public bool PF_1952 { get; set; }
        public bool PF_1995 { get; set; }
        public string PF_PreviousPFNo { get; set; }
        public string PF_ExitDate { get; set; }
        public string PF_CertificateNo { get; set; }
        public string PF_PPO { get; set; }
        public bool PF_OldUANNo { get; set; }
        public bool PF_LinkWithUAN { get; set; }
        public double PTSlab_MasterId { get; set; }
        public double LWFSlab_MasterId { get; set; }
        public double PFWages_MasterId { get; set; }
        public int BranchID { get; set; }
        public int BU_StateID { get; set; }
        public int BUComplienceId { get; set; }
        public string BUComplienceId_Encrypted { get; set; }
        //=============================== END Mas_Employee_Statutory====================================
        //=============================== Start Mas_Employee_Attendance====================================


        public double AttendanceId { get; set; }
        public string AttendanceId_Encrypted { get; set; }
        public double AttendanceEmployeeId { get; set; }
      //  public string EmployeeCardNo { get; set; }
        public bool? EM_Atten_OT_Applicable { get; set; }
        public float EM_Atten_OTMultiplyBy { get; set; }
        public float EM_Atten_PerDayShiftHrs { get; set; }
        public bool? EM_Atten_CoffApplicable { get; set; }
        public double EM_Atten_CoffSettingId { get; set; }
        public double EM_Atten_ShiftGroupId { get; set; }
        public double EM_Atten_ShiftRuleId { get; set; }
        public string EM_Atten_WOFF1 { get; set; }
        public string EM_Atten_WOFF2 { get; set; }
        public string EM_Atten_WOFF2_ForCheck { get; set; }
        public bool EM_Atten_WOFF_Check1 { get; set; }
        public bool EM_Atten_WOFF_Check2 { get; set; }
        public bool EM_Atten_WOFF_Check3 { get; set; }
        public bool EM_Atten_WOFF_Check4 { get; set; }
        public bool EM_Atten_WOFF_Check5 { get; set; }
        public double EM_Atten_LateMarkSettingApplicable { get; set; }
        public bool EM_Atten_LateMarkSettingId { get; set; }
        public bool  EM_Atten_IsSalaryFullPay { get; set; }
        public double EM_Atten_LeaveGroupId { get; set; }
        public bool? EM_Atten_DefaultAttenShow { get; set; }
        public bool EM_Atten_SinglePunch_Present { get; set; }
        public bool EM_Atten_RotationalWeekOff { get; set; }
        public bool? EM_Atten_Regularization_Required { get; set; }
        public bool? EM_Atten_ShortLeaveApplicable { get; set; }
        public double EM_Atten_ShortLeaveSettingId { get; set; }
        public bool? EM_Atten_PersonalGatepassApplicable { get; set; }
        public double EM_Atten_Atten_PersonalGatepassSettingId { get; set; }
        public DateTime EM_Atten_AttendanceLastDate { get; set; }
        public bool EM_Atten_PunchMissingApplicable { get; set; }
        public double EM_Atten_Atten_PunchMissingSettingId { get; set; }
        public bool EM_Atten_flexibleShiftApplicable { get; set; }
        public bool PHApplicable { get; set; }
        public double EM_Atten_Atten_OutDoorCompanySettingId { get; set; }
        public bool EM_Atten_OutDoorCompanyApplicable { get; set; }
        public bool EM_Atten_WOPH_CoffApplicable { get; set; }
        public List<dynamic> CheckedList { get; set; }
        public bool EM_Atten_ShortHRS_Applicable { get; set; }
        public double EM_Atten_Atten_OutDoorCompanyApplicable { get; set; }
        //public string EM_Atten_DailyMonthly { get; set; }
        public int EM_Atten_LocationRegistrationMappingIMasterId { get; set; }
        public bool EM_Atten_MonthlyRosterApplicable { get; set; }
        //=============================== END Mas_Employee_Attendance====================================
        //=============================== Start Mas_Employee_Bank====================================

        public double EmployeeBankId { get; set; }
        public string EmployeeBankId_Encrypted { get; set; }
        public double EmployeeBankEmployeeId { get; set; }
        public string SalaryIFSC { get; set; }
        public string SalaryBankName { get; set; }
        public string SalaryAccountNo { get; set; }
        public string SalaryBankAddress { get; set; }
        public string SalaryBankBranchName { get; set; }
        public string SalaryNameAsPerBank { get; set; }
        public string PermanentIFSC { get; set; }
        public string PermanentBankName { get; set; }
        public string PermanentAccountNo { get; set; }
        public string PermanentBankAddress { get; set; }
        public string PermanentBankBranchName { get; set; }
        public string PermanentNameAsPerBank { get; set; }
        public string SalaryConfirmAccountNo { get; set; }
        public string PermanentConfirmAccountNo { get; set; }
        public string SalaryMode { get; set; }
        //=============================== END Mas_Employee_Bank====================================

    }
}