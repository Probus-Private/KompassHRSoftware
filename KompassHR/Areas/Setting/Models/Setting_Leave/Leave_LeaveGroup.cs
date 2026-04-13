using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Leave_Group
    {
        [Key]
        public double LeaveGroupID { get; set; }
        public string LeaveGroupID_Encrypted { get; set; }      
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string LeaveGroupName { get; set; }
        public double CmpId { get; set; }

    }
}