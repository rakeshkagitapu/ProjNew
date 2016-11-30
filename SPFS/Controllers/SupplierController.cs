using PagedList;
using PagedList.Mvc;
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
    public class SupplierController : BaseController
    {
        // GET: Supplier

        public ActionResult Index1()
        {
            //Clear Tempdata of Search Criteria
            InitializeSearchCriteria();
            return RedirectToAction("Index");
        }

        private void InitializeSearchCriteria()
        {
            TempData["SupplierSearchCriteria"] = new GeneralSearchCriteria { Active = 2, Page = 1, SearchText = string.Empty, CurrentSort = " " };
        }

        public ActionResult Index(int? page)
        {
            CreateViewBags();
            List<SupplierListViewModel> suppliers = new List<SupplierListViewModel>();

            if (TempData["SupplierSearchCriteria"] == null) InitializeSearchCriteria();
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
            suppliers = GetSuppliers(objSearch);
            suppliers = SortByInput(objSearch.CurrentSort, suppliers);

            int pageNumber = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View(suppliers.ToPagedList(pageNumber, SPFSConstants.SupplierPageSize));
        }

        private List<SupplierListViewModel> GetSuppliers(GeneralSearchCriteria ObjSearch)
        {
            List<SupplierListViewModel> suppliers;
            using (Repository rep = new Repository())
            {
                var supplier = (from supp in rep.Context.SPFS_SUPPLIERS
                                select new SupplierListViewModel()
                                {
                                    CID = supp.CID,
                                    Duns = supp.Duns,
                                    Name = supp.Name,
                                    Address_1 = supp.Address_1,
                                    Address_2 = supp.Address_2,
                                    City = supp.City,
                                    State = supp.State,
                                    Country = supp.Country,
                                    Postal_Code = supp.Postal_Code,
                                    SPFS_Active = supp.SPFS_Active

                                });
                if (ObjSearch.Active.HasValue)
                {
                    if (ObjSearch.Active.Value == 1) supplier = supplier.Where(item => item.SPFS_Active);
                    if (ObjSearch.Active.Value == 0) supplier = supplier.Where(item => !item.SPFS_Active);

                }
                if (!string.IsNullOrEmpty(ObjSearch.SearchText))
                {
                    supplier = supplier.Where(item => item.Name.Contains(ObjSearch.SearchText) || item.Duns.Contains(ObjSearch.SearchText) || item.CID.ToString().Contains(ObjSearch.SearchText));

                }
                suppliers = SortByInput(ObjSearch.CurrentSort, supplier.ToList());
            }

            return suppliers;
        }

        private void CreateViewBags()
        {
            List<SelectListItem> list = new List<SelectListItem>() { new SelectListItem { Value = "1", Text = "Active" }, new SelectListItem { Value = "0", Text = "Inactive" },
             new SelectListItem { Value = "2", Text = "All"} };
            ViewBag.ActiveList = list;
        }

        /// <summary>
        /// Method to Get supplier's Overview
        /// </summary>
        /// 
        /// <returns></returns>
        public ActionResult ShowDetails(int? cid)
        {
            if (!cid.HasValue || cid <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SupplierListViewModel supplier = new SupplierListViewModel();
            using (Repository rep = new Repository())
            {
                supplier = (from sup in rep.Context.SPFS_SUPPLIERS
                            where sup.CID == cid
                            select new SupplierListViewModel()
                            {
                                Name = sup.Name,
                                Duns = sup.Duns,
                                CID = sup.CID,
                                Address_1 = sup.Address_1,
                                Address_2 = sup.Address_2,
                                City = sup.City,
                                Country = sup.Country,
                                Postal_Code = sup.Postal_Code,
                                State = sup.State,
                                SPFS_Active = sup.SPFS_Active ? true : false,


                            }).FirstOrDefault();
                if (supplier == null)
                {
                    return HttpNotFound();
                }
            }
            return View(supplier);
        }

        /// <summary>
        /// Method to Save Active supplier's Overview
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShowDetails(SupplierListViewModel supplier)
        {

            using (Repository rep = new Repository())
            {
                var supplr = rep.Context.SPFS_SUPPLIERS.Where(c => c.CID == supplier.CID).FirstOrDefault();
                if (TryUpdateModel(supplr))
                {
                    supplr.SPFS_Active = supplier.SPFS_Active;
                    supplr.Modified_by = new Utilities().GetCurrentUser().UserName;
                    supplr.Modified_date = DateTime.Now;

                    rep.Context.SaveChanges();

                    return RedirectToAction("Index");


                }

            }
            return View(supplier);
        }


        /// <summary>
        /// Method to Get suppliers by sort
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSuppliersBySort(int? id, string search, string sortby)
        {
            try
            {
                int pageno = 1;
                List<SupplierListViewModel> suppliers = null;
                ViewData["ActiveId"] = id.HasValue ? id.Value : 0;

                GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
                objSearch.Active = id;
                objSearch.CurrentSort = sortby;
                objSearch.SearchText = search;
                TempData["SupplierSearchCriteria"] = objSearch;

                suppliers = GetSuppliers(objSearch);
                pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
                return PartialView("_SupplierList", suppliers.ToPagedList(pageno, SPFSConstants.SupplierPageSize));
            }
            catch (Exception)
            {
                Utilities util = new Utilities();
                string msg = "error occured while loading suppliers information";
                throw new Exception(util.GetDivElements(msg, "alert alert-danger", "Error ! "));

            }
           
        }
        /// <summary>
        /// Method to Sort the suppliers by input 
        /// </summary>
        /// <param name="sortby"></param>
        /// <param name="suppliers"></param>
        /// <returns></returns>
        private List<SupplierListViewModel> SortByInput(string sortby, List<SupplierListViewModel> suppliers)
        {
            ViewData["NameSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Name Asc") ? "Name desc" : "Name Asc";
            ViewData["CIDSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("CID") ? "CID desc" : "CID Asc";
            ViewData["DUNSSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("DUNS Asc") ? "DUNS desc" : "DUNS Asc";
            ViewData["CitySort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("City Asc") ? "City desc" : "City Asc";
            ViewData["CountrySort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Country Asc") ? "Country desc" : "Country Asc";
            ViewData["IsActiveSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("IsActive Asc") ? "IsActive desc" : "IsActive Asc";
            ViewData["ActiveSort"] = sortby;
            switch (sortby)
            {
                case "Name desc":
                    suppliers = suppliers.OrderByDescending(s => s.Name).ToList();
                    break;
                case "Name Asc":
                    suppliers = suppliers.OrderBy(s => s.Name).ToList();
                    break;
                case "CID desc":
                    suppliers = suppliers.OrderByDescending(s => s.CID).ToList();
                    break;
                case "CID Asc":
                    suppliers = suppliers.OrderBy(s => s.CID).ToList();
                    break;
                case "DUNS desc":
                    suppliers = suppliers.OrderByDescending(s => s.Duns).ToList();
                    break;
                case "DUNS Asc":
                    suppliers = suppliers.OrderBy(s => s.Duns).ToList();
                    break;
                case "City desc":
                    suppliers = suppliers.OrderByDescending(s => s.City).ToList();
                    break;
                case "City Asc":
                    suppliers = suppliers.OrderBy(s => s.City).ToList();
                    break;
                case "Country desc":
                    suppliers = suppliers.OrderByDescending(s => s.Country).ToList();
                    break;
                case "Country Asc":
                    suppliers = suppliers.OrderBy(s => s.Country).ToList();
                    break;
               
                case "IsActive desc":
                    suppliers = suppliers.OrderByDescending(s => s.SPFS_Active).ToList();
                    break;
                case "IsActive Asc":
                    suppliers = suppliers.OrderBy(s => s.SPFS_Active).ToList();
                    break;
                default:
                    suppliers = suppliers.OrderBy(s => s.Name).ThenBy(ss => ss.CID).ToList();
                    break;
            }
            return suppliers;

        }

        /// <summary>
        /// Method to retrieve suppliers by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSupplierByActiveOrInActive(int? id, string search)
        {
            int pageno = 1;
            List<SupplierListViewModel> suppliers = null;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
            objSearch.Active = id;
            objSearch.SearchText = search;
            suppliers = GetSuppliers(objSearch);
            pageno = objSearch.Page.Value;
            TempData["SupplierSearchCriteria"] = objSearch;
            return PartialView("_SupplierList", suppliers.ToPagedList(pageno, SPFSConstants.SupplierPageSize));
        }
        /// <summary>
        /// Method to retrieve suppliers by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetSupplierBySearch(int? id, string search)
        {
            int pageno = 1;
            List<SupplierListViewModel> suppliers = null;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
            objSearch.Active = id;
            objSearch.SearchText = search;
            suppliers = GetSuppliers(objSearch);
            // pageno = objSearch.Page.Value;
            TempData["SupplierSearchCriteria"] = objSearch;
            return PartialView("_SupplierList", suppliers.ToPagedList(pageno, SPFSConstants.SupplierPageSize));
        }
        /// <summary>
        /// Method to retrieve suppliers by page
        /// </summary>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public ActionResult SearchByPage(int? page)
        {
            CreateViewBags();
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
            objSearch.Page = page.HasValue ? page : objSearch.Page;
            TempData["SupplierSearchCriteria"] = objSearch;

            List<SupplierListViewModel> suppliers = null;
            suppliers = GetSuppliers(objSearch);
            suppliers = SortByInput(objSearch.CurrentSort, suppliers);
            int pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View("Index", suppliers.ToPagedList(pageno, SPFSConstants.SupplierPageSize));
        }
        /// <summary>
        /// Used to Export Items to Excel.
        /// </summary>
        /// <param name="fileName"></param>
        public void ExportData(string fileName)
        {
            List<SupplierListViewModel> SupplierViewModel = null;
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["SupplierSearchCriteria"];
            TempData.Keep("SupplierSearchCriteria");
            if (objSearch != null)
            {
                SupplierViewModel = GetSuppliers(objSearch);
                SupplierViewModel = SortByInput(objSearch.CurrentSort, SupplierViewModel);
            }
            var result = (from sup in SupplierViewModel
                          select new
                          {
                              CID = sup.CID,
                              Duns = sup.Duns,
                              Name = sup.Name,
                              Address1 = sup.Address_1,
                              Address2 = sup.Address_2,
                              City = sup.City,
                              State = sup.State,
                              Country = sup.Country,
                              PostalCode = sup.Postal_Code,
                              Active = sup.SPFS_Active ? true : false,
                          }).ToList();


            base.ExportToCSV(result.GetCSV(), fileName + DateTime.Now.ToShortDateString());
        }

    }

}