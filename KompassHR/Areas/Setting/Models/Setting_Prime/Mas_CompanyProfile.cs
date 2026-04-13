using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_CompanyProfile
    {
        public double CompanyId { get; set; }        
        public string  CompanyId_Encrypted { get; set; }
        public bool  Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime  CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string PhoneNo { get; set; }
        public string WebSite { get; set; }
        public string EmailId { get; set; }
        public string IndustryType { get; set; }
        public string PAN { get; set; }        
        public string GST { get; set; }
        public string ManagerName { get; set; }
        public string CIN { get; set; }
        public bool IsActive { get; set; }
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EstablishedDate { get; set; }
        public string NotificationDays { get; set; }
        public string Logo { get; set; }
        public string Stamp { get; set; }
    }
}