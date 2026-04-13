using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Contractor_AttendanceSummary
    {
            public int? CmpId { get; set; }
            public int? BranchId { get; set; }
            public int? ContractorId { get; set; }
            public DateTime InvoiceMonth { get; set; }
            public int ContractorInvoiceId { get; set; }
            public string ContractorInvoiceId_Encrypted { get; set; }
        }

        public class SaveTypeContractorAttendance_summaryViewModel
        {
            public List<TypeContractorAttendance_summary> ObjContractorAttendanceSummary { get; set; }

        }

        public class TypeContractorAttendance_summary
        {
           // public string Hide { get; set; }
            public string ContractorMonthlyCmpId { get; set; }
            public string ContractorMonthlyBranchId { get; set; }
            public string ContractorMonthlyContractorId { get; set; }
            public string ContractorMonthlyEmployeeId { get; set; }
            public string ContractorMonthlyDepartmentId { get; set; }
            public string ContractorMonthlyDesignationId { get; set; }
            public string DailyMonthly { get; set; }
            public string ShortHrApplicable { get; set; }
            public string ShortHrApplicableMin { get; set; }

            public string Contractor { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeNo { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public string ContractorName { get; set; }
            public string SubUnit { get; set; }
            public string ManpowerCategory { get; set; }
          
            public string AB { get; set; }
            public string PP { get; set; }
            public string WO { get; set; }
            public string PH { get; set; }
            public string CO { get; set; }
            public string CL { get; set; }
            public string SL { get; set; }
            public string EL { get; set; }
            public string Other1 { get; set; }
            public string Other2 { get; set; }
            public string Other3 { get; set; }
            public string LOPDays { get; set; }
            public string PayableDays { get; set; }
            public string OThrs { get; set; }
            public string LOPHrs { get; set; }
            public string InvoiceProcess { get; set; }
            public string LateCount { get; set; }
            public string LateHrs { get; set; }
            public string EarlyCount { get; set; }
            public string EarlyHrs { get; set; }
            public string InOut_ShortHrs { get; set; }
            public string Rate_PerDay { get; set; }
            public string Rate_Canteen { get; set; }
            public string Rate_AttendanceBonus { get; set; }
            public string Rate_TransportAllow { get; set; }
            public string Rate_Other1 { get; set; }
            public string Rate_Other2 { get; set; }
            public string TotalRate_PerDay { get; set; }
            public string TotalRate_Canteen { get; set; }
            public string TotalRate_AttendanceBonus { get; set; }
            public string TotalRate_TransportAllow { get; set; }
            public string TotalRate_Other1 { get; set; }
            public string TotalRate_Other2 { get; set; }
            public string TotalOTAmount { get; set; }
            public string TotalAmount { get; set; }
    }

    public class DeleteContractorAttendanceViewModel
    {
        public List<Type_DeleteContractorAttendance> ObjContractorAttendance { get; set; }

    }
    public class Type_DeleteContractorAttendance
    {
        public string ContractorMonthlyId { get; set; }
        public string ContractorMonthlyEmployeeId { get; set; }
        public DateTime ContractorMonthlyMonthYear { get; set; }
    }
}