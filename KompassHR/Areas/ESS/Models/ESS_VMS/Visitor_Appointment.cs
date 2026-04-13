using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_VMS
{
    public class Visitor_Appointment
    {

        public double VisitorAppointmentID { get; set; }
        public string VisitorAppointmentID_Encrypted { get; set; }
        public string MachineName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public double CmpId { get; set; }
        public double VisitorAppointmentBranchID { get; set; }
        public double VisitingBranchID { get; set; }
        public double VisitorAppointmentEmployeeID { get; set; }
        public int DocNo { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string VisitorName { get; set; }
        public string AdditionPerson { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public string EmailID { get; set; }
        public string Company { get; set; }
        public bool IsSpecial { get; set; }
        public double VisitorPuposeId { get; set; }
        public string VisitorRemark { get; set; }
        public string Status { get; set; }
        public string MeetingLocation { get; set; }
        public double ConferenceId { get; set; }
        public double VisitorTypeId { get; set; }

    }
}