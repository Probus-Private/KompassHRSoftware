using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.MyContact
{
    public class Mas_MyContact
    {
        public double MyContactId { get; set; }
        public string MyContactId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ContactCategoryID { get; set; }
        public string PersonName { get; set; }
        public string CompanyName { get; set; }
        public string Designation { get; set; }
        public string EmailID { get; set; }
        public string ContactNo { get; set; }
        public string WhatsAppNo { get; set; }
        public string Address { get; set; }
        public bool OpenToAll { get; set; }
        public double MyContactEmployeeID { get; set; }
        public string ContactCategoryName { get; set; }
        //public string OpenToAll { get; set; }
    }
}