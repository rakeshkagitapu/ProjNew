using Excel;
using SPFS.DAL;
using SPFS.Helpers;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Controllers
{
    public class RatingsController :BaseController
    {
        // GET: Ratings
        public ActionResult Index(int? SiteID, bool isUpload = false)
        {
            RatingsViewModel ratingsViewModel = new RatingsViewModel { SiteID = SiteID, isUpload = isUpload };
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
                     sites = (from ste in UserRep.Context.SPFS_SITES
                                join uste in UserRep.Context.SPFS_USERSITES on ste.SiteID equals uste.SiteID
                                where uste.UserID == util.GetCurrentUser().UserID
                                select new SelectListItem { Value = ste.SiteID.ToString(), Text = ste.Name }).ToList();
                }
                

            }

            ViewBag.Months = util.GetMonths(false);
            ViewBag.Years = util.GetYears(false);
            ViewBag.Sites = sites;
        }

        //checks if there are any existing uploads 
        // displays warning if there are existing uploads in same month
        // Initializes partial view
        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "Search")]
        public ActionResult Search(RatingsViewModel ratingModel)
        {
            RatingsViewModel excelViewModel = new RatingsViewModel();
            var historicalRecords = new List<HistoricalRecordsCheck>();
            DateTime date = new DateTime(ratingModel.Year, ratingModel.Month, 01);
            Utilities util = new Utilities();
            using (Repository Rep = new Repository())
            {
                historicalRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                     where ratings.SiteID == ratingModel.SiteID && ratings.Initial_submission_date == date
                                     select new HistoricalRecordsCheck()
                                     {
                                         SiteID = ratings.SiteID,
                                         CID = ratings.CID,
                                         Initial_submission_date = ratings.Initial_submission_date
                                     }).ToList().Union
                                     (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
                                      where ratings.SiteID == ratingModel.SiteID && ratings.Initial_submission_date == date
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

            CreateListViewBags();
            return View("Index", ratingModel);
        }
    }
}