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
        private List<SelectListItem> selectSuppliers;

        private List<SelectSiteGDIS> selectGDIS;
        public RatingsController()
        {
            CacheObjects obj = new CacheObjects();

            selectGDIS = obj.GetSites;
            selectSuppliers = obj.GetSuppliers;
        }

       
        // GET: Ratings
        public ActionResult Index(int? SiteID, bool isUpload = false)
        {
            RatingsViewModel ratingsViewModel = new RatingsViewModel { SiteID = SiteID, isUpload = isUpload , RatingRecords = new List<RatingRecord>()};
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;

            CreateListViewBags();
            ViewBag.Suppliers = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList(); 
            ViewBag.ShowResult = false;
            return View(ratingsViewModel);
        }

       
        private void CreateListViewBags()
        {
            Utilities util = new Utilities();
            int userID = util.GetCurrentUser().UserID;
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
                                where uste.UserID == userID
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
            // DateTime date = new DateTime(ratingModel.Year, ratingModel.Month, 01);
            int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month);

            Utilities util = new Utilities();
            using (Repository Rep = new Repository())
            {
                historicalRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                     where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                     select new HistoricalRecordsCheck()
                                     {
                                         SiteID = ratings.SiteID,
                                         CID = ratings.CID,
                                         Initial_submission_date = ratings.Initial_submission_date
                                     }).ToList().Union
                                     (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
                                      where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                      select new HistoricalRecordsCheck()
                                      {
                                          SiteID = ratings.SiteID,
                                          CID = ratings.CID,
                                          Initial_submission_date = ratings.Initial_submission_date
                                      }).ToList();

            }
            if (historicalRecords.Count > 0)
            {
                util.GetDivElements("There are existing records submitted for this month", "alert alert-warning", "Warning ! ");
            }

            CreateListViewBags();
           // ViewBag.Suppliers = selectSuppliers;
            ratingModel.RatingRecords = IncidentSpendOrder(ratingModel);

            var rateSuppliers = ratingModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
            var modifiedlist = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
            ViewBag.RatingSuppliers = rateSuppliers;
            var NotinListSuppliers = (from fulllist in modifiedlist
                                      where !(rateSuppliers.Any(i => i.Value == fulllist.Value))
                                      select fulllist).ToList();
            if (NotinListSuppliers != null)
            {
                ViewBag.Suppliers = NotinListSuppliers;
            }
            else
            {
                ViewBag.Suppliers = modifiedlist;
            }
            //if(ratingModel.RatingRecords.Count > 0)
            //{
            //    ViewBag.NewSite = false;
            //}
            //else
            //{
            //    ViewBag.NewSite = true;
            //}
            ViewBag.ShowResult = true;
            TempData["SearchedResults"] = ratingModel;

            return View("Index", ratingModel);
        }

        public List<RatingRecord> IncidentSpendOrder(RatingsViewModel RatingModel)
        {
            List<RatingRecord> recordsChild = new List<RatingRecord>();
            List<RatingRecord> recordsParent = new List<RatingRecord>();
            List<RatingRecord> Mergedrecords = new List<RatingRecord>();
            List<RatingRecord> Sortedrecords = new List<RatingRecord>();
            using (Repository Rep = new Repository())
            {
                recordsChild = (from site in Rep.Context.SPFS_SITES
                                join spend in Rep.Context.SPFS_SPEND_SUPPLIERS on site.SiteID equals spend.SiteID
                                join sup in Rep.Context.SPFS_SUPPLIERS on spend.CID equals sup.CID
                                where spend.SiteID == RatingModel.SiteID
                           select new RatingRecord
                           {
                               CID = spend.CID,
                               SiteID = spend.SiteID,
                               Gdis_org_entity_ID =site.Gdis_org_entity_ID,
                               Gdis_org_Parent_ID =site.Gdis_org_Parent_ID,
                               Reject_incident_count =spend.Reject_incident_count,
                               Reject_parts_count =spend.Reject_parts_count,
                               SupplierName = sup.Name,
                               DUNS = sup.Duns

                           }).ToList();

                var parentID = recordsChild.Max(p => p.Gdis_org_Parent_ID);


                recordsParent = (from spend in Rep.Context.SPFS_SPEND_SUPPLIERS
                                 where spend.Gdis_org_Parent_ID == parentID
                                 group spend by new {spend.CID,spend.Gdis_org_Parent_ID} into g
                                 select new RatingRecord
                                 {
                                     CID = g.Key.CID,
                                     Gdis_org_Parent_ID = g.Key.Gdis_org_Parent_ID,
                                     Total_Spend = g.Sum(x=>x.Total_Spend)
                                   

                                 }).ToList();

            }
            Mergedrecords = (from child in recordsChild
                             join parent in recordsParent on
                             new { child.CID, child.Gdis_org_Parent_ID } equals
                             new { parent.CID, parent.Gdis_org_Parent_ID } into merged
                             from m in merged.DefaultIfEmpty()
                             select new RatingRecord
                             {
                                 CID = child.CID,
                                 DUNS = child.DUNS,
                                 SiteID = child.SiteID,
                                 Gdis_org_entity_ID = child.Gdis_org_entity_ID,
                                 Gdis_org_Parent_ID = child.Gdis_org_Parent_ID,
                                 Reject_incident_count = child.Reject_incident_count,
                                 Reject_parts_count = child.Reject_parts_count,
                                 Total_Spend = m == null ? 0 : m.Total_Spend,
                                 SupplierName = child.SupplierName

                             }).ToList();

             Sortedrecords =Mergedrecords.OrderByDescending(x => x.Reject_incident_count).ThenByDescending(x => x.Total_Spend).ToList();

            return Sortedrecords;
           
        }

        //public ActionResult AddRowReload(int CID,RatingsViewModel RatingModel )
        //{
        //    RatingRecord NewRec = GetSupplierDataByCID(CID, RatingModel);
        //    RatingModel.RatingRecords.Add(NewRec);


        //    CreateListViewBags();
        //    ViewBag.Suppliers = selectSuppliers;
        //    return View("Index", RatingModel);
        //}
        //public ActionResult AddRowReload(int CID, int SiteID, int count)
        public ActionResult AddRowReload(int CID)
        {
                       

            //RatingsViewModel RatingModel = new RatingsViewModel();

            RatingsViewModel RatingModel = (RatingsViewModel)TempData["SearchedResults"];

            RatingRecord NewRec = GetSupplierDataByCID(CID, RatingModel.SiteID.Value);
            RatingModel.RatingRecords.Add(NewRec);

            TempData["SearchedResults"] = RatingModel;
            //List<RatingRecord> Records = new List<RatingRecord>();

            //ViewBag.newIndex = count;
            //for(int i =0;i<count; i++)
            //{
            //    RatingRecord empRec = new RatingRecord();
            //    empRec.CID = 0;
            //    Records.Add(empRec);

            //}

            //Records.Add(NewRec);

            //RatingModel.RatingRecords = Records;

            //return PartialView("_AppendRow", RatingModel);
            var rateSuppliers = RatingModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
            ViewBag.RatingSuppliers = rateSuppliers;
            
            return PartialView("_SupplierRatings", RatingModel);
        }


        private RatingRecord GetSupplierDataByCID(int CID, int SiteID)
        {
            RatingRecord Rec = new RatingRecord();
            SelectSiteGDIS gdis = selectGDIS.Where(g => g.SiteID.Equals(SiteID)).FirstOrDefault();
            using (Repository Rep = new Repository())
            {
               Rec = (from site in Rep.Context.SPFS_SITES
                            join spend in Rep.Context.SPFS_SPEND_SUPPLIERS on site.SiteID equals spend.SiteID
                            join sup in Rep.Context.SPFS_SUPPLIERS on spend.CID equals sup.CID
                            where spend.SiteID == SiteID && spend.CID == CID
                            select new RatingRecord
                            {
                                CID = spend.CID,
                                SiteID = spend.SiteID,
                                Gdis_org_entity_ID = site.Gdis_org_entity_ID,
                                Gdis_org_Parent_ID = site.Gdis_org_Parent_ID,
                                Reject_incident_count = spend.Reject_incident_count,
                                Reject_parts_count = spend.Reject_parts_count,
                                SupplierName = sup.Name,
                                DUNS = sup.Duns

                            }).FirstOrDefault();


                if (Rec == null)
                {
                     Rec = (from sup in Rep.Context.SPFS_SUPPLIERS
                                  where sup.CID == CID
                                  select new RatingRecord
                                  {
                                      CID = sup.CID,
                                      SiteID = SiteID,
                                      Gdis_org_entity_ID =gdis.Gdis_org_entity_ID,
                                      Gdis_org_Parent_ID = gdis.Gdis_org_Parent_ID,
                                      Reject_incident_count = 0,
                                      Reject_parts_count = 0,
                                      SupplierName = sup.Name,
                                      DUNS = sup.Duns

                                  }).FirstOrDefault();
                }
            }
            return Rec;
        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "SaveData")]
        public ActionResult SaveData(RatingsViewModel ratingModel)
        {



            return View("Index", ratingModel);

        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "SubmitData")]
        public ActionResult SubmitData(RatingsViewModel ratingModel)
        {



            return View("Index", ratingModel);

        }
    }
}