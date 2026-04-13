using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Module_Employee_BulkInsert
    {
        public string SrNo { get; set; }
        public string EmployeeId_Encrypted { get; set; }
        public string CmpID { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeBranchId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string Salutation { get; set; }
        public string EmployeeName { get; set; }
        public string ContractorID { get; set; }
        public string EmployeeDepartmentID { get; set; }
        public string EmployeeSubDepartmentName { get; set; }
        public string EmployeeDesignationID { get; set; }
        public string EmployeeGradeID { get; set; }
        public string EmployeeUnitID { get; set; }
        public string EmployeeLevelID { get; set; }
        public string IsCriticalStageApplicable { get; set; }
        public string EmployeeCriticalStageID { get; set; }
        public string EmployeeLineID { get; set; }
        public string JoiningDate { get; set; }

        public string PersonalId_Encrypted { get; set; }
        public string PersonalEmployeeId { get; set; }
        public string AadhaarNo { get; set; }
        public string NameAsPerAadhaar { get; set; }
        public string PrimaryMobile { get; set; }
        public string BirthdayDate { get; set; }
        public string EmployeeQualificationID { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }

        public string AddressId_Encrypted { get; set; }
        public string AddressEmployeeId { get; set; }
        public string PresentPin { get; set; }
        public string PresentState { get; set; }
        public string PresentDistrict { get; set; }
        public string PresentTaluka { get; set; }
        public string PresentPO { get; set; }
        public string PresentCity { get; set; }
        public string PresentPostelAddress { get; set; }

        public string AttendanceId_Encrypted { get; set; }
        public string AttendanceEmployeeId { get; set; }
        public string EM_Atten_WOFF1 { get; set; }
        public string EM_Atten_ShiftGroupId { get; set; }
        public string EM_Atten_ShiftRuleId { get; set; }
        public string EM_Atten_OT_Applicable { get; set; }
        public string EM_Atten_OTMultiplyBy { get; set; }
        public string EM_Atten_PerDayShiftHrs { get; set; }
        public string EmployeeAllocationCategoryId { get; set; }

        public double StatutoryId { get; set; }
        public string StatutoryId_Encrypted { get; set; }
        public double StatutoryEmployeeId { get; set; }
        public string PF_FSType { get; set; }
        public string PF_FS_Name { get; set; }
        public string PF_UAN { get; set; }
        public string PF_NO { get; set; }
        public string ESIC_NO { get; set; }
        public string PF_Nominee1 { get; set; }
        public string PF_Reletion1 { get; set; }

        public string DuplicateAddharNo { get; set; }
    }

    public class _BulkInsertDuplicateAddharNo
    {
        public string AadhaarNo { get; set; }
    }

    public class DuplicateAddharNoDataTable
    {
        public string Company { get; set; }
        public string BU { get; set; }
        public string Employee { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string ContractorName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string GradeName { get; set; }
        public string JoiningDate { get; set; }
        public string AadhaarNo { get; set; }

        public string Status { get; set; }
        public string LeavingDate { get; set; }
        public string Duration { get; set; }
    }


    public class _Employee_BulkUpdate
    {


        public string SrNo { get; set; }
        // public string EmployeeId_Encrypted { get; set; }
        public string CmpID { get; set; }
        // public string EmployeeId { get; set; }
        public string EmployeeBranchId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string Salutation { get; set; }
        public string EmployeeName { get; set; }
        public string ContractorID { get; set; }
        public string EmployeeDepartmentID { get; set; }
        public string EmployeeSubDepartmentName { get; set; }
        public string EmployeeDesignationID { get; set; }
        public string EmployeeGradeID { get; set; }
        public string EmployeeUnitID { get; set; }
        public string EmployeeLevelID { get; set; }
        public string IsCriticalStageApplicable { get; set; }
        public string EmployeeCriticalStageID { get; set; }
        public string EmployeeLineID { get; set; }
        //public string JoiningDate { get; set; }

        // public string PersonalId_Encrypted { get; set; }
        // public string PersonalEmployeeId { get; set; }
        // public string AadhaarNo { get; set; }
        public string PrimaryMobile { get; set; }
        public string BirthdayDate { get; set; }
        public string EmployeeQualificationID { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }

        // public string AddressId_Encrypted { get; set; }
        // public string AddressEmployeeId { get; set; }
        public string PresentPin { get; set; }
        public string PresentState { get; set; }
        public string PresentDistrict { get; set; }
        public string PresentTaluka { get; set; }
        public string PresentPO { get; set; }
        public string PresentCity { get; set; }
        public string PresentPostelAddress { get; set; }

        // public string AttendanceId_Encrypted { get; set; }
        // public string AttendanceEmployeeId { get; set; }
        public string EM_Atten_WOFF1 { get; set; }
        public string EM_Atten_ShiftGroupId { get; set; }
        public string EM_Atten_ShiftRuleId { get; set; }
        public string EM_Atten_OT_Applicable { get; set; }
        public string EM_Atten_OTMultiplyBy { get; set; }
        public string EM_Atten_PerDayShiftHrs { get; set; }

        //public string DuplicateAddharNo { get; set; }
    }

    public class _Employee_BulkEmployeeLeft
    {

        public string SrNo { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string EmployeeName { get; set; }
        public string GradeName { get; set; }
        public string ContractorName { get; set; }
        public string JoiningDate { get; set; }
        public string LeavingDate { get; set; }

        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double ContractorId { get; set; }
        public double GradeId { get; set; }
    }

    public class SingleEmployeeLeft
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double EmployeeId { get; set; }
        public DateTime LeftDate { get; set; }
    }

    //public class Module_Employee_BulkInsert_Premium
    //{
    //    public string SrNo { get; set; }
    //    public string EmployeeId_Encrypted { get; set; }
    //    public string CmpID { get; set; }
    //    public string EmployeeId { get; set; }
    //    public string EmployeeBranchId { get; set; }
    //    public string EmployeeNo { get; set; }
    //    public string EmployeeCardNo { get; set; }
    //    public string Salutation { get; set; }
    //    public string EmployeeName { get; set; }
    //    public string ContractorID { get; set; }
    //    public string EmployeeDepartmentID { get; set; }
    //    public string EmployeeSubDepartmentName { get; set; }
    //    public string EmployeeDesignationID { get; set; }
    //    public string EmployeeGradeID { get; set; }
    //    public string EmployeeUnitID { get; set; }
    //    public string EmployeeLevelID { get; set; }
    //    public string IsCriticalStageApplicable { get; set; }
    //    public string EmployeeCriticalStageID { get; set; }
    //    public string EmployeeLineID { get; set; }
    //    public string JoiningDate { get; set; }

    //    public string PersonalId_Encrypted { get; set; }
    //    public string PersonalEmployeeId { get; set; }
    //    public string AadhaarNo { get; set; }
    //    public string NameAsPerAadhaar { get; set; }
    //    public string PrimaryMobile { get; set; }
    //    public string BirthdayDate { get; set; }
    //    public string EmployeeQualificationID { get; set; }
    //    public string Gender { get; set; }
    //    public string MaritalStatus { get; set; }

    //    public string AddressId_Encrypted { get; set; }
    //    public string AddressEmployeeId { get; set; }
    //    public string PresentPin { get; set; }
    //    public string PresentState { get; set; }
    //    public string PresentDistrict { get; set; }
    //    public string PresentTaluka { get; set; }
    //    public string PresentPO { get; set; }
    //    public string PresentCity { get; set; }
    //    public string PresentPostelAddress { get; set; }

    //    public string AttendanceId_Encrypted { get; set; }
    //    public string AttendanceEmployeeId { get; set; }
    //    public string EM_Atten_WOFF1 { get; set; }
    //    public string EM_Atten_ShiftGroupId { get; set; }
    //    public string EM_Atten_ShiftRuleId { get; set; }
    //    public string EM_Atten_OT_Applicable { get; set; }
    //    public string EM_Atten_OTMultiplyBy { get; set; }
    //    public string EM_Atten_PerDayShiftHrs { get; set; }
    //    public string EmployeeAllocationCategoryId { get; set; }

    //    public double StatutoryId { get; set; }
    //    public string StatutoryId_Encrypted { get; set; }
    //    public double StatutoryEmployeeId { get; set; }
    //    public string PF_FSType { get; set; }
    //    public string PF_FS_Name { get; set; }
    //    public string PF_UAN { get; set; }
    //    public string PF_NO { get; set; }
    //    public string ESIC_NO { get; set; }
    //    public string PF_Nominee1 { get; set; }
    //    public string PF_Reletion1 { get; set; }

    //    public double CTCEmployeeId  { get; set; }
    //    public string FromDate { get; set; }
    //    public string ToDate { get; set; } 
    //    public bool IsIncrement { get; set; }
    //    public string DailyMonthly { get; set; }
    //    public string Rate { get; set; }
    //    public double RateOTRate { get; set; }
    //    public double CategoryId  { get; set; }
    //    public double RatePartA_A { get; set; }
    //    public double RatePartA_Basic { get; set; }
    //    public string AttendanceBonus { get; set; }
    //    public string AccountNo { get; set; }
    //    public string AccountName { get; set; }
    //    public string IFSC_Code { get; set; }
    //    public string CanteenApplicable { get; set; }
    //    public string DuplicateAddharNo { get; set; }
    //}

    public class Module_Employee_BulkInsert_Premium
    {
        public int SrNo { get; set; }
        public string CompanyName { get; set; }                 //Mas_Employee --Column Is--> //CmpID
        public string BranchName { get; set; }                  //Mas_Employee --Column Is--> //EmployeeBranchId
        public string ContractorName { get; set; }              //Mas_Employee --Column Is--> //ContractorID
        public string Salutation { get; set; }                  //Mas_Employee --Column Is--> //Salutation
        public string EmployeeName { get; set; }                //Mas_Employee --Column Is--> //EmployeeName
        public string EmployeeNo { get; set; }                  //Mas_Employee --Column Is--> //EmployeeNo
        public string EmployeeCardNo { get; set; }              //Mas_Employee --Column Is--> //EmployeeCardNo
        public string Designation { get; set; }                 //Mas_Employee --Column Is--> //EmployeeDesignationID
        //public string SubUnit { get; set; }                     //Mas_Employee --Column Is--> //EmployeeUnitID

        public string AadhaarNo { get; set; }                   //Mas_Employee_Personal --Column Is--> //AadhaarNo
        public string NameAsPerAadhaar { get; set; }            //Mas_Employee_Personal --Column Is--> // NameAsPerAadhaar
        public string PrimaryMobile { get; set; }               //Mas_Employee_Personal --Column Is--> // PrimaryMobile
        public string Gender { get; set; }                      //Mas_Employee_Personal --Column Is--> //Gender
        public string BirthDate { get; set; }                   //Mas_Employee_Personal --Column Is--> //BirthdayDate
        public string MaritalStatus { get; set; }               //Mas_Employee_Personal --Column Is--> //MaritalStatus
        public string HighestQualification { get; set; }        //Mas_Employee_Personal --Column Is--> //QualificationRemark/EmployeeQualificationID

        public string JoiningDate { get; set; }                 //Mas_Employee          --Column Is--> //Mas_Employee_Personal- AgeOfJoining (Mas_Employee-JoiningDate)

        public string ShiftGroup { get; set; }                  //Mas_Employee_Attendance --Column Is--> //EM_Atten_ShiftGroupId
        public string ShiftRule { get; set; }                   //Mas_Employee_Attendance --Column Is--> //EM_Atten_ShiftRuleId
        public int CanteenApplicable { get; set; }   // 0 or 1  //Mas_Employee_Attendance --Column Is--> //EM_Atten_CanteenApplicable

        public string Pincode { get; set; }                     //Mas_Employee_Address --Column Is--> //PresentPin/PermanentPin
        public string StateName { get; set; }                   //Mas_Employee_Address --Column Is--> //PresentState/PermanentState
        public string DistrictName { get; set; }                //Mas_Employee_Address --Column Is--> //PresentDistrict/PermanentDistrict
        public string TalukaName { get; set; }                  //Mas_Employee_Address --Column Is--> // PermanentTaluka/PresentTaluka
        public string POName { get; set; }                      //Mas_Employee_Address --Column Is--> //PresentPO/PermanentPO
        public string CityName { get; set; }                    //Mas_Employee_Address --Column Is--> //PresentCity/PermanentCity
        public string PostalAddress { get; set; }               //Mas_Employee_Address --Column Is--> //PermanentPostelAddress/PresentPostelAddress

        public string PayType { get; set; }                     //Mas_Employee_CTC --Column Is--> //DailyMonthly
        public string Rate { get; set; }                        //Mas_Employee_CTC --Column Is--> //RateOTRate
        public string AttendanceBonus { get; set; }             //Mas_Employee_CTC -Column Name Is->RatePartB_AttendanceBonus_IsApplicable

        public string AccountNo { get; set; }                   //Mas_Employee_Bank --Column Is--> //SalaryAccountNo,PermanentAccountNo
        public string BankName { get; set; }                    //Mas_Employee_Bank --Column Is--> // PermanentBankName
        public string IFSCCode { get; set; }                    //Mas_Employee_Bank --Column Is--> //SalaryIFSC




        //public string Department { get; set; }                  //Mas_Employee --Column Is--> //EmployeeDepartmentID
        //public string SubDepartment { get; set; }               //Mas_Employee --Column Is--> //EmployeeSubDepartmentName
        //public string ManpowerCategory { get; set; }                         //--Column Is--> //EmployeeAllocationCategoryId
        //public string Grade { get; set; }                       //Mas_Employee --Column Is--> //EmployeeGradeID
        //public string CriticalStageApplicable { get; set; }     //Mas_Employee --Column Is--> //IsCriticalStageApplicable
        //public string CriticalStageCategory { get; set; }       //Mas_Employee --Column Is--> //EmployeeCriticalStageID
        //public string LineMaster { get; set; }                  //Mas_Employee --Column Is--> //EmployeeLineID
        //public string AssessmentLevel { get; set; }
        //public string WeeklyOff { get; set; }                   //Mas_Employee_Attendance --Column Is--> //EM_Atten_WOFF1
        //public string PFNo { get; set; }                        //Mas_Employee_Statutory --Column Is--> //PF_NO
        //public string RelationType { get; set; }                //Mas_Employee_Statutory --Column Is--> //PF_Reletion1
        //public string RelationName { get; set; }                //Mas_Employee_Statutory --Column Is--> //PF_GuardianName1
        //public string UANNo { get; set; }                       //Mas_Employee_Statutory --Column Is--> //PF_UAN
        //public string ESICNo { get; set; }                      //Mas_Employee_Statutory --Column Is--> //ESIC_NO
        //public string NomineeRelation { get; set; }             //Mas_Employee_Statutory --Column Is--> //PF_Nominee1/PF_Nominee2/PF_Nominee3
        //public string NomineeName { get; set; }                 //Mas_Employee_Statutory --Column Is--> //
        //public decimal RateBasic { get; set; }                                                  // RatePartA_Basic
       
    }


    public class SaveBulkInsertRequest
    {
        public List<Module_Employee_BulkInsert_Premium> tbldata { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
    }

}