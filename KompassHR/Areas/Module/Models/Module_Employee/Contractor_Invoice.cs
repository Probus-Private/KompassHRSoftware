using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{

    public class Contractor_Invoice
        {

        public Contractor_Invoice()
        {
            ContractorInvoice = new List<EmployeeInvoiceData>();
        }


        public int? CmpId { get; set; }
            public int? BranchId { get; set; }
            public int? ContractorId { get; set; }
            public DateTime InvoiceMonth { get; set; }
            public int ContractorInvoiceId { get; set; }
            public string ContractorInvoiceId_Encrypted { get; set; }
        

        //public double ContractorMonthlyContractorId { get; set; }
        //public string ContractorMonthlyContractorName { get; set; }
        //public DateTime ContractorMonthlyMonthYear { get; set; }
        //public double ContractorNoOfEmployee { get; set; }
        //public double Total_PerDayAmount { get; set; }
        //public double Total_CanteenAmount { get; set; }
        //public double Total_AttendanceBonusAmount { get; set; }
        //public double Total_TransportAllowAmount { get; set; }
        //public double Total_OtherRate1Amount { get; set; }
        //public double Total_OtherRate2Amount { get; set; }
        //public double TotalAmount { get; set; }
        //public double ServiceCharges { get; set; }
        //public double Rate { get; set; }
        //public double Percentage { get; set; }
        //public string IsGross { get; set; }
        //public string IsOT { get; set; }
        //public double SupervisorCharges { get; set; }
        //public double Total_ServiceCharges { get; set; }
        //public double Total_SupervisorCharges { get; set; }
        //public double Total_InvoiceAmout { get; set; }


        public List<EmployeeInvoiceData> ContractorInvoice { get; set; }
    }



    public class EmployeeInvoiceData
    {
        public double ContractorInvoiceId { get; set; }
        public string ContractorInvoiceId_Encrypted { get; set; }
        public string ContractorMonthlyContractorId { get; set; }
       /// public string ContractorMonthlyContractorName { get; set; }
        public string Contractor { get; set; }
        public DateTime ContractorMonthlyMonthYear { get; set; }
        public double NoOfEmployee { get; set; }
        public double NoOfDays { get; set; }
        // public double ContractorNoOfEmployee { get; set; }
        public Decimal Total_PerDayAmount { get; set; }
        public Decimal Total_CanteenAmount { get; set; }
        public double Total_AttendanceBonusAmount { get; set; }
        public Decimal Total_TransportAllowAmount { get; set; }
        public Decimal Total_OtherRate1Amount { get; set; }
        public Decimal Total_OtherRate2Amount { get; set; }
        public Decimal TotalAmount { get; set; }
        public string ServiceCharges { get; set; }
        public Decimal Rate { get; set; }
        public Decimal Percentage { get; set; }
        public bool IsGross { get; set; }
        public bool IsOT { get; set; }
        public Decimal SupervisorCharges { get; set; }
        public double Total_ServiceCharges { get; set; }

        public Decimal Total_SupervisorCharges { get; set; }
        public Decimal Total_InvoiceAmout { get; set; }


    }
}

