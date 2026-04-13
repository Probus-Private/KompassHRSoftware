using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class AttenRoster
    {
        public double SrNo { get; set; }
        public double RosterID { get; set; }
        public string RosterID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double RosterCmpID { get; set; }
        public string CompanyName { get; set; }
        public double RosterBranchID { get; set; }
        public string BranchName { get; set; }
        public DateTime RosterMonthYear { get; set; }
        public double RosterEmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string JoiningDate { get; set; }

        public double RosterManagerID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Is_WO { get; set; }
        public double ShiftId { get; set; }
        public string ShiftName { get; set; }
        public double EmployeeId { get; set; }
    }
   
}