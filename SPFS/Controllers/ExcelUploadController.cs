using Excel;
using SPFS.DAL;
using SPFS.Helpers;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Controllers
{
    public class ExcelUploadController : BaseController
    {
        // GET: ExcelUpload
        public ActionResult Index(int? SiteID, bool isUpload = true)
        {
            ExcelRatingsViewModel ratingsViewModel = new ExcelRatingsViewModel { SiteID = SiteID, isUpload = isUpload };
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;

            CreateListViewBags();
            return View(ratingsViewModel);
        }

        private void CreateListViewBags()
        {
            Utilities util = new Utilities();
            List<SelectListItem> sites;

            using (Repository UserRep = new Repository())
            {

                if (util.GetCurrentUser().RoleID == 1)
                {
                    sites = (from ste in UserRep.Context.SPFS_SITES
                             select new SelectListItem { Value = ste.SiteID.ToString(), Text = ste.Name }).ToList();
                }
                else
                {
                    var usrid = util.GetCurrentUser().UserID;
                    sites = (from ste in UserRep.Context.SPFS_SITES
                             join uste in UserRep.Context.SPFS_USERSITES on ste.SiteID equals uste.SiteID
                             where uste.UserID == usrid
                             select new SelectListItem { Value = ste.SiteID.ToString(), Text = ste.Name }).ToList();
                }


            }

            ViewBag.Months = util.GetMonths();
            ViewBag.Years = util.GetYears();
            ViewBag.Sites = sites;
        }

        //checks if there are any existing uploads 
        // displays warning if there are existing uploads in same month
        // Initializes partial view
        //[HttpPost]
        //[MultipleSubmitDefaultAttribute(Name = "action", Argument = "Search")]
        //public ActionResult Search(FormCollection form)
        //{

        //    return View();
        //}

        //checks if there are any existing uploads 
        // displays warning if there are existing uploads in same month
        // Initializes partial view
        [HttpPost]
        public ActionResult Index(int? siteID, int month, int year)
        {
            ExcelRatingsViewModel excelViewModel = new ExcelRatingsViewModel();
            var historicalRecords = new List<HistoricalRecordsCheck>();
            DateTime date = new DateTime(year, month, 01);
            Utilities util = new Utilities();
            using (Repository Rep = new Repository())
            {
                historicalRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                     where ratings.SiteID == siteID && ratings.Initial_submission_date == date
                                     select new HistoricalRecordsCheck()
                                     {
                                         SiteID = ratings.SiteID,
                                         CID = ratings.CID,
                                         Initial_submission_date = ratings.Initial_submission_date
                                     }).ToList().Union
                                     (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
                                      where ratings.SiteID == siteID && ratings.Initial_submission_date == date
                                      select new HistoricalRecordsCheck()
                                      {
                                          SiteID = ratings.SiteID,
                                          CID = ratings.CID,
                                          Initial_submission_date = ratings.Initial_submission_date
                                      }).ToList();

            }
            if (historicalRecords.Count > 0)
            {
                util.GetDivElements("There are existing records uploaded for this month", "alert alert-warning", "Warning ! ");
            }


            return View();
        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "Upload")]
        public ActionResult Upload(ExcelRatingsViewModel ratingModel)
        {
            if (ModelState.IsValid)
            {

                if (ratingModel.UploadFile != null && ratingModel.UploadFile.ContentLength > 0)
                {
                    // ExcelDataReader works with the binary Excel file, so it needs a FileStream
                    // to get started. This is how we avoid dependencies on ACE or Interop:
                    Stream stream = ratingModel.UploadFile.InputStream;
                    DataSet result = null;

                    if (ratingModel.UploadFile.FileName.EndsWith(".xls"))
                    {
                        IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        reader.IsFirstRowAsColumnNames = true;
                        result = reader.AsDataSet();
                        reader.Close();
                    }
                    else if (ratingModel.UploadFile.FileName.EndsWith(".xlsx"))
                    {
                        IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        reader.IsFirstRowAsColumnNames = true;
                        result = reader.AsDataSet();
                        reader.Close();
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                    ratingModel = ProcessExcelDataintoViewModel(ratingModel, result);
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            return View("Index", ratingModel);
        }

        private ExcelRatingsViewModel ProcessExcelDataintoViewModel(ExcelRatingsViewModel ratingModel, DataSet result)
        {
            List<RatingRecord> ratings = result.Tables[0].ToList<RatingRecord>();

            ratingModel.RatingRecords = ratings;
            return ratingModel;

        }
        //checks if there are any existing uploads 
        // displays warning if there are existing uploads in same month
        // Initializes partial view
        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "Search")]
        public ActionResult Search(ExcelRatingsViewModel ratingModel)
        {
            CreateListViewBags();
            return View("Index",ratingModel);
        }
    }
}