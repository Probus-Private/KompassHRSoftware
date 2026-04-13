using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_KompassHR_HelpDesk
{
    public class Kompass_HelpDesk
    {
        public int HelpDeskId { get; set; }
        public string HelpDeskId_Encrypted { get; set; }
        public int HelpDesk_DocNo { get; set; }
        public DateTime HelpDesk_DocDate { get; set; }
        public int HelpDesk_RequestBy { get; set; }
        public string HelpDesk_Priority { get; set; }
        public string HelpDesk_RequestType { get; set; }
        public string RequestType_DescriptionOther { get; set; }
        public string HelpDesk_Title { get; set; }
        public string HelpDesk_Description { get; set; }
        public string AttachFile { get; set; }
        public string FilePath { get; set; }
        public string ExpectedResult { get; set; }
        public string ActualResult { get; set; }
        public string ModuleName { get; set; }
        public string ResolutionComment { get; set; }
        public string ResolutionActualResult { get; set; }
        public string ResolutionDate { get; set; }
    }
}