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
    public class RatingsController : BaseController
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
            RatingsViewModel ratingsViewModel = new RatingsViewModel { SiteID = SiteID, isUpload = isUpload, RatingRecords = new List<RatingRecord>() };
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;

            CreateListViewBags();
            ViewBag.Suppliers = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
            ViewBag.ShowResult = false;
            ViewBag.OldResults = false;
            ViewBag.EditMode = true;
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
                             where ste.SPFS_Active == true
                             select new SelectListItem { Value = ste.SiteID.ToString(), Text = ste.Name }).ToList();
                }
                else
                {
                    sites = (from ste in UserRep.Context.SPFS_SITES
                             join uste in UserRep.Context.SPFS_USERSITES on ste.SiteID equals uste.SiteID
                             where uste.UserID == userID && ste.SPFS_Active == true
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
            DateTime current = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // ratingModel.Month  = Convert.ToInt32(ratingModel.Month.ToString().PadLeft(2, '0'));
            if (current.AddMonths(-4) < date)
            {
                int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month.ToString().PadLeft(2, '0'));
                List<RatingRecord> StagingRecords = new List<RatingRecord>();
                List<RatingRecord> CurrentRecords = new List<RatingRecord>();
                List<RatingRecord> PreviousMonthRecords = new List<RatingRecord>();
                List<RatingRecord> PreviousMonthRecordsStaging = new List<RatingRecord>();
                Utilities util = new Utilities();
                using (Repository Rep = new Repository())
                {
                    CurrentRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                      where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                      select new RatingRecord
                                      {
                                          CID = ratings.CID,
                                          SiteID = ratings.SiteID,
                                          Inbound_parts = ratings.Inbound_parts,
                                          OTR = ratings.OTR,
                                          OTD = ratings.OTD,
                                          PFR = ratings.PFR
                                      }).ToList();


                    if (CurrentRecords.Count < 0)
                    {
                        StagingRecords = (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
                                          where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                          select new RatingRecord
                                          {
                                              CID = ratings.CID,
                                              SiteID = ratings.SiteID,
                                              Inbound_parts = ratings.Inbound_parts,
                                              OTR = ratings.OTR,
                                              OTD = ratings.OTD,
                                              PFR = ratings.PFR
                                          }).ToList();
                        if (StagingRecords.Count > 0)
                        {
                            //There are existing records submitted for this month
                            //display data from staging
                        }
                        else
                        {
                            if (current.AddMonths(-1) <= date)
                            {
                                int CheckingDate_Previous = Convert.ToInt32("" + date.Year + (date.Month-1).ToString().PadLeft(2, '0'));
                               
                                PreviousMonthRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                                        where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                                        select new RatingRecord
                                                        {
                                                            CID = ratings.CID,
                                                            SiteID = ratings.SiteID,
                                                            Inbound_parts = ratings.Inbound_parts,
                                                            OTR = ratings.OTR,
                                                            OTD = ratings.OTD,
                                                            PFR = ratings.PFR
                                                        }).ToList();
                                if(PreviousMonthRecords.Count > 0)
                                {
                                    //display current months grid
                                }
                                else
                                {
                                    PreviousMonthRecordsStaging = (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
                                                            where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                                            select new RatingRecord
                                                            {
                                                                CID = ratings.CID,
                                                                SiteID = ratings.SiteID,
                                                                Inbound_parts = ratings.Inbound_parts,
                                                                OTR = ratings.OTR,
                                                                OTD = ratings.OTD,
                                                                PFR = ratings.PFR
                                                            }).ToList();

                                    if(PreviousMonthRecordsStaging.Count>0)
                                    {
                                        //you havent submitted last months data. would you like to finish
                                        //Yes - Load last months data
                                        //No - Continue with current ratings
                                    }
                                    else
                                    {
                                        //display current months grid
                                    }

                                }
                            }
                            else
                            {
                                //display grid
                            }
                        }
                    }
                    else
                    {
                        //data exists for you and any changes will overwrite existing data. Press clear to stop editing submittedratings
                    }
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

                ViewBag.ShowResult = true;
                ViewBag.OldResults = false;
                ViewBag.EditMode = true;
                TempData["SearchedResults"] = ratingModel;

                return View("Index", ratingModel);
            }
            else
            {
                int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month.ToString().PadLeft(2, '0'));
                List<RatingRecord> OldRecords = new List<RatingRecord>();
                Utilities util = new Utilities();
                using (Repository Rep = new Repository())
                {
                    OldRecords = (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
                                  where ratings.SiteID == ratingModel.SiteID && ratings.Rating_period == CheckingDate
                                  select new RatingRecord()
                                  {
                                      CID = ratings.CID,
                                      SiteID = ratings.SiteID,
                                      Inbound_parts = ratings.Inbound_parts,
                                      OTR = ratings.OTR,
                                      OTD = ratings.OTD,
                                      PFR = ratings.PFR

                                  }).ToList();

                }
                if (OldRecords.Count > 0)
                {
                    List<RatingRecord> OldRecordsUpdated = new List<RatingRecord>();
                    foreach (RatingRecord rec in OldRecords)
                    {
                        rec.DUNS = GetDUNSfromCID(rec.CID);
                        rec.SupplierName = selectSuppliers.Where(r => r.Value == rec.CID.ToString()).First().Text;
                        OldRecordsUpdated.Add(rec);
                    }
                    ratingModel.RatingRecords = OldRecordsUpdated;

                    CreateListViewBags();
                    // ViewBag.Suppliers = selectSuppliers;
                    RatingsViewModel UpdatedModel = Merge(ratingModel);

                    var rateSuppliers = UpdatedModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
                    var modifiedlist = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
                    ViewBag.RatingSuppliers = rateSuppliers;
                    ViewBag.ShowResult = true;
                    ViewBag.OldResults = true;
                    ViewBag.EditMode = false;
                    return View("Index", UpdatedModel);
                }
                else
                {

                    ViewBag.ShowResult = false;
                    ViewBag.EditMode = false;
                    CreateListViewBags();
                    return View("Index", ratingModel);
                }

            }
        }


        private string GetDUNSfromCID(int CID)
        {
            string DUNS = string.Empty;
            using (Repository repository = new Repository())
            {

                var result = from sup in repository.Context.SPFS_SUPPLIERS
                             where sup.CID == CID
                             select sup.Duns;

                DUNS = Convert.ToString(result.FirstOrDefault());

            }
            return DUNS.Replace("\0", "").Trim();
        }
        private RatingsViewModel Merge(RatingsViewModel RatingModel)
        {
            RatingsViewModel RateModel = new RatingsViewModel();

            List<RatingRecord> ISORecords = IncidentSpendOrder(RatingModel);
            List<RatingRecord> MergedRecords = new List<RatingRecord>();
            //List<RatingRecord> UnMatchedRecords = new List<RatingRecord>();

            var query = from x in ISORecords
                        join y in RatingModel.RatingRecords
                        on x.CID equals y.CID
                        select new { x, y };

            foreach (var match in query)
            {
                match.x.Inbound_parts = match.y.Inbound_parts;
                match.x.OTD = match.y.OTD;
                match.x.OTR = match.y.OTR;
                match.x.PFR = match.y.PFR;
                match.x.Temp_Upload_ = match.y.Temp_Upload_;
                match.x.ErrorInformation = match.y.ErrorInformation;


            }

            //  MergedRecords = ISORecords;
            //var unmatch = (from agrr in RatingModel.RatingRecords
            //               where !(ISORecords.Any(i => i.CID == agrr.CID))
            //               select agrr).ToList();
            //if (unmatch != null)
            //{
            //    ISORecords.AddRange(unmatch);
            //}

            MergedRecords = ISORecords;
            RateModel.RatingRecords = MergedRecords;
            RateModel.isUpload = false;
            RateModel.Month = RatingModel.Month;
            RateModel.Year = RatingModel.Year;
            RateModel.SiteID = RatingModel.SiteID;
            SelectSiteGDIS gdis = selectGDIS.Where(g => g.SiteID.Equals(RatingModel.SiteID)).FirstOrDefault();

            RateModel.SiteName = gdis.Name;

            return RateModel;
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
                                    Gdis_org_entity_ID = site.Gdis_org_entity_ID,
                                    Gdis_org_Parent_ID = site.Gdis_org_Parent_ID,
                                    Reject_incident_count = spend.Reject_incident_count,
                                    Reject_parts_count = spend.Reject_parts_count,
                                    SupplierName = sup.Name,
                                    DUNS = sup.Duns

                                }).ToList();

                var parentID = recordsChild.Max(p => p.Gdis_org_Parent_ID);


                recordsParent = (from spend in Rep.Context.SPFS_SPEND_SUPPLIERS
                                 where spend.Gdis_org_Parent_ID == parentID
                                 group spend by new { spend.CID, spend.Gdis_org_Parent_ID } into g
                                 select new RatingRecord
                                 {
                                     CID = g.Key.CID,
                                     Gdis_org_Parent_ID = g.Key.Gdis_org_Parent_ID,
                                     Total_Spend = g.Sum(x => x.Total_Spend)


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

            Sortedrecords = Mergedrecords.OrderByDescending(x => x.Reject_incident_count).ThenByDescending(x => x.Total_Spend).ToList();

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
                               Gdis_org_entity_ID = gdis.Gdis_org_entity_ID,
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