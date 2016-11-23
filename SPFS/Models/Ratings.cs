﻿using SPFS.Model;
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

        [Display(Name ="Location:")]
        [Required(ErrorMessage = "Please select Location")]
        public int? SiteID { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        [Display(Name = "Entry Type:")]
        public bool isUpload { get; set; }
        public  virtual List<RatingRecord> RatingRecords { get; set; }
    }
    public class ExcelRatingsViewModel : RatingsViewModel
    {

        [Display(Name = "Upload File:")]
        public HttpPostedFileBase UploadFile { get; set; }


    }
    public class RatingRecord
    {
        public string SupplierName { get; set; }
        public int RatingsID { get; set; }
        public System.DateTime Rating_period { get; set; }
        public int SiteID { get; set; }
        public int CID { get; set; }
        public decimal Inbound_parts { get; set; }
        public decimal OTR { get; set; }
        public decimal OTD { get; set; }
        public decimal PFR { get; set; }
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


    }

    public class HistoricalRecordsCheck
    {
        public int SiteID { get; set; }
        public int CID { get; set; }
        public System.DateTime Initial_submission_date { get; set; }
    }
}