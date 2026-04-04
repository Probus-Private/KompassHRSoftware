using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Statutory
    {
        public double StatutoryId { get; set; }
        public string StatutoryId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double StatutoryEmployeeId { get; set; }
        public bool ESIC_Applicable { get; set; }
        public double ESIC_CodeId { get; set; }
        public string ESIC_NO { get; set; }
        public string ESIC_ClosingDate { get; set; }
        public bool ESIC_IS_OldESICNo { get; set; }
        public string ESIC_PreviousESICNo { get; set; }
        public bool ESIC_IS_LinkWithESIC { get; set; }
        public bool PT_Applicable { get; set; }
        public double PT_CodeId { get; set; }
        public bool LWF_Applicable { get; set; }
        public double LWF_CodeId { get; set; }
        public string LWF_LIN { get; set; }
        public bool Gratuity_Applicable { get; set; }
        public string Gratuity_No { get; set; }
        public bool PF_Applicable { get; set; }
        public double PF_CodeId { get; set; }
        public bool? PF_Limit { get; set; }
        public bool? PF_EPS { get; set; }
        public string PF_VPF { get; set; }
        public string PF_FSType { get; set; }
        public string PF_FS_Name { get; set; }
        public string PF_UAN { get; set; }
        public string PF_NO { get; set; }
        public string PF_Nominee1 { get; set; }
        public string PF_Reletion1 { get; set; }
        public string PF_DOB1 { get; set; }
        public string PF_Share1 { get; set; }
        public string PF_Address1 { get; set; }
        public string PF_GuardianName1 { get; set; }
        public string PF_Nominee2 { get; set; }
        public string PF_Reletion2 { get; set; }
        public string PF_DOB2 { get; set; }
        public string PF_Share2 { get; set; }
        public string PF_Address2 { get; set; }
        public string PF_GuardianName2 { get; set; }
        public string PF_Nominee3 { get; set; }
        public string PF_Reletion3 { get; set; }
        public string PF_DOB3 { get; set; }
        public string PF_Share3 { get; set; }
        public string PF_Address3 { get; set; }
        public string PF_GuardianName3 { get; set; }
        public string PF_MobileNo { get; set; }
        public string PF_BankName { get; set; }
        public string PF_BankIFSC { get; set; }
        public string PF_Account { get; set; }
        public bool PF_1952 { get; set; }
        public bool PF_1995 { get; set; }
        public string PF_PreviousPFNo { get; set; }
        public string PF_ExitDate { get; set; }
        public string PF_CertificateNo { get; set; }
        public string PF_PPO { get; set; }
        public bool PF_OldUANNo { get; set; }
        public bool PF_LinkWithUAN { get; set; }
        public bool Medical_Insurance_Applicable { get; set; }
        public bool Accidental_Policy_Applicable { get; set; }

        public double PTSlab_MasterId { get; set; }
        public double LWFSlab_MasterId { get; set; }
        public double PFWages_MasterId { get; set; }

        public int CmpID { get; set; }
        public int BranchID { get; set; }
        public int BU_StateID { get; set; }
        public int BUComplienceId { get; set; }
        public string BUComplienceId_Encrypted { get; set; }

    }
}