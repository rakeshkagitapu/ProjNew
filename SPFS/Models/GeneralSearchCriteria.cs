using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    public class GeneralSearchCriteria
    {
        public int? Active { get; set; }
        public string SearchText { get; set; }
        public string CurrentSort { get; set; }
        public int? Page { get; set; }       
    }
}