using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Logistic
{
    public class Logistic_CourierServiceProvider
    {
        public double CourierServiceProviderID { get; set; }
        public string CourierServiceProviderID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CourierServiceProviderName { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public string WhatappsNo { get; set; }
        public string EmailId { get; set; }
        public string CourierType { get; set; }
        public string Destination { get; set; }
    }
}