using PagedList;
using SPFS.DAL;
using SPFS.Helpers;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Controllers
{
    public class SiteController : BaseController
    {
        // GET: Sites
        public ActionResult Index1()
        {
            //Clear Tempdata of Search Criteria
            InitializeSearchCriteria();
            return RedirectToAction("Index");
        }

        private void InitializeSearchCriteria()
        {
            TempData["SiteSearchCriteria"] = new GeneralSearchCriteria { Active = 2, Page = 1, SearchText = string.Empty, CurrentSort = " " };
        }

        public ActionResult Index(int? page)
        {
            CreateViewBags();
            List<SiteListViewModel> Sites = new List<SiteListViewModel>();

            if (TempData["SiteSearchCriteria"] == null) InitializeSearchCriteria();
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
            Sites = GetSites(objSearch);
            Sites = SortByInput(objSearch.CurrentSort, Sites);

            int pageNumber = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View(Sites.ToPagedList(pageNumber, SPFSConstants.SitePageSize));
        }

        private List<SiteListViewModel> GetSites(GeneralSearchCriteria ObjSearch)
        {
            List<SiteListViewModel> Sites;
            using (Repository rep = new Repository())
            {
               var Site= (from site in rep.Context.SPFS_SITES
                     select new SiteListViewModel()
                     {
                         SiteID =site.SiteID,
                         Gdis_org_entity_ID = site.Gdis_org_entity_ID,
                         Gdis_org_Parent_ID = site.Gdis_org_Parent_ID,
                         Name = site.Name,
                         Address_1 = site.Address_1,
                         Address_2 = site.Address_2,
                         City = site.City,
                         State = site.State,
                         Country = site.Country,
                         Postal_Code = site.Postal_Code,
                         SPFS_Active = site.SPFS_Active,


                     });
                if (ObjSearch.Active.HasValue)
                {
                    if (ObjSearch.Active.Value == 1) Site = Site.Where(item => item.SPFS_Active);
                    if (ObjSearch.Active.Value == 0) Site = Site.Where(item => !item.SPFS_Active);

                }
                if (!string.IsNullOrEmpty(ObjSearch.SearchText))
                {
                     Site = Site.Where(item => item.Name.Contains(ObjSearch.SearchText) || item.Gdis_org_entity_ID.ToString().Contains(ObjSearch.SearchText) || item.Gdis_org_Parent_ID.ToString().Contains(ObjSearch.SearchText)); 
                    
                }
                Sites = SortByInput(ObjSearch.CurrentSort, Site.ToList());
            }

            return Sites;
        }

        private void CreateViewBags()
        {
            List<SelectListItem> list = new List<SelectListItem>() { new SelectListItem { Value = "1", Text = "Active" }, new SelectListItem { Value = "0", Text = "Inactive" },
             new SelectListItem { Value = "2", Text = "All"} };
            ViewBag.ActiveList = list;
        }
        /// <summary>
        /// Method to Get Sites by sort
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSitesBySort(int? id, string search, string sortby)
        {
            try
            { 
                int pageno = 1;
                List<SiteListViewModel> Sites = null;
                ViewData["ActiveId"] = id.HasValue ? id.Value : 0;

                GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
                objSearch.Active = id;
                objSearch.CurrentSort = sortby;
                objSearch.SearchText = search;
                TempData["SiteSearchCriteria"] = objSearch;

                Sites = GetSites(objSearch);
                pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
                return PartialView("_SiteList", Sites.ToPagedList(pageno, SPFSConstants.SitePageSize));

            }
            catch (Exception)
            {
                Utilities util = new Utilities();
                string msg = "error occured while loading sites information";
                throw new Exception(util.GetDivElements(msg, "alert alert-danger", "Error ! "));

            }
            
        }
        /// <summary>
        /// Method to Sort the Sites by input 
        /// </summary>
        /// <param name="sortby"></param>
        /// <param name="Sites"></param>
        /// <returns></returns>
        private List<SiteListViewModel> SortByInput(string sortby, List<SiteListViewModel> Sites)
        {
            ViewData["NameSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Name Asc") ? "Name desc" : "Name Asc";
            ViewData["GDISORGSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("CID") ? "GDISORG desc" : "GDISORG Asc";
            ViewData["GDISPARSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("GDISPAR Asc") ? "GDISPAR desc" : "GDISPAR Asc";
            ViewData["CountrySort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Country Asc") ? "Country desc" : "Country Asc";
            ViewData["ZipSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Zip Asc") ? "Zip desc" : "Zip Asc";
            ViewData["IsActiveSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("IsActive Asc") ? "IsActive desc" : "IsActive Asc";
            ViewData["ActiveSort"] = sortby;
            switch (sortby)
            {
                case "Name desc":
                    Sites = Sites.OrderByDescending(s => s.Name).ToList();
                    break;
                case "Name Asc":
                    Sites = Sites.OrderBy(s => s.Name).ToList();
                    break;
                case "GDISORG desc":
                    Sites = Sites.OrderByDescending(s => s.Gdis_org_entity_ID).ToList();
                    break;
                case "GDISORG Asc":
                    Sites = Sites.OrderBy(s => s.Gdis_org_entity_ID).ToList();
                    break;
                case "GDISPAR desc":
                    Sites = Sites.OrderByDescending(s => s.Gdis_org_Parent_ID).ToList();
                    break;
                case "GDISPAR Asc":
                    Sites = Sites.OrderBy(s => s.Gdis_org_Parent_ID).ToList();
                    break;
                case "Country desc":
                    Sites = Sites.OrderByDescending(s => s.Country).ToList();
                    break;
                case "Country Asc":
                    Sites = Sites.OrderBy(s => s.Country).ToList();
                    break;
                case "Zip desc":
                    Sites = Sites.OrderByDescending(s => s.Postal_Code).ToList();
                    break;
                case "Zip Asc":
                    Sites = Sites.OrderBy(s => s.Postal_Code).ToList();
                    break;
                case "IsActive desc":
                    Sites = Sites.OrderByDescending(s => s.SPFS_Active).ToList();
                    break;
                case "IsActive Asc":
                    Sites = Sites.OrderBy(s => s.SPFS_Active).ToList();
                    break;
                default:
                    Sites = Sites.OrderBy(s => s.Name).ThenBy(ss => ss.Gdis_org_entity_ID).ToList();
                    break;
            }
            return Sites;

        }

        /// <summary>
        /// Method to retrieve Sites by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSiteByActiveOrInActive(int? id, string search)
        {
            int pageno = 1;
            List<SiteListViewModel> Sites = null;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
            objSearch.Active = id;
            objSearch.SearchText = search;
            Sites = GetSites(objSearch);
            pageno = objSearch.Page.Value;
            TempData["SiteSearchCriteria"] = objSearch;
            return PartialView("_SiteList", Sites.ToPagedList(pageno, SPFSConstants.SitePageSize));
        }
        /// <summary>
        /// Method to retrieve Sites by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSiteBySearch(int? id, string search)
        {
            int pageno = 1;
            List<SiteListViewModel> Sites = null;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
            objSearch.Active = id;
            objSearch.SearchText = search;
            Sites = GetSites(objSearch);
            //pageno = objSearch.Page.Value;
            TempData["SiteSearchCriteria"] = objSearch;
            return PartialView("_SiteList", Sites.ToPagedList(pageno, SPFSConstants.SitePageSize));
        }
        /// <summary>
        /// Method to retrieve Sites by page
        /// </summary>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public ActionResult SearchByPage(int? page)
        {
            CreateViewBags();
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
            objSearch.Page = page.HasValue ? page : objSearch.Page;
            TempData["SiteSearchCriteria"] = objSearch;

            List<SiteListViewModel> Sites = null;
            Sites = GetSites(objSearch);
            Sites = SortByInput(objSearch.CurrentSort, Sites);
            int pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View("Index", Sites.ToPagedList(pageno, SPFSConstants.SitePageSize));
        }

        /// <summary>
        /// Method to Get site's Overview
        /// </summary>
        /// 
        /// <returns></returns>
        public ActionResult SiteDetails(int? siteid)
        {
            if (!siteid.HasValue || siteid <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiteListViewModel site = new SiteListViewModel();
            using (Repository rep = new Repository())
            {
                site = (from sit in rep.Context.SPFS_SITES
                            where sit.SiteID == siteid
                            select new SiteListViewModel()
                            {
                                Gdis_org_entity_ID = sit.Gdis_org_entity_ID,
                                Gdis_org_Parent_ID =sit.Gdis_org_Parent_ID,
                                Name = sit.Name,
                                Address_1 = sit.Address_1,
                                Address_2 = sit.Address_2,
                                City = sit.City,
                                Country = sit.Country,
                                Postal_Code = sit.Postal_Code,
                                State = sit.State,
                                SPFS_Active = sit.SPFS_Active ? true : false,


                            }).FirstOrDefault();
                if (site == null)
                {
                    return HttpNotFound();
                }
            }
            return View(site);
        }

        /// <summary>
        /// Method to Save Active site's Overview
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SiteDetails(SiteListViewModel siteviewmodel)
        {

            using (Repository rep = new Repository())
            {
                var site = rep.Context.SPFS_SITES.Where(c => c.SiteID == siteviewmodel.SiteID).FirstOrDefault();
                if (TryUpdateModel(site))
                {
                    site.SPFS_Active = siteviewmodel.SPFS_Active;
                    site.Modified_by = new Utilities().GetCurrentUser().UserName;
                    site.Modified_date = DateTime.Now;

                    rep.Context.SaveChanges();

                    return RedirectToAction("Index");


                }

            }
            return View(siteviewmodel);
        }
        /// <summary>
        /// Used to Export Items to Excel.
        /// </summary>
        /// <param name="fileName"></param>
        public void ExportData(string fileName)
        {
            List<SiteListViewModel> SiteViewModel = null;
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SiteSearchCriteria"];
            TempData.Keep("SiteSearchCriteria");
            if (objSearch != null)
            {
                SiteViewModel = GetSites(objSearch);
                SiteViewModel = SortByInput(objSearch.CurrentSort, SiteViewModel);
            }
            var result = (from site in SiteViewModel
                          select new
                          {
                              Gdis_org_entity_ID = site.Gdis_org_entity_ID,
                              Gdis_org_Parent_ID = site.Gdis_org_Parent_ID,
                              Name = site.Name,
                              Address_1 = site.Address_1,
                              Address_2 = site.Address_2,
                              City = site.City,
                              State = site.State,
                              Country = site.Country,
                              Postal_Code = site.Postal_Code,
                              SPFS_Active = site.SPFS_Active,
                          }).ToList();


            base.ExportToCSV(result.GetCSV(), fileName + DateTime.Now.ToShortDateString());
        }
    }
}