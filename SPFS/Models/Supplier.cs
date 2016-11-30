using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    public class SupplierListViewModel
    {
        
      
        public int CID { get; set; }

        [Display(Name = "DUNS")]
        public string Duns { get; set; }

        [Display(Name = "Supplier Name")]
        public string Name { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }

        [Display(Name="Address") ]
        public string Address
        {
            get
            {
                return Address_1 + " " + Address_2 + " " + City + " " + State;
                //return string.Format("Address1 :{0}; Address2:{1}\n City: {2} / State: {3}",new string[] { this.Address_1.Trim(),this.Address_2.Trim(),this.City.Trim(),this.State.Trim() }); //Address_1 + " " + Address_2 + " " + City + " " + State; 
            }
        }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

       
        [Display(Name = "Zip")]
        public string Postal_Code { get; set; }

        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }
    }

    public class SupplierCacheViewModel
    {        
        public int CID { get; set; }

        [Display(Name = "DUNS")]
        public string Duns { get; set; }

        public string ERPSupplierID { get; set; }

        public int Gdis_org_entity_ID { get; set; }

        public string CombinedKey
        {
            get
            {
                return ERPSupplierID + Gdis_org_entity_ID;
            }
        }


        [Display(Name = "Supplier Name")]
        public string Name { get; set; }
        
        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }

        public int SpendSupplierID { get; set; }
    }
}