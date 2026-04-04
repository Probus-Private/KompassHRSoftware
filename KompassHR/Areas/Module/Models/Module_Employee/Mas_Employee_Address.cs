using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Address
    {
        public double AddressId { get; set; }
        public string AddressId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double AddressEmployeeId { get; set; }
        public string PresentPin { get; set; }
        public string PresentState { get; set; }
        public string PresentDistrict { get; set; }
        public string PresentTaluka { get; set; }
        public string PresentPO { get; set; }
        public string PresentCity { get; set; }
        public string PresentPostelAddress { get; set; }
        public string PermanentPin { get; set; }
        public string PermanentState { get; set; }
        public string PermanentDistrict { get; set; }
        public string PermanentTaluka { get; set; }
        public string PermanentPO { get; set; }
        public string PermanentCity { get; set; }
        public string PermanentPostelAddress { get; set; }
        public bool PermanentAddressSameAsCurrentAddress { get; set; }
    }
}