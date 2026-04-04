using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_BudgetMapping
    {

        public double BudgetMappingId { get; set; }
        public string BudgetMappingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int BudgetMappingCmpId { get; set; }
        public int BudgetMappingBuId { get; set; }
        public int BudgetMappingBudgetId { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive number.")]
        public int Amount { get; set; }
    }
}