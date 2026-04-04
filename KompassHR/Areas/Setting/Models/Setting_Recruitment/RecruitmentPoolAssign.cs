using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class RecruitmentPoolAssign
    {
        public double RecruitmentPoolAssignId { get; set; }
        public string RecruitmentPoolAssignId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double RecruitmentAssignEmployeeId { get; set; }
      //  public bool IsActive { get; set; }
    }
}