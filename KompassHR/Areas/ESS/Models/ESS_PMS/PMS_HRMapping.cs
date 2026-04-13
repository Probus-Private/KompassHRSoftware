using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_HRMapping
    {
       
            public double PmshrmappingId { get; set; }
            public string PmshrmappingId_Encrypted { get; set; }
            public bool Deactivate { get; set; }
            public bool UseBy { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string MachineName { get; set; }
            public long CmpID { get; set; }
            public long BranchId { get; set; }
            public long DepartmentId { get; set; }
            public long DesignationId { get; set; }
            public long EmployeeId { get; set; }
            public long ResponsibleHR { get; set; }
            public bool IsActive { get; set; }
            public string SelectedEmployeeIds { get; set; }

    }
}