using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TMS_TeamAssign
    {
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TeamAssignId { get; set; }
        public string TeamAssignId_Encrypted { get; set; }
        public double TeamManagerId { get; set; }
        public double TeamEmployeeId { get; set; }
        public bool IsActive { get; set; }
        public double DepartmentId { get; set; }
        public double ProjectID { get; set; }
        public double ClientID { get; set; }
    }

    public class OBJ_TMS_TeamAssign
    {
        public double SN { get; set; }
        public string EmployeeName { get; set; }
    }
}