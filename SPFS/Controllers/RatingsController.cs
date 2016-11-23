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

            ViewBag.Months = util.GetMonths();
            ViewBag.Years = util.GetYears();
            ViewBag.Sites = sites;
        }

        
    }
}