using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class Login
    {
        public string ESSLoginID { get; set; }
        public string CustomerCode { get; set; }
        public string ESSPassword { get; set; }
        public bool ESSLock { get; set; }
        public bool IsExit { get; set; }
        public int EmployeeId { get; set; }
        public bool RememberMe { get; set; }
    }
}