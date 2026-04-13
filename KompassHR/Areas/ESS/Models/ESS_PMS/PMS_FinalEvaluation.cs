using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_FinalEvaluation
    {
        public decimal FinalEvaluationId { get; set; }
        public string FinalEvaluationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double YearId { get; set; }
        public double EmployeeId { get; set; }
        public double ReviewerId { get; set; }
        public decimal OutOfRating { get; set; }
        public decimal KRAFinalRating { get; set; }
        public decimal CompetencyFinalRating { get; set; }
        public decimal TotalFinalRating { get; set; }
    }
}