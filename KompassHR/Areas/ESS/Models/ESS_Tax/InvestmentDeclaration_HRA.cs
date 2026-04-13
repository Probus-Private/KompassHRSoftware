using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_HRA
    {
        public int InvestmentDeclarationId { get; set; }
        public string InvestmentDeclaration_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int InvestmentDeclarationFyearId { get; set; }
        public int InvestmentDeclarationEmployeeId { get; set; }
        public string InvestmentDeclarationEmployeeNo { get; set; }
        public string InvestmentDeclarationEmployeeName { get; set; }
        public string HRA_Apr_Metro { get; set; }
        public string HRA_May_Metro { get; set; }
        public string HRA_Jun_Metro { get; set; }
        public string HRA_Jul_Metro { get; set; }
        public string HRA_Aug_Metro { get; set; }
        public string HRA_Sep_Metro { get; set; }
        public string HRA_Oct_Metro { get; set; }
        public string HRA_Nov_Metro { get; set; }
        public string HRA_Dec_Metro { get; set; }
        public string HRA_Jan_Metro { get; set; }
        public string HRA_Feb_Metro { get; set; }
        public string HRA_Mar_Metro { get; set; }
        public string HRA_Apr_CityName { get; set; }
        public string HRA_May_CityName { get; set; }
        public string HRA_Jun_CityName { get; set; }
        public string HRA_Jul_CityName { get; set; }
        public string HRA_Aug_CityName { get; set; }
        public string HRA_Sep_CityName { get; set; }
        public string HRA_Oct_CityName { get; set; }
        public string HRA_Nov_CityName { get; set; }
        public string HRA_Dec_CityName { get; set; }
        public string HRA_Jan_CityName { get; set; }
        public string HRA_Feb_CityName { get; set; }
        public string HRA_Mar_CityName { get; set; }
        public string HRA_Apr_LandlordName { get; set; }
        public string HRA_May_LandlordName { get; set; }
        public string HRA_Jun_LandlordName { get; set; }
        public string HRA_Jul_LandlordName { get; set; }
        public string HRA_Aug_LandlordName { get; set; }
        public string HRA_Sep_LandlordName { get; set; }
        public string HRA_Oct_LandlordName { get; set; }
        public string HRA_Nov_LandlordName { get; set; }
        public string HRA_Dec_LandlordName { get; set; }
        public string HRA_Jan_LandlordName { get; set; }
        public string HRA_Feb_LandlordName { get; set; }
        public string HRA_Mar_LandlordName { get; set; }
        public string HRA_Apr_PanNo { get; set; }
        public string HRA_May_PanNo { get; set; }
        public string HRA_Jun_PanNo { get; set; }
        public string HRA_Jul_PanNo { get; set; }
        public string HRA_Aug_PanNo { get; set; }
        public string HRA_Sep_PanNo { get; set; }
        public string HRA_Oct_PanNo { get; set; }
        public string HRA_Nov_PanNo { get; set; }
        public string HRA_Dec_PanNo { get; set; }
        public string HRA_Jan_PanNo { get; set; }
        public string HRA_Feb_PanNo { get; set; }
        public string HRA_Mar_PanNo { get; set; }
        public string HRA_Apr_Address { get; set; }
        public string HRA_May_Address { get; set; }
        public string HRA_Jun_Address { get; set; }
        public string HRA_Jul_Address { get; set; }
        public string HRA_Aug_Address { get; set; }
        public string HRA_Sep_Address { get; set; }
        public string HRA_Oct_Address { get; set; }
        public string HRA_Nov_Address { get; set; }
        public string HRA_Dec_Address { get; set; }
        public string HRA_Jan_Address { get; set; }
        public string HRA_Feb_Address { get; set; }
        public string HRA_Mar_Address { get; set; }
        public double HRA_Apr_CurrentAmt { get; set; }
        public double HRA_May_CurrentAmt { get; set; }
        public double HRA_Jun_CurrentAmt { get; set; }
        public double HRA_Jul_CurrentAmt { get; set; }
        public double HRA_Aug_CurrentAmt { get; set; }
        public double HRA_Sep_CurrentAmt { get; set; }
        public double HRA_Oct_CurrentAmt { get; set; }
        public double HRA_Nov_CurrentAmt { get; set; }
        public double HRA_Dec_CurrentAmt { get; set; }
        public double HRA_Jan_CurrentAmt { get; set; }
        public double HRA_Feb_CurrentAmt { get; set; }
        public double HRA_Mar_CurrentAmt { get; set; }
        public double HRA_Total { get; set; }

        public string HRAProof_Upload { get; set; }
        public string FilePath { get; set; }
    }
}