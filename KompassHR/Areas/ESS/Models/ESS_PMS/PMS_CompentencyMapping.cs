using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_CompentencyMapping
    {
        public double CompetencyMappingId { get; set; }          
        public string CompetencyMappingId_Encrypted { get; set;}
        public bool Deactivate { get; set; }                     
        public bool UseBy { get; set; }                          
        public string CreatedBy { get; set; }                    
        public DateTime CreatedDate { get; set; }                
        public string ModifiedBy { get; set; }                   
        public DateTime ModifiedDate { get; set; }               
        public string MachineName { get; set; }                  
        public double DesignationID { get; set; }                
        public double CompetencyID { get; set; }                 
       // public double Weightage { get; set; }
        public string Description { get; set; }
        public double PMS_YearId { get; set; }
    }
}