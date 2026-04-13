using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Conference
{
    public class Curve_DocumentSubmission
    {
        public double DocAssignmentDetailsId { get; set; }
        public string DocAssignmentDetailsId_Encrypted { get; set; }
        public double EmployeeId { get; set; }
        public double DocAssignmentMasterId { get; set; }
        public string DocumentPath { get; set; }
        public string Remark { get; set; }
        public string Description { get; set; }
        public string Instruction { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string Frequency { get; set; }
    }
}