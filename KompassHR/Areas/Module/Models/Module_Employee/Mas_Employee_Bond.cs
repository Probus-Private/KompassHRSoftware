using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Bond
    {
       public double  BondID     {get;set;}
       public string  BondID_Encrypted  {get;set;}
       public bool  Deactivate        {get;set;}
       public bool UseBy             {get;set;}
       public string CreatedBy         {get;set;}
       public DateTime CreatedDate       {get;set;}
       public string ModifiedBy        {get;set;}
       public DateTime  ModifiedDate      {get;set;}
       public string MachineName       {get;set;}
       public double  BondEmployeeId    {get;set;}
       public DateTime  StartDate         {get;set;}
       public DateTime EndDate           {get;set;}
       public float  Amount            {get;set;}
       public string Description       {get;set;}
       public string BondAttachment    {get;set;}

    }
}