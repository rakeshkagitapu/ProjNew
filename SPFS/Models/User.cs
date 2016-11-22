
using SPFS.DAL;
using SPFS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    /// <summary>
    /// Class User.
    /// </summary>
    public class UserViewModel
    {

        /// </summary>
        /// <value>The user identifier.</value>
        public UserViewModel()
        {
            SPFS_USERSITES = new HashSet<SPFS_USERSITES>();
        }

        public int UserID { get; set; }
        [Required]
        [StringLength(75)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(75)]
        public string Email { get; set; }

        [Required]
        [StringLength(75)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(75)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }


        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleID { get; set; }
        public virtual SPFS_ROLES SPFS_ROLES { get; set; }


        [Display(Name = "Locations")]
        public List<String> UserSites { get; set; }


        public virtual ICollection<SPFS_USERSITES> SPFS_USERSITES { get; set; }

        public Nullable<System.DateTime> Active_Date { get; set; }
        public System.DateTime Created_date { get; set; }
        public string Created_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
        public string Modified_by { get; set; }

    }

    public class UserListViewModel
    {

        public int UserID { get; set; }


        [Display(Name = "User Name")]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }


        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }


        [Display(Name = "Role")]
        public string RoleName { get; set; }


        [Display(Name = "Locations")]
        public string UserSitesName
        {
            get
            {
                var sites = string.Join(",", UserSites.ToArray());
                return sites;
            }
        }
        public List<String> UserSites { get; set; }


    }

    public class UserExportViewModel
    {

        public int UserID { get; set; }


        [Display(Name = "User Name")]
        public string UserName { get; set; }

        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }


        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Active")]
        public bool SPFS_Active { get; set; }


        [Display(Name = "Role")]
        public string RoleName { get; set; }


        [Display(Name = "Location")]
        public string UserSiteName { get; set; }
       


    }
}