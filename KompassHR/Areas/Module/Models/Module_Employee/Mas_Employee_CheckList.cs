using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_CheckList
    {
        public double EmployeeCheckListID { get; set; }
        public string EmployeeCheckList_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CheckListEmployeeID { get; set; }
        public double EmpCheckListID { get; set; }
        public string EmpCheckListRemark { get; set; }
        public string CheckListName { get; set; }
    }
}