using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Dashboards.Models
{
    public class PMS_Dashboard
    {
        public class ObjectiveHierarchyVM
        {
            public long Id { get; set; }
            public long? ParentId { get; set; }
            public string Name { get; set; }
            public int ProgressPercentage { get; set; }
            public string NodeType { get; set; }
            public int ChildCount { get; set; }
        }
    }
}