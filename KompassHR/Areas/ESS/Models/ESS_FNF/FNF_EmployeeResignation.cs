using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FNF
{
    public class FNF_EmployeeResignation
    {
        public double FnfId { get; set; }
        public string FnfId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double FnfBranchID { get; set; }
        public double FnfEmployeeId { get; set; }
        public int DocNo { get; set; }
        public DateTime DOJ { get; set; }
        public DateTime ConfirmationDate { get; set; }
        public string IsConfirmation { get; set; }
        public string NoticePeriodDays { get; set; }
        public string NoticePeriodType { get; set; }

        public int NoticePeriodMonthDays { get; set; }

        public double DayMonth { get; set; }
        public string WorkDuration { get; set; }
        public double FnfDesignationId { get; set; }
        public double FnfDepartmentId { get; set; }
        public DateTime ResignationDate { get; set; }
        public DateTime LastWorkingDate { get; set; }
        public DateTime NoticePeriodEndDate { get; set; }
        public DateTime RelievingDate { get; set; }

        public double FnfReasonId { get; set; }
        public double FnfGradeId { get; set; }
        public string Remark { get; set; }
   
        public bool IsImmediatelyRelieve { get; set; }
        public bool IsNoticePeriodPurchase { get; set; }
     
        public string Status { get; set; }
        public bool ApprovedAutoManually { get; set; }
     
        public double ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
     
        public string FullAddress { get; set; } 
        public string PersonalEmailId { get; set; }
        public string PrimaryMobile { get; set; }
        public bool AcceptTerms { get; set; }
        public string AttachFileName { get; set; }
        public bool IsCalculated { get; set; }

        public string FnfApprovalAnsId { get; set; }
        public bool IsReasonCorrect { get; set; }
        public string IsReasonCorrectRemark { get; set; }
        public bool IsCriticalResource { get; set; }
        public string IsCriticalResourceRemark { get; set; }
        public bool IsHireagain { get; set; }
        public string IsHireagainRemark { get; set; }
        public bool IsImmediateRelease { get; set; }
        public string IsImmediateReleaseRemark { get; set; }
        public bool IsVoluntary { get; set; }
        public string VoluntaryRemark { get; set; }
        public bool TaskPending { get; set; }
        public string TaskPendingRemark { get; set; }
        public bool KPIPending { get; set; }
        public string KPIPendingRemark { get; set; }
        public string EmployerNextCompany { get; set; }
        public bool ServiceBond { get; set; }
        public string ServiceBondRemark { get; set; }
        public string InvestigationRequired { get; set; }
        public string InvestigationRequiredRemark { get; set; }
        public string ManagerRemark { get; set; }

        public bool IsManually { get; set; } = true;
      //  public string ResignationType { get; set; }
        public double ResignationTypeId { get; set; }
    }
}