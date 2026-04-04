using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Payroll_Advance
    {
        public double AdvanceID { get; set; }
        public string AdvanceID_Encrypted { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double AdvanceEmployeeID { get; set; }
        public string AdvanceAmount { get; set; }
        public DateTime FromMonthYear { get; set; }
        public DateTime ToMonthYear { get; set; }
        public string InstallmentAmount { get; set; }
        public int NoOfInstallment { get; set; }
        public string Remark { get; set; }
        public double BalanceAmount { get; set; }
        public double SettlementAmount { get; set; }
        public string AdvanceSettlementRemark { get; set; }
        public bool IsAdvanceSettlement { get; set; } = true;
    }
}