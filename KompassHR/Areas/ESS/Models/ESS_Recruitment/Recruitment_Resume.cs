using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_Resume
    {
        public double SrNo { get; set; }
        public double ResumeId { get; set; }
        public string ResumeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string Resume_ResourceId { get; set; }
        public double ResumeJobOpeningId { get; set; }
        public double ResumeReferEmployeeId { get; set; }
        public string Salutation { get; set; }
        public string CandidateName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string AlternateMobileNo { get; set; }
        public string QualificationRemark { get; set; }
        public string RelevantExperience { get; set; }
        public string Gender { get; set; }
        public string TotalExperience { get; set; }
        public string CTC { get; set; }
        public string ExpectedCTC { get; set; }
        public string OfferRemark { get; set; }
        public string CurrentCity { get; set; }
        public string CurrentlyWorking { get; set; }
        public string ResumePath { get; set; }
        public string ResumeStatus { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string DOB { get; set; }
        public string MaritalStatus { get; set; }
        public string HighestQualification { get; set; }
        public string LastDesignation { get; set; }
        public string LastWorkingDay { get; set; }
        public string LastCompanyName { get; set; }
        public string RelevantSkill { get; set; }
        public string BondMonth { get; set; }
        public string ResumeCategory { get; set; }
        public string FamilyDetails { get; set; }
        public int Age { get; set; }
        public string LeaglBoand { get; set; }
        public double NoticePeriodDays { get; set; }
        public string ServingNoticePeriod { get; set; }
        public string HoldingAnyOffer { get; set; }
        public string TechnicalDetails { get; set; }
        public string CommunicationSkill { get; set; }
        public string JobDescription { get; set; }
        public string ExpatLocal { get; set; }
        public string ReadyToTraval { get; set; }
        public string Refreance { get; set; }
        public double ResumeSource { get; set; }
        public double ResumeSubSource { get; set; }
        public string HrSelection { get; set; }
        public string HrRanking { get; set; }
        public string Nationality { get; set; }
        public double SutableForDesignation { get; set; }
        public string HRComment { get; set; }
        public string CandidateComment { get; set; }
        public string IsManualAuto { get; set; }
        public string CandidateType { get; set; }
        public string FilePath { get; set; }
        public DateTime ExpectedJoiningDate { get; set; }
    }

    public class ShortList
    {
        public int ResumeId { get; set; }
    }

    public class AssignResourceIdToAllCandidate
    {
        public string ResumeId_Encrypted { get; set; }
    }

    public class CandidateData
    {
        public string Salutation { get; set; }
        public string CandidateName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string AlternateMobileNo { get; set; }
        public string QualificationRemark { get; set; }
        public string RelevantExperience { get; set; }
        public string Gender { get; set; }
        public string TotalExperience { get; set; }
        public string CTC { get; set; }
        public string ExpectedCTC { get; set; }
        public string OfferRemark { get; set; }
        public string CurrentCity { get; set; }
        public string CurrentlyWorking { get; set; }
        public string ResumePath { get; set; }
        public string ResumeStatus { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DOB { get; set; }
        public string MaritalStatus { get; set; }
        public string HighestQualification { get; set; }
        public string LastDesignation { get; set; }
        public DateTime? LastWorkingDay { get; set; }
        public string LastCompanyName { get; set; }
        public string RelevantSkill { get; set; }
        public string BondMonth { get; set; }
        public string ResumeCategory { get; set; }
        public string FamilyDetails { get; set; }
        public int Age { get; set; }
        public string LeaglBoand { get; set; }
        public double NoticePeriodDays { get; set; }
        public string ServingNoticePeriod { get; set; }
        public string HoldingAnyOffer { get; set; }
        public string TechnicalDetails { get; set; }
        public string CommunicationSkill { get; set; }
        public string JobDescription { get; set; }
        public string ExpatLocal { get; set; }
        public string ReadyToTraval { get; set; }
        public string Refreance { get; set; }
        public double ResumeSource { get; set; }
        public double ResumeSubSource { get; set; }
        public string HrSelection { get; set; }
        public string HrRanking { get; set; }
        public string Nationality { get; set; }
        public double SutableForDesignation { get; set; }
        public string HRComment { get; set; }
        public string CandidateComment { get; set; }
        public string IsManualAuto { get; set; }
        public string CandidateType { get; set; }
        public DateTime ExpectedJoiningDate { get; set; }

    }

    public class BulkCandidateData
    {
        public int SrNo { get; set; }
        public string Salutation { get; set; }
        public string CandidateName { get; set; }
        public string MobileNo { get; set; }
        public string AlternateMobileNo { get; set; }
        public string EmailId { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string CurrentCity { get; set; }
        public string MaritalStatus { get; set; }
        public string HighestQualification { get; set; }
        public string SpecificQualification { get; set; }
        public string Nationality { get; set; }
        public string FamilyInfo { get; set; }
        public string CandidateType { get; set; }
        public string CurrentlyWorking { get; set; }
        public string CurrentCompanyName { get; set; }
        public string TotalExperience { get; set; }
        public string RelevantExperience { get; set; }
        public string CurrentDesignation { get; set; }
        public string SutableForDesignation { get; set; }
        public string TechnicalDetails { get; set; }
        public string CommunicationSkill { get; set; }
        public string RelevantSkill { get; set; }
        public string ReadyToTravel { get; set; }
        public string CurrentCTC { get; set; }
        public string ExpectedCTC { get; set; }
        public string LocalOrOutstation { get; set; }
        public string JobDescriptions { get; set; }
        public string NoticePeriodApplicable { get; set; }
        public string BalanceNoticePeriod { get; set; }
        public string LegalBondApplicable { get; set; }
        public string BalanceLegalBondMonths { get; set; }
        public string HoldingAnyOffers { get; set; }
        public string OfferRemark { get; set; }
        public string HRSelection { get; set; }
        public string HRRanking { get; set; }
        public string ResumeSource { get; set; }
        public double ResumeSubSource { get; set; }
        public string HRComment { get; set; }
        public DateTime ExpectedJoiningDate { get; set; }

    }
}