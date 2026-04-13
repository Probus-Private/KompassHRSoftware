using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_ResourceRequest
    {
        public double ResourceId { get; set; }
        public string ResourceId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double ResourceBranchId { get; set; }
        public double ResourceEmployeeId { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double DesignationID { get; set; }
        public double DepartmentID { get; set; }
        public double GradeID { get; set; }
        public DateTime ClosingDate { get; set; }
        public int? TotalPositions { get; set; }
        public double PositionTypeID { get; set; }
        public double? EmployeeifReplacement { get; set; }
        public bool Confidential { get; set; }
        public string MinExperience { get; set; }
        public string MaxExperience { get; set; }
        public string Qualification { get; set; }
        public string PreferredLanguage { get; set; }
        public string PreferredTechnicalSkills { get; set; }
        public string MusthaveTechnicalSkills { get; set; }
        public string Competencies { get; set; }
        public bool PositionBudgeted { get; set; }
        public double Currency { get; set; }
        public string MinBudget { get; set; }
        public string MaxBudget { get; set; }
        public string MinAge { get; set; }
        public string MaxAge { get; set; }
        public bool LegalBond { get; set; }
        public string PreferredMaritalStatus { get; set; }
        public string PreferredGender { get; set; }
        public string Residence { get; set; }
        public double WorkTypeID { get; set; }
        public string PreferredShiftTiming { get; set; }
        public string Priority { get; set; }
        public double ReportingManager1 { get; set; }
        public double ReportingManager2 { get; set; }
        public double ReportingHR { get; set; }
        public string Status { get; set; }
        public double ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public string RequestStatus { get; set; }
        public string AdditionNotify { get; set; }
        public int? LegalBondDuration { get; set; }
        public string AttachFile { get; set; }
        public string Remark { get; set; }
        public string FilePath { get; set; }
        public int JDTemplateId { get; set; }
        public int AgencyAssignId { get; set; }

        public int? BranchId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CandidateType { get; set; }
    }

    public class AttritionReport
    {
        public int? Year { get; set; }
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
    }

    public class EmployeeDetails
    {
        public int? CmpID { get; set; }
        public int? EmployeeBranchId { get; set; }
        public int? EmployeeDepartmentID { get; set; }
        public string JobTemplate { get; set; }
    }

    public class Recruitment_ResourceRequest_Update
    {
        public DateTime ClosingDate { get; set; }
        public string BudgetApproved { get; set; }
        public int? MaxBudget { get; set; }
        public string LegalBondApplicable { get; set; }
        public int? LegalBondDuration { get; set; }
        public int ResponsibleRecruitmentEmployeeId { get; set; }
    }
}