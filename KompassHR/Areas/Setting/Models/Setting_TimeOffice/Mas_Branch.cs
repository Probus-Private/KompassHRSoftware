using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Mas_Branch
    {
        public double BranchId { get; set; }
        public string BranchId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public string BranchName { get; set; }
        public bool IsActive { get; set; }
        public string BranchAddress { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public double PlantHeadID { get; set; }
        public double HRPersonID { get; set; }
        public string BranchSignature { get; set; }
        public string Logo { get; set; }
        public string Stamp { get; set; }
        public string GMap_Coordinates { get; set; }
    }
}