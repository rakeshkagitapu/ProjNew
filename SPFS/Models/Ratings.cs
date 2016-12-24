using SPFS.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public bool IsCurrentRatings { get; set; }
        public bool IsStagingRatings { get; set; }
        public bool IsPreviousRatings { get; set; }
        public bool IsPreviousStagingRatings { get; set; }

        public bool IsAlert { get; set; }

        public bool EditMode { get; set; }

        public bool ShowResult { get; set; }

        public bool OldResults { get; set; }
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
        public int Rating_period { get; set; }
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
                if (ErrorInformation != null)
                {
                    var data = ErrorInformation.Select(hm => hm.ErrorMessage);
                    return string.Join("\r\n", data);
                }
                else
                {
                    return "Nothing found";
                }
            }
        }
        public List<ErrorDetails> ErrorInformation { get; set; }

        public int ExcelDiferentiatorID { get; set; }

        public string CombinedKey { get; set; }


        public decimal Total_Spend { get; set; }

        [Display(Name = "Reject Incidents")]
        public int Reject_incident_count { get; set; }

        [Display(Name = "Reject Parts")]
        public int Reject_parts_count { get; set; }

        public double PPM { get; set; }

        public double IPM { get; set; }

        public double PCT { get; set; }

        public Nullable<int> Gdis_org_Parent_ID { get; set; }
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

        [DisplayName("ERP_Supplier_ID")]
        [Display(Name = "ERP_Supplier_ID")]
        public string ERP_Supplier_ID { get; set; }

        [DisplayName("Inbound")]
        [Display(Name = "Inbound")]
        public int Inbound_parts { get; set; }

        [DisplayName("OnTime Quantity")]
        [Display(Name = "OnTime Quantity")]
        public int OTR { get; set; }

        [DisplayName("OnTime Quantity Due")]
        [Display(Name = "OnTime Quantity Due")]
        public int OTD { get; set; }

        [DisplayName("Premium Freight")]
        [Display(Name = "Premium Freight")]
        public int PFR { get; set; }



    }
}