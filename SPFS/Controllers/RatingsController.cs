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

        private List<SelectListItem> selectSupplierscid;

        private List<SelectSiteGDIS> selectGDIS;
        public RatingsController()
        {
            CacheObjects obj = new CacheObjects();

            selectGDIS = obj.GetSites;
            selectSuppliers = obj.GetSuppliers;
            selectSupplierscid = obj.GetSuppliersCID;
        }


        // GET: Ratings
        public ActionResult Index(int? SiteID, bool isUpload = false)
        {
            RatingsViewModel ratingsViewModel = new RatingsViewModel { SiteID = SiteID, isUpload = isUpload, RatingRecords = new List<RatingRecord>() };
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;

            CreateListViewBags();
            ViewBag.Suppliers = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
            ratingsViewModel.ShowResult = false;
            ratingsViewModel.OldResults = false;
            ratingsViewModel.EditMode = true;
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
            DateTime current = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 01);
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
                    CurrentRecords = ProdRecords(ratingModel, CheckingDate, Rep);

                    int CurrentCount = CurrentRecords != null ? CurrentRecords.Count : 0;
                    if (CurrentCount <= 0)
                    {
                        StagingRecords = StageRecords(ratingModel, CheckingDate, Rep);

                        if (StagingRecords.Count > 0)
                        {
                            ratingModel.IsStagingRatings = true;
                            ratingModel.RatingRecords = StagingRecords;
                            ViewBag.divmsg = "Data exists for this rating period that has <strong>Not</strong> been <strong>Submitted</strong>";
                            //There are existing records submitted for this month
                            //display data from staging
                            return LoadData(ratingModel, ratingModel.IsStagingRatings);
                        }
                        else
                        {
                            if (current.AddMonths(-1) <= date)
                            {
                                int CheckingDate_Previous = Convert.ToInt32("" + date.Year + (date.Month - 1).ToString().PadLeft(2, '0'));

                                PreviousMonthRecords = ProdRecords(ratingModel, CheckingDate_Previous, Rep);

                                if (PreviousMonthRecords.Count > 0)
                                {
                                    //display current months grid
                                    ratingModel.IsPreviousRatings = true;
                                    ratingModel.RatingRecords = CurrentRecords;
                                    ViewBag.divmsg = "Last month data check was succesfull";
                                    return LoadData(ratingModel, false);
                                }
                                else
                                {
                                    PreviousMonthRecordsStaging = StageRecords(ratingModel, CheckingDate_Previous, Rep);

                                    if (PreviousMonthRecordsStaging.Count > 0)
                                    {
                                        //you havent submitted last months data. would you like to finish
                                        //Yes - Load last months data
                                        //No - Continue with current ratings
                                        ratingModel.IsAlert = true;
                                        ratingModel.IsPreviousStagingRatings = true;
                                        ViewBag.alertmsg = "Data exists for last ratings period which has <strong>Not</strong> been <strong>Submitted!</strong>";
                                        ratingModel.ShowResult = false;
                                        ratingModel.EditMode = false;
                                        CreateListViewBags();
                                        return View("Index", ratingModel);
                                    }
                                    else
                                    {
                                        //display current months grid
                                        ratingModel.RatingRecords = CurrentRecords;
                                        ViewBag.divmsg = "This is not current period";
                                        return LoadData(ratingModel, false);
                                    }

                                }
                            }
                            else
                            {
                                //display grid
                                ratingModel.RatingRecords = CurrentRecords;
                                ViewBag.divmsg = "This is not current period";
                                return LoadData(ratingModel, false);
                            }
                        }
                    }
                    else
                    {
                        //data exists for you and any changes will overwrite existing data. Press clear to stop editing submittedratings
                        ratingModel.IsAlert = true;
                        ratingModel.IsCurrentRatings = true;
                        ViewBag.alertmsg = "Data exists and has been <strong>Submitted</strong> for this rating period. Do you wish to <strong>Edit</strong> these previously <strong>Submitted</strong> ratings?";
                        ratingModel.ShowResult = false;
                        ratingModel.EditMode = false;
                        CreateListViewBags();
                        return View("Index", ratingModel);
                    }
                }


            }
            else
            {
                int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month.ToString().PadLeft(2, '0'));
                List<RatingRecord> OldRecords = new List<RatingRecord>();
                using (Repository Rep = new Repository())
                {
                    OldRecords = ProdRecords(ratingModel, CheckingDate, Rep);

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
                    ratingModel.ShowResult = true;
                    ratingModel.OldResults = true;
                    ratingModel.EditMode = false;
                    return View("Index", UpdatedModel);
                }
                else
                {

                    ratingModel.ShowResult = false;
                    ratingModel.EditMode = false;
                    CreateListViewBags();
                    return View("Index", ratingModel);
                }

            }
        }

        private ActionResult LoadData(RatingsViewModel ratingModel, bool dataExists)
        {
            CreateListViewBags();
            RatingsViewModel UpdatedModel = new RatingsViewModel();
            // ViewBag.Suppliers = selectSuppliers;  ratingModel.RatingRecords = IncidentSpendOrder(ratingModel);
            if (dataExists)
            {
                UpdatedModel = Merge(ratingModel);

                var rateSuppliers = UpdatedModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
                LoadDropdowns(rateSuppliers);
            }
            else
            {

                UpdatedModel.RatingRecords = IncidentSpendOrder(ratingModel);
                UpdatedModel.isUpload = false;
                UpdatedModel.Month = ratingModel.Month;
                UpdatedModel.Year = ratingModel.Year;
                UpdatedModel.SiteID = ratingModel.SiteID;
                SelectSiteGDIS gdis = selectGDIS.Where(g => g.SiteID.Equals(ratingModel.SiteID)).FirstOrDefault();

                UpdatedModel.SiteName = gdis.Name;
                var rateSuppliers = ratingModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
                LoadDropdowns(rateSuppliers);
            }



            UpdatedModel.ShowResult = true;
            UpdatedModel.OldResults = false;
            UpdatedModel.EditMode = true;
            TempData["SearchedResults"] = UpdatedModel;

            return View("Index", UpdatedModel);
        }
        //public ActionResult LoadAlertData(RatingsViewModel RatingModel, bool isStaging ,bool isLastmonth)
        public ActionResult LoadAlertData(int SiteID, int Year, int Month, bool isStaging, bool isLastmonth)
        {
            RatingsViewModel RatingModel = new RatingsViewModel() { SiteID = SiteID, Year = Year, Month = Month };

            using (Repository Rep = new Repository())
            {
                int CheckingDate;
                if (isStaging)
                {

                    if (isLastmonth)
                    {
                        CheckingDate = Convert.ToInt32("" + RatingModel.Year + (RatingModel.Month - 1).ToString().PadLeft(2, '0'));
                    }
                    else
                    {
                        CheckingDate = Convert.ToInt32("" + RatingModel.Year + (RatingModel.Month).ToString().PadLeft(2, '0'));
                    }
                    RatingModel.RatingRecords = StageRecords(RatingModel, CheckingDate, Rep);
                }
                else
                {
                    CheckingDate = Convert.ToInt32("" + RatingModel.Year + (RatingModel.Month).ToString().PadLeft(2, '0'));
                    RatingModel.RatingRecords = ProdRecords(RatingModel, CheckingDate, Rep);
                }
            }
            CreateListViewBags();
            // ViewBag.Suppliers = selectSuppliers;  ratingModel.RatingRecords = IncidentSpendOrder(ratingModel);

            RatingsViewModel UpdatedModel = Merge(RatingModel);

            var rateSuppliers = UpdatedModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
            LoadDropdowns(rateSuppliers);

            UpdatedModel.ShowResult = true;
            UpdatedModel.OldResults = false;
            UpdatedModel.EditMode = true;
            TempData["SearchedResults"] = UpdatedModel;

            return View("Index", UpdatedModel);
        }

        private void LoadDropdowns(List<SelectListItem> rateSuppliers)
        {
            //var modifiedlist = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
            var modifiedlist = selectSupplierscid;
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
        }

        private static List<RatingRecord> StageRecords(RatingsViewModel ratingModel, int CheckingDate, Repository Rep)
        {
            return (from ratings in Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS
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
        }

        private static List<RatingRecord> ProdRecords(RatingsViewModel ratingModel, int CheckingDate, Repository Rep)
        {
            return (from ratings in Rep.Context.SPFS_SUPPLIER_RATINGS
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
            foreach (RatingRecord rec in MergedRecords)
            {
                if (rec.Inbound_parts != 0)
                {
                    double ppm = (double)rec.Reject_parts_count / rec.Inbound_parts;
                    rec.PPM = Math.Round(ppm * 1000000);
                    double ipm = (double)rec.Reject_incident_count / rec.Inbound_parts;
                    rec.IPM = Math.Round((ipm * 1000000), 2);
                }
                else
                {
                    rec.PPM = 0;
                    rec.IPM = 0.00;
                }
                if (rec.OTD != 0)
                {
                    double pct = (double)rec.OTR / rec.OTD;
                    rec.PCT = Math.Round(pct * 100);
                }
                else
                {
                    rec.PCT = 0;
                }
            }
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
            //TempData["SearchedResults"]

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

        public JsonResult UpdateRating(int CID, int Inbound, int OTR, int OTD, int PFR, double PPM, double IPM, double PCT)
        {

            //RatingsViewModel RatingModel = new RatingsViewModel();

            RatingsViewModel RatingModel = (RatingsViewModel)TempData["SearchedResults"];

            RatingRecord Rec = RatingModel.RatingRecords.Where(r => r.CID.Equals(CID)).Select(r =>
            {
                r.Inbound_parts = Inbound;
                r.IPM = IPM; r.OTD = OTD; r.OTR = OTR; r.PCT = PCT; r.PFR = PFR; r.PPM = PPM; return r;
            }).FirstOrDefault();


            TempData["SearchedResults"] = RatingModel;


            return Json(true, JsonRequestBehavior.AllowGet);
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
            if (true)
            {

                ModelState.AddModelError("", "error occured");
            }

            if (ModelState.IsValid)
            {
            }



            return View("Index", ratingModel);

        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "SubmitData")]
        public ActionResult SubmitData(RatingsViewModel ratingModel)
        {



            return View("Index", ratingModel);

        }


        public ActionResult LoadSuppliers()
        {
            RatingsViewModel RatingModel = (RatingsViewModel)TempData["SearchedResults"];
            TempData.Keep("SearchedResults");
            if (RatingModel != null)
            {
                var rateSuppliers = RatingModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
                //var modifiedlist = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
                var modifiedlist = selectSupplierscid;
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
            }
            else
            {
                var modifiedlist = selectSupplierscid;
                ViewBag.Suppliers = modifiedlist;

            }
            return PartialView("_AddSupplier");
        }

        public JsonResult GetSupplierbyName(string nameString)
        {
            RatingsViewModel RatingModel = (RatingsViewModel)TempData["SearchedResults"];
            TempData.Keep("SearchedResults");
            List<SelectListItem> SupplierList;
            if (RatingModel != null)
            {
                var rateSuppliers = RatingModel.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
                var modifiedlist = selectSupplierscid;
                var NotinListSuppliers = (from fulllist in modifiedlist
                                          where !(rateSuppliers.Any(i => i.Value == fulllist.Value))
                                          select fulllist).ToList();


                if (NotinListSuppliers != null)
                {
                    SupplierList = NotinListSuppliers;
                }
                else
                {
                    SupplierList = modifiedlist;
                }
            }
            else
            {
                var modifiedlist = selectSupplierscid;
                SupplierList = modifiedlist;
            }



            var newSuppliercache = string.IsNullOrWhiteSpace(nameString) ? SupplierList :
            SupplierList.Where(s => s.Text.StartsWith(nameString, StringComparison.InvariantCultureIgnoreCase));
            return Json(newSuppliercache, JsonRequestBehavior.AllowGet);
        }
    }
}