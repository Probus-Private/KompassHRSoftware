using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Preboarding
{
    public class Preboarding_ModuleSetting
    {

        public double ModuleSettingID { get; set; }
        public string ModuleSettingID_Encrypted { get; set; }
        public double CmpId { get; set; }
        public bool Personal { get; set; }
        public bool Address { get; set; }
        public bool Reference { get; set; }
        public bool Bank { get; set; }
        public bool Statutory { get; set; }
        public bool Family { get; set; }
        public bool Qualification { get; set; }
        public bool UploadDocument { get; set; }
        public bool Photo { get; set; }
        public bool Signature { get; set; }
        public bool PreEmployer { get; set; }
        public bool Skill { get; set; }
        public bool Language { get; set; }
        public bool Other { get; set; }
        public string CompulsoryModule { get; set; }
    }
}