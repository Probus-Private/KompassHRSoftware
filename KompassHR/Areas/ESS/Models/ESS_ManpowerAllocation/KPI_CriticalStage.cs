using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class KPI_CriticalStage
    {
        public double KPICriticalStageId { get; set; }
        public string KPICriticalStageId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public double CmpID { get; set; }
        public double KPICriticalStageBranchId { get; set; }
        public DateTime CriticalStageFromDate { get; set; }
        public DateTime CriticalStageToDate { set; get; }
        public double CriticalStageGradeId { get; set; }
        public double CriticalStageDepartmentId { get; set; }
        public double CriticalStageDesignationId { get; set; }
        public double CriticalStageCategoryId { get; set; }
        public double CriticalStageEmployeeId { get; set; }
        public double CriticalStageShiftGroupId { get; set; }
        public double CriticalStageShiftRuleId { get; set; }
    }
    public class KPIBulkRecord
    {
        public int CriticalStageEmployeeId { get; set; }
        public int CriticalStageShiftRuleId { get; set; }
        public int CriticalStageShiftGroupId { get; set; }
    }
}