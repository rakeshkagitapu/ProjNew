using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SPFS.Models
{
    public class SiteListViewModel
    {
        public int SiteID { get; set; }

        [Display(Name = "GDIS ORG ENTITY ID")]
        public int Gdis_org_entity_ID { get; set; }

        [Display(Name = "GDIS ORG PARENT ID")]
        public Nullable<int> Gdis_org_Parent_ID { get; set; }

        [Display(Name = "Location")]
        public string Name { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Zip")]
        public string Postal_Code { get; set; }

        
        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }

        [Display(Name = "Address")]
        public string Address
        {
            get
            {
                return Address_1 + " " + Address_2 + " " + City + " " + State;
            }
        }


    }

    public class SelectSiteGDIS
    {
        public int SiteID { get; set; }

        public int Gdis_org_entity_ID { get; set; }

        public Nullable<int> Gdis_org_Parent_ID { get; set; }

        public string Name { get; set; }
    }
}