using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TMS_MOM
    {
        public double MOMId { get; set; }
        public string MOMId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public DateTime DocDate { get; set; }
        public double DocNo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Title { get; set; }
        public double Owner { get; set; }
        public string Agenda { get; set; }

        public string ActionItem { get; set; }
        public string Priority { get; set; }
        public double Employee { get; set; }
        public DateTime Deadline { get; set; }

        public bool NextMeeting { get; set; }
        public DateTime NextMeetDate { get; set; }
        public DateTime NextMeetTime { get; set; }
        public string NextMeetAgenda { get; set; }
        public String Remark { get; set; }

        //public List<MOM_Detail> MOMDetails { get; set; }
        public List<MOM_Detail> MOMDetails { get; set; } = new List<MOM_Detail>();
    }


    public class MOM_Detail
    {
        public double MOMDetailsId { get; set; }
        public string MOMDetailsId_Encrypted { get; set; }
        public bool IsDeleted { get; set; }

        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ActionItem { get; set; }
        public string Priority { get; set; }
        public double Employee { get; set; }
        public String EmployeeName { get; set; }
        public DateTime Deadline { get; set; }
        public double MOMId { get; set; }
    }
}