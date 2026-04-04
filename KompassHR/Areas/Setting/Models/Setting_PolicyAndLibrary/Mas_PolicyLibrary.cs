using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary
{
    public class Mas_PolicyLibrary
    {
        public int PolicyLibraryId { get; set; }
        public string PolicyLibraryId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpID { get; set; }
        public double PolicyLibraryBranchId { get; set; }
        public double PolicyLibraryPolicyId { get; set; }
        public string Remark { get; set; }
        public string FileType { get; set; }
        public string DocumentPath { get; set; }
    }
    public class GetGroupList
    {
        public double PolicyLibraryId { get; set; }
        public string PolicyName { get; set; }
        public string remark { get; set; }
        public string DocumentPath { get; set; }
    }
}