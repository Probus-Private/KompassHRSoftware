using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_OtherTDS
    {
     public double OtherTDSId { get; set; }
     public string   OtherTDSId_Encrypted { get; set; }
     public bool   Deactivate { get; set; }
     public bool UseBy { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string MachineName { get; set; }
    public int OtherTDSFyearId { get; set; }
    public double OtherTDSEmployeeId { get; set; }
    public double OtherTypeId { get; set; }
    public float TotalAmount { get; set; }
    public float TDSAmount { get; set; }
    public DateTime MonthYear { get; set; }
    public string Remark { get; set; }

    }
}