using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_ApproveReject
    {
        public string InvestmentDeclarationId { get; set; }
        public string EmployeeId { get; set; }
        public string InvestmentDeclarationFyearId { get; set; }
        public string ActualStatus { get; set; }
        public string Status { get; set; }
        public string RejectRemark { get; set; }
    }
}