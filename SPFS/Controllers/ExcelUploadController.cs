using Excel;
using OfficeOpenXml;
using PagedList;
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
        private List<SupplierCacheViewModel> supplierCacheObj;

        private List<SelectListItem> selectSupplierscid;

        private List<SelectListItem> selectSuppliers;

        private List<SelectSiteGDIS> selectGDIS;


        public ExcelUploadController()
        {
            CacheObjects obj = new CacheObjects();

            selectGDIS = obj.GetSites;
            selectSuppliers = obj.GetSuppliers;
            supplierCacheObj = obj.GetSuppliersCache;
            selectSupplierscid = obj.GetSuppliersCID;


        }

        //private List<SupplierCacheViewModel> GetSupplierListData()
        //{
        //    List<SupplierCacheViewModel> result = new List<SupplierCacheViewModel>();

        //    using (Repository repository = new Repository())
        //    {
        //        var MultipleLeftJoin = from sup in repository.Context.SPFS_SUPPLIERS
        //                               select new SupplierCacheViewModel
        //                               {
        //                                   CID = sup.CID,
        //                                   Duns = sup.Duns, //.Replace("\0", "").Trim(),
        //                                   Name =sup.Name
        //                               };

        //        result = MultipleLeftJoin.ToList();
        //    }
        //    result.ForEach(z => z.Duns = z.Duns.Replace("\0", "").Trim());
        //    return result;
        //}

        //GET: ExcelUpload
        public ActionResult Index(int? SiteID, bool isUpload = true)
        {
            ExcelRatingsViewModel ratingsViewModel = new ExcelRatingsViewModel { SiteID = SiteID, isUpload = isUpload };
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;
            ratingsViewModel.ShowResult = false;
            ratingsViewModel.EditMode = true;
            CreateListViewBags();

            return View(ratingsViewModel);
        }

        public ActionResult LoadUploadIndex(int SiteID, int Year, int Month)
        {
            ExcelRatingsViewModel ratingsViewModel = new ExcelRatingsViewModel { SiteID = SiteID, isUpload = true };
            ratingsViewModel.Month = Month;
            ratingsViewModel.Year = Year;
            ratingsViewModel.ShowResult = true;
            ratingsViewModel.EditMode = false;
            CreateListViewBags();

            return View("Index", ratingsViewModel);
        }
        //public ActionResult Index(ExcelRatingsViewModel exratingsViewModel)
        //{
        //    ExcelRatingsViewModel ratingsViewModel = new ExcelRatingsViewModel();
        //    if (!exratingsViewModel.SiteID.HasValue)
        //    {               
        //        ratingsViewModel.Month = DateTime.Now.Month - 1;
        //        ratingsViewModel.Year = DateTime.Now.Year;
        //    }
        //    else
        //    {
        //        ratingsViewModel.SiteID = exratingsViewModel.SiteID;
        //        ratingsViewModel.Month = exratingsViewModel.Month;
        //        ratingsViewModel.Year = exratingsViewModel.Year;

        //    }
        //    ratingsViewModel.isUpload = true;
        //    CreateListViewBags();
        //    return View(ratingsViewModel);
        //}
        private void CreateListViewBags()
        {
            Utilities util = new Utilities();
            List<SelectListItem> sites;
            int userID = util.GetCurrentUser().UserID;
            //List<SelectListItem> suppliers;
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

                //suppliers = (from supplier in UserRep.Context.SPFS_SUPPLIERS
                //         select new SelectListItem { Value = supplier.CID.ToString(), Text = supplier.Name}).ToList();
            }

            ViewBag.Months = util.GetMonths(true);
            ViewBag.Years = util.GetYears(true);
            ViewBag.Sites = sites;
            // ViewBag.Suppliers = suppliers;
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
                    ViewBag.Suppliers = selectSuppliers;
                    var count = 0;
                    if (ratingModel.RatingRecords.Count > 0)
                    {
                        foreach (var record in ratingModel.RatingRecords)
                        {
                            if ((record.ErrorInformation != null ? record.ErrorInformation.Count : 0) > 1)
                            {
                                count++;
                            }
                        }
                        if (count > 0)
                        {
                            ViewBag.Count = count;
                            //ViewBag.ShowMerge = false;
                        }
                        else
                        {
                            ViewBag.Count = count;
                            // ViewBag.ShowMerge = true;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("UploadFile", "Please upload Valid File");
                        CreateListViewBags();
                        return View("Index", ratingModel);
                    }
                }
                else
                {
                    ModelState.AddModelError("UploadFile", "Please Upload Your file");
                    CreateListViewBags();
                    return View("Index", ratingModel);
                }
            }

            return View("ExcelReview", ratingModel);

        }

        private ExcelRatingsViewModel ProcessExcelDataintoViewModel(ExcelRatingsViewModel ratingModel, DataSet result)
        {
            List<RatingRecord> ratings = result.Tables[0].ToList<RatingRecord>();
            List<RatingRecord> Inboundratings = CheckInboundRatings(ratings, ratingModel);
            List<RatingRecord> PrimaryKeyratings = new List<RatingRecord>();
            foreach (var item in Inboundratings)
            {
                if (!string.IsNullOrEmpty(item.DUNS))
                {
                    item.DUNS = item.DUNS.PadLeft(9, '0');
                }
                List<ErrorDetails> ErrorInfo = new List<ErrorDetails>();
                bool iRecordfound = false;
                if (supplierCacheObj.Any(m => m.ERPSupplierID == item.ERP_Supplier_ID && m.Gdis_org_entity_ID.Equals(item.Gdis_org_entity_ID)))  //supplierCacheObj.Any(m => m.CID.Equals(item.CID))
                {
                    if (supplierCacheObj.Any(m => m.CID.Equals(item.CID)))
                    {
                        iRecordfound = true;
                        if (string.IsNullOrWhiteSpace(item.DUNS))
                        {
                            item.DUNS = GetDUNSfromCID(item.CID);
                        }
                    }
                    else if (supplierCacheObj.Any(m => m.Duns.Equals(item.DUNS)))
                    {

                        item.CID = GetCIDfromDuns(item.DUNS);
                        if (item.CID == 0)
                        {
                            iRecordfound = false;
                            GetErrors(item, ErrorInfo);
                        }
                    }
                    else
                    {
                        iRecordfound = false;
                        GetErrors(item, ErrorInfo);
                    }


                }
                else if (supplierCacheObj.Any(m => m.CID.Equals(item.CID)))
                {
                    iRecordfound = true;
                    if (string.IsNullOrWhiteSpace(item.DUNS))
                    {
                        item.DUNS = GetDUNSfromCID(item.CID);
                    }
                }
                else if (supplierCacheObj.Any(m => m.Duns.Equals(item.DUNS)))
                {
                    iRecordfound = true;
                    item.CID = GetCIDfromDuns(item.DUNS);
                    if (item.CID == 0)
                    {
                        iRecordfound = false;
                        GetErrors(item, ErrorInfo);
                    }
                }
                else
                {
                    iRecordfound = false;
                    GetErrors(item, ErrorInfo);
                }

                PrimaryKeyratings.Add(item);
            }

            //ratingModel.RatingRecords = PrimaryKeyratings;
            ratingModel.RatingRecords = PrimaryKeyratings.OrderByDescending(o => o.ErrorInformation != null ? o.ErrorInformation.Count : 0).ToList();
            TempData["RatingRecords"] = ratingModel.RatingRecords;
            return ratingModel;

        }

        private static void GetErrors(RatingRecord item, List<ErrorDetails> ErrorInfo)
        {
            string msgSupplierName = string.Empty;
            string msgErp = string.Empty;
            string msgCid = string.Empty;
            string msgDuns = string.Empty;
            msgSupplierName = string.Format("ERPSupplierID={0} ,CID={1} and Duns={2} are not matching", new string[] {Convert.ToString(item.ERP_Supplier_ID),
                                Convert.ToString(item.CID),Convert.ToString(item.DUNS)});
            ErrorInfo.Add(new ErrorDetails { Key = Convert.ToString(item.SupplierName), ErrorMessage = msgSupplierName });

            msgErp = string.Format("ERPSupplierID={0} is not valid", new string[] { Convert.ToString(item.ERP_Supplier_ID) });
            ErrorInfo.Add(new ErrorDetails { Key = Convert.ToString(item.ERP_Supplier_ID), ErrorMessage = msgErp });

            msgCid = string.Format("CID={0} is not valid", new string[] { Convert.ToString(item.CID) });
            ErrorInfo.Add(new ErrorDetails { Key = Convert.ToString(item.CID), ErrorMessage = msgCid });

            msgDuns = string.Format("Duns={0} is not valid", new string[] { Convert.ToString(item.DUNS) });
            ErrorInfo.Add(new ErrorDetails { Key = Convert.ToString(item.DUNS), ErrorMessage = msgDuns });

            item.ErrorInformation = ErrorInfo;
        }

        private int GetCIDfromDuns(string DUNS)
        {
            int CID = 0;
            using (Repository repository = new Repository())
            {

                var result = from sup in repository.Context.SPFS_SUPPLIERS
                             where sup.Duns == DUNS
                             select sup.CID;

                CID = Convert.ToInt32(result.FirstOrDefault());

            }
            return CID;
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
        private List<RatingRecord> CheckInboundRatings(List<RatingRecord> ratngs, ExcelRatingsViewModel ratingsModel)
        {
            List<RatingRecord> ratings = new List<RatingRecord>();
            foreach (var item in ratngs)
            {
                if (item.Inbound_parts > 0)
                {
                    SelectSiteGDIS gdis = selectGDIS.Where(g => g.SiteID.Equals(ratingsModel.SiteID)).FirstOrDefault();
                    //RatingRecord ratingRecord = new RatingRecord();
                    // ratingRecord.CID = int.TryParse(item.CID,)
                    item.Gdis_org_entity_ID = gdis.Gdis_org_entity_ID;
                    item.Gdis_org_Parent_ID = gdis.Gdis_org_Parent_ID;

                    ratings.Add(item);
                }

            }


            return ratings;
        }

        //checks if there are any existing uploads 
        // displays warning if there are existing uploads in same month
        // Initializes partial view
        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "Search")]
        public ActionResult Search(ExcelRatingsViewModel ratingModel)
        {
            DateTime date = new DateTime(ratingModel.Year, ratingModel.Month, 01);
            DateTime current = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 01);

            if (current.AddMonths(-4) < date)
            {
                int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month.ToString().PadLeft(2, '0'));
                List<RatingRecord> StagingRecords = new List<RatingRecord>();
                List<RatingRecord> CurrentRecords = new List<RatingRecord>();
                List<RatingRecord> PreviousMonthRecords = new List<RatingRecord>();
                List<RatingRecord> PreviousMonthRecordsStaging = new List<RatingRecord>();

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
                            ratingModel.IsAlert = true;
                            ViewBag.alertmsg = "Data exists for this rating period that has <strong>Not</strong> been <strong>Submitted</strong> Do you wish to <strong>Upload</strong> again";
                            //There are existing records submitted for this month
                            //display data from staging
                            CreateListViewBags();
                            ratingModel.ShowResult = false;
                            ratingModel.EditMode = false;
                            TempData["SearchedResults"] = ratingModel;
                            return View("Index", ratingModel);
                        }
                        else
                        {
                            if (current.AddMonths(-1) <= date)
                            {
                                int CheckingDate_Previous = Convert.ToInt32("" + date.Year + (date.Month - 1).ToString().PadLeft(2, '0'));

                                PreviousMonthRecords = ProdRecords(ratingModel, CheckingDate_Previous, Rep);

                                if (PreviousMonthRecords.Count > 0)
                                {
                                    //Allow Upload
                                    ratingModel.IsPreviousRatings = true;
                                    ratingModel.ShowResult = true;
                                    ratingModel.EditMode = true;
                                    ViewBag.divmsg = "Last month data check was succesfull";
                                    CreateListViewBags();
                                    TempData["SearchedResults"] = ratingModel;
                                    return View("Index", ratingModel);
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
                                        CreateListViewBags();
                                        ratingModel.ShowResult = false;
                                        ratingModel.EditMode = false;
                                        TempData["SearchedResults"] = ratingModel;
                                        return View("Index", ratingModel);
                                    }
                                    else
                                    {
                                        //Allow Upload
                                        ratingModel.ShowResult = true;
                                        ratingModel.EditMode = true;
                                        CreateListViewBags();
                                        TempData["SearchedResults"] = ratingModel;
                                        return View("Index", ratingModel);
                                    }

                                }
                            }
                            else
                            {
                                //Allow Upload
                                ratingModel.ShowResult = true;
                                ratingModel.EditMode = true;
                                ViewBag.divmsg = "This is not current period";
                                CreateListViewBags();
                                TempData["SearchedResults"] = ratingModel;
                                return View("Index", ratingModel);


                            }
                        }
                    }
                    else
                    {
                        //data exists for you and any changes will overwrite existing data. Press clear to stop editing submittedratings
                        ratingModel.IsAlert = true;
                        ratingModel.IsCurrentRatings = true;
                        ViewBag.alertmsg = "Data exists and has been <strong>Submitted</strong> for this rating period. Do you wish to <strong>Upload</strong> again and any overlay previously <strong>Submitted</strong> ratings?";
                        TempData["SearchedResults"] = ratingModel;
                        ratingModel.ShowResult = false;
                        ratingModel.EditMode = false;
                        CreateListViewBags();
                        return View("Index", ratingModel);
                    }
                }


            }
            else
            {
                ratingModel.ShowResult = false;
                ratingModel.EditMode = false;
                TempData["SearchedResults"] = ratingModel;
                CreateListViewBags();
                return View("Index", ratingModel);


            }

        }

        public ActionResult ClearStaging(int SiteID, int Year, int Month)
        {
            ExcelRatingsViewModel ratingModel = new ExcelRatingsViewModel() { SiteID = SiteID, Year = Year, Month = Month };
            int CheckingDate = Convert.ToInt32("" + ratingModel.Year + ratingModel.Month.ToString().PadLeft(2, '0'));
            using (Repository Rep = new Repository())
            {
                Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS.RemoveRange(Rep.Context.SPFS_STAGING_SUPPLIER_RATINGS.Where(s => s.SiteID == ratingModel.SiteID && s.Rating_period == CheckingDate));

                Rep.Context.SaveChanges();


            }
            CreateListViewBags();
            ratingModel.ShowResult = true;
            ratingModel.EditMode = true;
            TempData["SearchedResults"] = ratingModel;
            return View("Index", ratingModel);
        }



        private static List<RatingRecord> StageRecords(ExcelRatingsViewModel ratingModel, int CheckingDate, Repository Rep)
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

        private static List<RatingRecord> ProdRecords(ExcelRatingsViewModel ratingModel, int CheckingDate, Repository Rep)
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

        public void ExportData(string fileName)
        {
            List<RatingRecord> Records = (List<RatingRecord>)TempData["RatingRecords"];
            TempData.Keep("RatingRecords");
            var result = (from record in Records
                          select new ExportedRecord
                          {
                              CID = record.CID,
                              DUNS = record.DUNS.Trim(),
                              ERP_Supplier_ID = record.ERP_Supplier_ID,
                              OTD = record.OTD,
                              OTR = record.OTR,
                              PFR = record.PFR,
                              Inbound_parts = record.Inbound_parts
                          }).ToList();

            Utilities util = new Utilities();
            string User = util.CurrentUserName;
            ExportToExcel(result, fileName + User + DateTime.Now);


        }
        public bool ExportToExcel(List<ExportedRecord> Records, string filename)
        {
            bool exportStatus = false;
            try
            {
                ExcelPackage excel = new ExcelPackage();
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells[1, 1].LoadFromCollection(Records, true);
                using (var memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", string.Format("attachment;  filename=" + filename + ".xlsx; charset = utf - 8"));
                    excel.SaveAs(memoryStream);
                    memoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                    exportStatus = true;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Log(ex.Message, Logging.LoggingLevel.Error, ex, base.User.Identity.Name, "", "", "", "ExportToExcel ", this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString());
                exportStatus = false;
            }
            return exportStatus;
        }

        #region Merge
        public ActionResult UploadIndex(ExcelRatingsViewModel RatingModel)
        {
            RatingsViewModel rating = new RatingsViewModel();
            rating = Merge(RatingModel);
            TempData["SearchedResults"] = rating;

            var rateSuppliers = rating.RatingRecords.Select(r => new SelectListItem { Text = r.SupplierName + " CID:" + r.CID, Value = r.CID.ToString() }).ToList();
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
            // ViewBag.Suppliers = modifiedlist;
            return View("UploadIndex", rating);
        }

        private RatingsViewModel Merge(ExcelRatingsViewModel RatingModel)
        {
            List<RatingRecord> Records = (List<RatingRecord>)TempData["RatingRecords"];
            //TempData.Keep("RatingRecords");
            ExcelRatingsViewModel AggregatedModel = AggregateRecords(RatingModel, Records);
            RatingsViewModel ConvertedModel = new RatingsViewModel();

            List<RatingRecord> ISORecords = IncidentSpendOrder(RatingModel);
            //List<RatingRecord> HistoryRecords = IncidentSpendOrder(RatingModel);
            List<RatingRecord> MergedRecords = new List<RatingRecord>();
            List<RatingRecord> UnMatchedRecords = new List<RatingRecord>();

            var query = from x in ISORecords
                        join y in AggregatedModel.RatingRecords
                        on x.CID equals y.CID
                        select new { x, y };

            foreach (var match in query)
            {
                match.x.Inbound_parts = match.y.Inbound_parts;
                match.x.OTD = match.y.OTD;
                match.x.OTR = match.y.OTR;
                match.x.PFR = match.y.PFR;
                match.x.Reject_incident_count = match.y.Reject_incident_count;
                match.x.Reject_parts_count = match.y.Reject_parts_count;
                match.x.Temp_Upload_ = match.y.Temp_Upload_;
                match.x.ErrorInformation = match.y.ErrorInformation;


            }

            //  MergedRecords = ISORecords;
            var unmatch = (from agrr in AggregatedModel.RatingRecords
                           where !(ISORecords.Any(i => i.CID == agrr.CID))
                           select agrr).ToList();
            if (unmatch != null)
            {
                ISORecords.AddRange(unmatch);
            }

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
            ConvertedModel.RatingRecords = MergedRecords;
            ConvertedModel.isUpload = true;
            ConvertedModel.Month = RatingModel.Month;
            ConvertedModel.Year = RatingModel.Year;
            ConvertedModel.SiteID = RatingModel.SiteID;
            SelectSiteGDIS gdis = selectGDIS.Where(g => g.SiteID.Equals(RatingModel.SiteID)).FirstOrDefault();

            ConvertedModel.SiteName = gdis.Name;
            //var count = 0;
            //foreach (var record in GroupedRecords)
            //{
            //    if ((record.ErrorInformation != null ? record.ErrorInformation.Count : 0) > 0)
            //    {
            //        count++;
            //    }
            //}
            return ConvertedModel;
        }

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

        private ExcelRatingsViewModel AggregateRecords(ExcelRatingsViewModel RatingModel, List<RatingRecord> Records)
        {
            List<RatingRecord> GroupedRecords = Records
                                    .GroupBy(r => r.CID)// new { r.CID,r.DUNS })
                                    .Select(g => new RatingRecord
                                    {
                                        CID = g.Key,
                                        DUNS = g.First().DUNS,
                                        SupplierName = selectSuppliers.Where(s => s.Value == g.Key.ToString()).First().Text,
                                        Gdis_org_entity_ID = g.First().Gdis_org_entity_ID,
                                        Inbound_parts = g.Sum(s => s.Inbound_parts),
                                        Reject_incident_count = g.Sum(s => s.Reject_incident_count),
                                        Reject_parts_count = g.Sum(s => s.Reject_parts_count),
                                        OTD = g.Sum(s => s.OTD),
                                        OTR = g.Sum(s => s.OTR),
                                        PFR = g.Sum(s => s.PFR),
                                        ErrorInformation = g.SelectMany((s => s.ErrorInformation != null ? s.ErrorInformation : new List<ErrorDetails>())).ToList(),
                                        Temp_Upload_ = g.First().Temp_Upload_
                                    }).ToList();

            RatingModel.RatingRecords = GroupedRecords;
            return RatingModel;
        }

        public List<RatingRecord> IncidentSpendOrder(ExcelRatingsViewModel RatingModel)
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

            Mergedrecords.ForEach(z => z.DUNS = z.DUNS.Replace("\0", "").Trim());
            Sortedrecords = Mergedrecords.OrderByDescending(x => x.Reject_incident_count).ThenByDescending(x => x.Total_Spend).ToList();

            return Sortedrecords;

        }
        #endregion



        /// <summary>
        /// Get Supplier by Search.
        /// </summary>
        /// <param name="search">search</param>
        /// <returns></returns>
        /// 
        public ActionResult LoadSuppliers(bool? errorCheck)
        {
            if (!errorCheck.HasValue)
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
            else
            {
                ViewBag.Suppliers = selectSupplierscid;
                return PartialView("_AddSupplier");
            }
        }
        public JsonResult GetSupplierbyName(string nameString, bool? errorCheck)
        {
            if (!errorCheck.HasValue)
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
            else
            {
                var newSuppliercache = string.IsNullOrWhiteSpace(nameString) ? selectSupplierscid :
               selectSupplierscid.Where(s => s.Text.StartsWith(nameString, StringComparison.InvariantCultureIgnoreCase));
                return Json(newSuppliercache, JsonRequestBehavior.AllowGet);
            }


        }

        public JsonResult UpdateRecord(int CID, string Name, int Rowid)
        {
            List<RatingRecord> Records = (List<RatingRecord>)TempData["RatingRecords"];

            RatingRecord OldRec = new RatingRecord();

            RatingRecord UpdatedRec = new RatingRecord();

            OldRec = Records.Where(r => r.ExcelDiferentiatorID.Equals(Rowid)).FirstOrDefault();

            UpdatedRec = Records.Where(r => r.ExcelDiferentiatorID.Equals(Rowid)).FirstOrDefault();

            Records.Remove(OldRec);

            List<ErrorDetails> ErrorInfo = new List<ErrorDetails>();

            UpdatedRec.CID = CID;
            UpdatedRec.DUNS = GetDUNSfromCID(CID);
            UpdatedRec.SupplierName = Name;
            UpdatedRec.Temp_Upload_ = true;
            UpdateErrors(UpdatedRec, ErrorInfo);

            Records.Add(UpdatedRec);
            //RatingRecord 

            TempData["RatingRecords"] = Records;

            return Json(UpdatedRec, JsonRequestBehavior.AllowGet);
        }

        private static void UpdateErrors(RatingRecord item, List<ErrorDetails> ErrorInfo)
        {
            string msgSupplierName = string.Empty;

            msgSupplierName = "Record passed primary key validation";
            ErrorInfo.Add(new ErrorDetails { Key = Convert.ToString(item.SupplierName), ErrorMessage = msgSupplierName });

            item.ErrorInformation = ErrorInfo;
        }

        public JsonResult RemoveRecord(int Rowid)
        {
            List<RatingRecord> Records = (List<RatingRecord>)TempData["RatingRecords"];

            RatingRecord RemoveRec = new RatingRecord();

            RemoveRec = Records.Where(r => r.ExcelDiferentiatorID.Equals(Rowid)).FirstOrDefault();

            Records.Remove(RemoveRec);

            // UpdatedRecords = Records;         
            //RatingRecord 

            TempData["RatingRecords"] = Records;



            return Json(true, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "SaveData")]
        public ActionResult SaveData(RatingsViewModel ratingModel)
        {

            if (true)
            {

                ModelState.AddModelError("", "error");
            }

            if (ModelState.IsValid)
            {
            }


            return View("UploadIndex", ratingModel);

        }

        [HttpPost]
        [MultipleSubmitAttribute(Name = "action", Argument = "SubmitData")]
        public ActionResult SubmitData(RatingsViewModel ratingModel)
        {



            return View("UploadIndex", ratingModel);

        }
    }
}