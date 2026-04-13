using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_Shifts
    {
        public int ShiftId { get; set; }
        public string ShiftId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int ShiftBranchId { get; set; }
        public string ShiftName { get; set; }
        public string SName { get; set; }
        public Nullable<System.TimeSpan> BeginTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public bool ShiftFlag { get; set; }
        public int Duration { get; set; }
        public int? GraceTimeForLateComming { get; set; }
        public int? GraceTimeForEarlyGoing { get; set; }
        public int? PunchBeginDuration { get; set; }
        public Nullable<System.TimeSpan> LunchTime { get; set; }
        public int? LunchTimeMin { get; set; }
        public int? LunchTimePunchBegin { get; set; }
        public int LunchFlag { get; set; }
        public int ShiftDurationForOTCalculation { get; set; }
        public int ShiftDurationForPP { get; set; }
        public int ShiftDurationForHD { get; set; }


        public bool Coff_Half_Regular_Applicable { get; set; }
        public float Coff_Half_Regular_From { get; set; }
        public float Coff_Half_Regular_To { get; set; }
        public bool Coff_Full_Regular_Applicable { get; set; }
        public float Coff_Full_Regular_From { get; set; }
        public float Coff_Full_Regular_To { get; set; }
        public bool Coff_Full_Half_Regular_Applicable { get; set; }
        public float Coff_Full_Half_Regular_From { get; set; }
        public float Coff_Full_Half_Regular_To { get; set; }
        public bool Coff_Two_Regular_Applicable { get; set; }
        public float Coff_Two_Regular_From { get; set; }
        public float Coff_Two_Regular_To { get; set; }
        public bool Coff_Half_PH_Applicable { get; set; }
        public float Coff_Half_PH_From { get; set; }
        public float Coff_Half_PH_To { get; set; }
        public bool Coff_Full_PH_Applicable { get; set; }
        public float Coff_Full_PH_From { get; set; }
        public float Coff_Full_PH_To { get; set; }
        public bool Coff_Full_Half_PH_Applicable { get; set; }
        public float Coff_Full_Half_PH_From { get; set; }
        public float Coff_Full_Half_PH_To { get; set; }
        public bool Coff_Two_PH_Applicable { get; set; }
        public float Coff_Two_PH_From { get; set; }
        public float Coff_Two_PH_To { get; set; }
        public bool Coff_Two_Half_PH_Applicable { get; set; }
        public float Coff_Two_Half_PH_From { get; set; }
        public float Coff_Two_Half_PH_To { get; set; }
        public bool Coff_Three_PH_Applicable { get; set; }
        public float Coff_Three_PH_From { get; set; }
        public float Coff_Three_PH_To { get; set; }



    }

    public class ShiftGroup
    {
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public int ShiftFlag { get; set; }
        public bool DayNight { get; set; }
    }
}