using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_VMS
{
    public class Visitor_Tra_Master
    {
        public double VisitorId { get; set; }
        public string VisitorId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public int VisitorBranchId { get; set; }
        public double VisitingBranchID { get; set; }
        public string VisitorInBy { get; set; }
        public string VisitorOutBy { get; set; }       
        public string DocNo { get; set; }
        public DateTime InDateTime { get; set; }
        public double VisitorAppointmentID { get; set; }
        public DateTime GateOutTime { get; set; }
        public bool CheckOutFlag { get; set; }
        public DateTime HostOutTime { get; set; }
        public int HostEmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string HostCabin { get; set; }
        public string VisitorName { get; set; }
        public string MobileNo { get; set; }
        public string Additional { get; set; }
        public string Batchno { get; set; }
        public string VechileNo { get; set; }
        public int VisitorPurposeId { get; set; }
        public string PurposeName { get; set; }
        public int VisitorIdendtityId { get; set; }
        public string VIdendtityName { get; set; }
        public string DocumentNo { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string IndustryType { get; set; }
        public string PhoneNo { get; set; }
        public string Temperature { get; set; }
        public string PhotoPath { get; set; }
        public string VisitorAllowItem { get; set; }
        public string VisitorReturnItem { get; set; }
        public string VisitorDepositeItem { get; set; }
        public string VisitorLocker { get; set; }
        public string VisitorType { get; set; }
        public int VisitorTypeId { get; set; }
        public int HostEntryEmployeeId { get; set; }
        public string Remark { get; set; }

        public class VisitorMaster
        {
            public string VisitorPurposeId { get; set; }
            public string VisitorAppointmentBranchID { get; set; }
            public string HostEmployeeId { get; set; }
            public string VisitorName { get; set; }
            public string Designation { get; set; }
            public string EmailID { get; set; }
            public string CompanyName { get; set; }
        }

        public class VisitorMasterInOutList
        {
            public DateTime? GateOutTime { get; set; }
            public DateTime? InDateTime { get; set; }
           
        }

    }
}