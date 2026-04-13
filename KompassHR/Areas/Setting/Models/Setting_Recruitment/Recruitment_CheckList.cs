using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_CheckList
    {
       
        public int CheckListID { get; set; }
        public string CheckListID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        //public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ModuleName { get; set; }
        public string Origin { get; set; }
        public string CheckListName { get; set; }
        public string ItemName { get; set; }
        public string Remarks { get; set; }
        public double CmpId { get; set; }
        public double GradeID { get; set; }
        public double DepartmentID { get; set; }
        public double DesignationID { get; set; }
        public double CheckListBranchId { get; set; }
        public int Rate { get; set; }
    }
}