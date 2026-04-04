using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class IncomeTax_DeductorResponsible
    {
        public double DeductorResponsibleId { get; set; }
        public string DeductorResponsibleId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CmpId { get; set; }
        public string DeductorTAN { get; set; }
        public string DeductorPAN { get; set; }
        public string DeductorName { get; set; }
        public string DeductorDivision { get; set; }
        public string ReturnUtility { get; set; }
        public string DeductorFlatDoorBlockNo { get; set; }
        public string DeductorNameofPremisesBuilding { get; set; }
        public string DeductorRoadStreetLane { get; set; }
        public string DeductorAreaLocality { get; set; }
        public string DeductorTownCityDistrict { get; set; }
        public string DeductorState { get; set; }
        public string DeductorStateCode { get; set; }
        public string DeductorPin { get; set; }
        public string DeductorEmail { get; set; }
        public string DeductorSTD { get; set; }
        public string DeductorPhone { get; set; }
        public string DeductorMobile { get; set; }
        public string DeductorType { get; set; }
        public string IsChangeAddressDeductor { get; set; }
        public string ResponsibleName { get; set; }
        public string ResponsibleDesignation { get; set; }
        public string ResponsibleFlatDoorBlockNo { get; set; }
        public string ResponsibleNameofPremisesBuilding { get; set; }
        public string ResponsibleRoadStreetLane { get; set; }
        public string ResponsibleAreaLocality { get; set; }
        public string ResponsibleTownCityDistrict { get; set; }
        public string ResponsibleState { get; set; }
        public string ResponsibleStateCode { get; set; }
        public string ResponsiblePin { get; set; }
        public string ResponsibleEmail { get; set; }
        public string ResponsibleSTD { get; set; }
        public string ResponsiblePhone { get; set; }
        public string ResponsibleMobile { get; set; }
        public string ResponsiblePAN { get; set; }
        public string IsChangeAddressResponsible { get; set; }
        public string RPUFileName { get; set; }
        public string Form16FullName { get; set; }
        public string Form16SO { get; set; }
        public string Form16Designation { get; set; }
        public string Form16Palace { get; set; }
    }
}