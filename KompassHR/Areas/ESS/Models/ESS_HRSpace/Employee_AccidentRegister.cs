using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_HRSpace
{
    public class Employee_AccidentRegister
    {
   
        public int AccidentRegisterId { get; set; }
        public string AccidentRegisterId_Encrypted { get; set; }
        public Boolean Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int AccidentNo { get; set; }
        public DateTime DateOfEntry { get; set; }
        public DateTime AccidentTime { get; set; }
        public string Company { get; set; }
        public string BusinessUnit { get; set; }
        public string SubUnit { get; set; }
        public string Shift { get; set; }
        public string Description { get; set; }
        public string  Cause { get; set; }
        public string Nature { get; set; }
        public string ImmediateCorrectionAction { get; set; }
        public string  HospitalName { get; set; }
        public string  WasAnybodyInjured { get; set; }
        public string WasAnybodyInvolved { get; set; }
        public string Witness { get; set; }
        public string AccidentBookedBy { get; set; }
    }
}