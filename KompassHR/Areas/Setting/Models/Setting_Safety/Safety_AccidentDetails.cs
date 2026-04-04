using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Safety
{
    public class Safety_AccidentDetails
    {
        public string AccidentDetailId { get; set; }
        public string AccidentDetailId_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double AccidentDetailBranchId { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string AccidentTime { get; set; }
        public double AccidentDetailSubUnitId { get; set; }
        public double AccidentDetailShiftId { get; set; }
        public string AccidentDescription { get; set; }
        public string AccidentCause { get; set; }
        public string AccidentNature { get; set; }
        public string ImmediateCorrectionAction { get; set; }
        public string HospitalName { get; set; }
        public string WasAnybodyInjured { get; set; }
        public string WasAnybodyInvolved { get; set; }
        public string WhereThereIsAnyWitness { get; set; }
        public string AccidentBookBy { get; set; }

    }
}