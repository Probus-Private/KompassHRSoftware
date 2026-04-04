using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class SearchEmployee
    {
        public int SearchEmployeeId { get; set; }
        public string SearchEmployeeId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int EmployeeId { get; set; }
       
    }
}