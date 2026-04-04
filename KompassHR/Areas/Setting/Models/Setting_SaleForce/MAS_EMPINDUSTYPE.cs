using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_EMPINDUSTYPE
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public int BUCode { get; set; }
        public int UserId { get; set; }
       public int MAS_Employee_Fid { get; set; }
       // public int EmployeeId { get; set; }   
        public int MAS_INDUSTRYTYPE { get; set; }
        public bool Deleted { get; set; }
    }
}