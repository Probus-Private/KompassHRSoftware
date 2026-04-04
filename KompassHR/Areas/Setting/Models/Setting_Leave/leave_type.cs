using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class leave_type
    {

        [Key]
        public double LeaveTypeId { get; set; }
        public string LeaveTypeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string LeaveTypeName { get; set; }
        public string LeaveTypeShortName { get; set; }
        public bool IsActive { get; set; }
        public bool ReadOnly { get; set; }

    }
}