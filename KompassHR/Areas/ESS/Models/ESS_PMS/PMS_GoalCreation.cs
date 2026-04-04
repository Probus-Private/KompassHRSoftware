using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_GoalCreation
    {
        public long GoalCreationID { get; set; }
        public string GoalCreationIDEncrypted { get; set; }
        public long CmpID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string GoalTitle { get; set; }
        public string Description { get; set; }

        //[Required(ErrorMessage = "Goal Type is required")]
        public int GoalTypeId { get; set; }

       // [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; } 
        public int Alignment { get; set; }

        public string SuccessMatrix { get; set; }
        public int Weightage { get; set; }

        //[Required(ErrorMessage = "Visibility is required")]
        public string Visibility { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    //public class ObjectiveViewModel
    //{
    //    public int ObjectiveId { get; set; }
    //    public string ObjectiveTitle { get; set; }
    //    public string EmployeeName { get; set; }
    //    public string ObjectiveType { get; set; }
    //    public string Quarter { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public int Progress { get; set; }
    //    public string ProgressStatus { get; set; }
    //    public List<KeyResultViewModel> KeyResults { get; set; }
    //}

    //public class KeyResultViewModel
    //{
    //    public string KRTitle { get; set; }
    //    public int KRProgress { get; set; }
    //}

}