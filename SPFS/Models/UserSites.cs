using SPFS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    public class UserSites
    {

        public int UserSitesID { get; set; }
        public int UserID { get; set; }
        public int SiteID { get; set; }
        public System.DateTime Created_date { get; set; }
        public string Created_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
        public string Modified_by { get; set; }

        public virtual SPFS_SITES SPFS_SITES { get; set; }
        public virtual SPFS_USERS SPFS_USERS { get; set; }
    }
    public class UserSitesViewModel
    {

        public string UserName { get; set; }
       
        [Required(ErrorMessage = "Please select User")]
        public int? UserID { get; set; }
       public List<int> SelectedSiteIDs { get; set; }

        public System.DateTime Created_date { get; set; }
        public string Created_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
        public string Modified_by { get; set; }
    }
    public class SiteUsersViewModel
    {

        public string SiteName { get; set; }

        [Required(ErrorMessage = "Please select Location")]
        public int? SiteID { get; set; }
        public List<int> SelectedUserIDs { get; set; }

        public System.DateTime Created_date { get; set; }
        public string Created_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
        public string Modified_by { get; set; }
    }
}