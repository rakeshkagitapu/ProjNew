using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    public class Upload
    {
        public int CID { get; set; }
        public string DUNS { get; set; }

        public int ERP_Supplier_ID { get; set; }

        public int Inbound { get; set; }

        public int OntimeQuantity_Received { get; set; }

        public int OntimeQuantity_Due { get; set; }

        public int Premium_Freight { get; set; }



    }
}