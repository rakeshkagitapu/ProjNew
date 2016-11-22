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
        private static List<SupplierCacheViewModel> supplierCacheObj;

        public ExcelUploadController()
        {
            if (supplierCacheObj == null)
            {
                supplierCacheObj = GetSupplierCacheData();
            }
        }

        private List<SupplierCacheViewModel> GetSupplierCacheData()
        {
            List<SupplierCacheViewModel> result = new List<SupplierCacheViewModel>();
            List<SupplierCacheViewModel> Formatedresult = new List<SupplierCacheViewModel>();
            using (Repository repository = new Repository())
            {
               var MultipleLeftJoin = from supSpend in 
                                         (from sup in repository.Context.SPFS_SUPPLIERS
                                          join spendSup in repository.Context.SPFS_SPEND_SUPPLIERS on sup.CID equals spendSup.CID into JoinedSupSpend
                                          from spendSup in JoinedSupSpend.DefaultIfEmpty()
                                          select new
                                          {
                                              CID = sup.CID,
                                              Duns = sup.Duns,
                                              SpendSupplierID = spendSup !=null ? spendSup.Spend_supplier_ID : 0

                                          }) 
                                          join erp in repository.Context.SPFS_LINK_ERP on supSpend.SpendSupplierID equals erp.Spend_supplier_ID into JoinedErp
                                          from erp in JoinedErp.DefaultIfEmpty()
                                     select new SupplierCacheViewModel
                                     {
                                         CID = supSpend.CID,
                                         Duns = supSpend.Duns, //.Replace("\0", "").Trim(),
                                         ERPSupplierID = erp != null ? erp.Erp_supplier_ID : 0
                                     };


                result = MultipleLeftJoin.ToList();

                //result = (from sup in repository.Context.SPFS_SUPPLIERS
                //          join spendSup in repository.Context.SPFS_SPEND_SUPPLIERS on sup.CID equals spendSup.CID
                //          join erpsup in repository.Context.SPFS_LINK_ERP on spendSup.Spend_supplier_ID equals erpsup.Spend_supplier_ID into tmpErp
                //          from erp in tmpErp.DefaultIfEmpty()

                                     //          select new SupplierCacheViewModel
                                     //          {
                                     //              CID = sup.CID,
                                     //              Duns = sup.Duns.Trim(),
                                     //              ERPSupplierID = erp != null ? erp.Erp_supplier_ID : 0,
                                     //          }).Distinct().ToList();

            }

            foreach(var item in result)
            {
                SupplierCacheViewModel scv = new SupplierCacheViewModel();
                scv.CID = item.CID;
                scv.Duns = item.Duns.Replace("\0", "").Trim();
                scv.ERPSupplierID = item.ERPSupplierID;
                Formatedresult.Add(scv);
            }
            return Formatedresult;

           // return result;
        }

        //GET: ExcelUpload
        public ActionResult Index(int? SiteID, bool isUpload = true)
        {
            ExcelRatingsViewModel ratingsViewModel = new ExcelRatingsViewModel { SiteID = SiteID, isUpload = isUpload };
            ratingsViewModel.Month = DateTime.Now.Month - 1;
            ratingsViewModel.Year = DateTime.Now.Year;

            CreateListViewBags();
            return View(ratingsViewModel);
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
            List<SelectListItem> suppliers;
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

                suppliers = (from supplier in UserRep.Context.SPFS_SUPPLIERS
                         select new SelectListItem { Value = supplier.CID.ToString(), Text = supplier.Name}).ToList();
            }

            ViewBag.Months = util.GetMonths(true);
            ViewBag.Years = util.GetYears(true);
            ViewBag.Sites = sites;
            ViewBag.Suppliers = suppliers;
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
            CreateListViewBags();
            return View("ExcelReview", ratingModel);
        }

        private ExcelRatingsViewModel ProcessExcelDataintoViewModel(ExcelRatingsViewModel ratingModel, DataSet result)
        {
            List<RatingRecord> ratings = result.Tables[0].ToList<RatingRecord>();
            List<RatingRecord> Inboundratings = CheckInboundRatings(ratings);
            List<RatingRecord> PrimaryKeyratings = new List<RatingRecord>();
            foreach (var item in Inboundratings)
            {
                if (!string.IsNullOrEmpty(item.DUNS))
                    {
                    item.DUNS = item.DUNS.PadLeft(9, '0');
                    }
                List<ErrorDetails> ErrorInfo = new List<ErrorDetails>();
                bool iRecordfound = false;
               if( supplierCacheObj.Any(m =>m.ERPSupplierID.Equals(item.ERP_Supplier_ID)))  //supplierCacheObj.Any(m => m.CID.Equals(item.CID))
                {
                   
                    if(supplierCacheObj.Any(m=>m.CID.Equals(item.CID)))
                    {
                        iRecordfound = true;
                        if (string.IsNullOrWhiteSpace(item.DUNS) )
                        {
                            item.DUNS = GetDUNSfromCID(item.CID);
                        }
                    }
                    else if (supplierCacheObj.Any(m => m.Duns.Equals(item.DUNS)))
                    { 
                      
                        item.CID = GetCIDfromDuns(item.DUNS);
                        if(item.CID ==0)
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
                else if(supplierCacheObj.Any(m => m.CID.Equals(item.CID)))
                {
                    iRecordfound = true;
                    if (string.IsNullOrWhiteSpace(item.DUNS))
                    {
                        item.DUNS = GetDUNSfromCID(item.CID);
                    }
                }
               else if(supplierCacheObj.Any(m => m.Duns.Equals(item.DUNS)))
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
                      where sup.Duns== DUNS
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
            return DUNS;
        }
        private List<RatingRecord> CheckInboundRatings(List<RatingRecord> ratngs)
        {
            List<RatingRecord> ratings = new List<RatingRecord>();
            foreach(var item in ratngs)
            {
               if(item.Inbound_parts > 0)
                {
                    //RatingRecord ratingRecord = new RatingRecord();
                    // ratingRecord.CID = int.TryParse(item.CID,)

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
            ExcelRatingsViewModel excelViewModel = new ExcelRatingsViewModel();
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

        public void ExportData(string fileName)
        {
            List<RatingRecord> Records = (List<RatingRecord>)TempData["RatingRecords"];
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

            ExportToExcel(result, fileName + DateTime.Now.ToShortDateString());


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
    }
}