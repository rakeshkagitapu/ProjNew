using SPFS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SPFS.Models
{
    public class RatingsViewModel
    {
        public string SiteName { get; set; }

        [Display(Name = "Location:")]
        [Required(ErrorMessage = "Please select Location")]
        public int? SiteID { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        [Display(Name = "Entry Type:")]
        public bool isUpload { get; set; }
        public virtual List<RatingRecord> RatingRecords { get; set; }
    }
    public class ExcelRatingsViewModel : RatingsViewModel
    {
       

        [Display(Name = "Upload File:")]
        public HttpPostedFileBase UploadFile { get; set; }


    }
    public class RatingRecord
    {
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }
        public int RatingsID { get; set; }
        public System.DateTime Rating_period { get; set; }
        public int SiteID { get; set; }
        public int CID { get; set; }

        [Display(Name = "Inbound Parts")]
        public int Inbound_parts { get; set; }

        [Display(Name = "On Time Quantity Received")]
        public int OTR { get; set; }

        [Display(Name = "On Time Quantity Due")]
        public int OTD { get; set; }

        [Display(Name = "Premium Frieght Instances")]
        public int PFR { get; set; }
        public System.DateTime Initial_submission_date { get; set; }
        public Nullable<bool> Temp_Upload_ { get; set; }
        public Nullable<bool> Interface_flag { get; set; }
        public int UserID { get; set; }
        public System.DateTime Created_date { get; set; }
        public string Created_by { get; set; }
        public Nullable<System.DateTime> Modified_date { get; set; }
        public string Modified_by { get; set; }

        public virtual SPFS_SITES SPFS_SITES { get; set; }
        public virtual SPFS_SUPPLIERS SPFS_SUPPLIERS { get; set; }
        public virtual SPFS_USERS SPFS_USERS { get; set; }

        public string DUNS { get; set; }

        [Display(Name = "ERP SupplierID")]
        public string ERP_Supplier_ID { get; set; }

        public int Gdis_org_entity_ID { get; set; }
        public string ErrorDetails
        {
            get
            {
                var data = ErrorInformation.Select(hm => hm.ErrorMessage);
                return string.Join("\r\n", data);
            }
        }
        public List<ErrorDetails> ErrorInformation { get; set; }

        public int ExcelDiferentiatorID { get; set; }

        public string CombinedKey { get; set; }
    }

    public class HistoricalRecordsCheck
    {
        public int SiteID { get; set; }
        public int CID { get; set; }
        public System.DateTime Initial_submission_date { get; set; }
    }

    public class ErrorDetails
    {
        public string Key { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class ExportedRecord
    {
        public int CID { get; set; }
        public string DUNS { get; set; }

        public string ERP_Supplier_ID { get; set; }

        public int Inbound_parts { get; set; }

        public int OTR { get; set; }

        public int OTD { get; set; }

        public int PFR { get; set; }



    }
}