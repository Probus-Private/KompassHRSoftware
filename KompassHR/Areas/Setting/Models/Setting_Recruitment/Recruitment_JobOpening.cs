using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_JobOpening
    {
        public double JobOpeningId { get; set; }
        public string JobOpeningId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double JobOpeningBranchId { get; set; }
        public double JobOpeningDepartmentId { get; set; }
        public double JobOpeningDesignationId { get; set; }
        public int DocNo { get; set; }
        public string JobPostingEmployeeId { get; set; }
        public DateTime JobPostingDate { get; set; }
        public DateTime JobPostingClosingDate { get; set; }
        public string JobTittle { get; set; }
        public string NoOfPostions { get; set; }
        public string JobLocation { get; set; }
        public string Experience { get; set; }
        public string AnnualSalary { get; set; }
        public double CurrencyID { get; set; }
        public string JobType { get; set; }
        public string PreferredCandidate { get; set; }
        public string Skill { get; set; }
        public string JobDescription { get; set; }
        public string RequiredWithinDays { get; set; }
        public bool NightShiftAllowed { get; set; }
        public bool WFMAllowed { get; set; }
        public bool HybridAllowed { get; set; }
        public string NoOfDayWorkingInWeek { get; set; }
        public string AgeBetween { get; set; }
        public double JobOpeningManpowerRequisitionId { get; set; }
        public bool IsWalkIn { get; set; }
        public string WalkInFromDate { get; set; }
        public string WalkInToDate { get; set; }
        public string WalkInTime { get; set; }
        public string WalkInVenue { get; set; }
        public string ContactPerson { get; set; }
        public string MobileNo { get; set; }



    }
}