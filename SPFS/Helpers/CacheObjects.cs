using SPFS.DAL;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Helpers
{
    public class CacheObjects
    {
        private static List<SupplierCacheViewModel> supplierCacheObj;
        
        private static DateTime _cacheLastChecked;

        private static DateTime _cacheLastCheckedSup;
        
        private static List<SelectListItem> selectSuppliers;

        private static List<SelectListItem> selectSuppliersCID;

        private static List<SelectSiteGDIS> selectGDIS;

        public List<SupplierCacheViewModel> GetSuppliersCache { get { return supplierCacheObj; } }

        public List<SelectListItem> GetSuppliers { get { return selectSuppliers; } }

        public List<SelectListItem> GetSuppliersCID { get { return selectSuppliersCID; } }
        public List<SelectSiteGDIS> GetSites { get { return selectGDIS; } }
        public CacheObjects()
        {
            if (supplierCacheObj == null)
            {
                supplierCacheObj = GetSupplierCacheData();
                selectGDIS = GetSiteListData();
                _cacheLastChecked = DateTime.Now;
            }
            else
            {
                CheckCache();
            }

            if (selectSuppliers == null)
            {
                selectSuppliers = GetSupplierListData();
                selectSuppliersCID = GetSupplierListDataCID(selectSuppliers);
                _cacheLastCheckedSup = DateTime.Now;
            }
            else
            {
                CheckSupCache();
            }
        }

        private void CheckCache()
        {
            int cacheRefresh;
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CacheRefresh"], out cacheRefresh))
                cacheRefresh = 12 * 60;
            if (_cacheLastChecked.AddMinutes(cacheRefresh) < DateTime.Now)
            {
                supplierCacheObj = GetSupplierCacheData();
                selectGDIS = GetSiteListData();
                _cacheLastChecked = DateTime.Now;
            }
        }
        private void CheckSupCache()
        {
            int cacheRefreshSup;
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CacheRefreshSup"], out cacheRefreshSup))
                cacheRefreshSup = 55;
            if (_cacheLastCheckedSup.AddMinutes(cacheRefreshSup) < DateTime.Now)
            {
                selectSuppliers = GetSupplierListData();
                selectSuppliersCID = GetSupplierListDataCID(selectSuppliers);
                _cacheLastCheckedSup = DateTime.Now;
            }
        }
        private List<SupplierCacheViewModel> GetSupplierCacheData()
        {
            List<SupplierCacheViewModel> result = new List<SupplierCacheViewModel>();
            List<SupplierCacheViewModel> Formatedresult = new List<SupplierCacheViewModel>();
            using (Repository repository = new Repository())
            {
                var MultipleLeftJoin = from spend in (from supSpend in
                                          (from sup in repository.Context.SPFS_SUPPLIERS
                                           join spendSup in repository.Context.SPFS_SPEND_SUPPLIERS on sup.CID equals spendSup.CID into JoinedSupSpend
                                           from spendSup in JoinedSupSpend.DefaultIfEmpty()
                                           where  sup.SPFS_Active == true
                                           select new
                                           {
                                               CID = sup.CID,
                                               Duns = sup.Duns,
                                               SpendSupplierID = spendSup != null ? spendSup.Spend_supplier_ID : 0,
                                               SiteID = spendSup != null ? spendSup.SiteID : 0

                                           })
                                                      join erp in repository.Context.SPFS_LINK_ERP on supSpend.SpendSupplierID equals erp.Spend_supplier_ID into JoinedErp
                                                      from erp in JoinedErp.DefaultIfEmpty()
                                                      select new
                                                      {
                                                          CID = supSpend.CID,
                                                          Duns = supSpend.Duns, //.Replace("\0", "").Trim(),
                                                          ERPSupplierID = erp.Erp_supplier_ID,
                                                          SiteID = supSpend.SiteID
                                                      })
                                       join site in repository.Context.SPFS_SITES on spend.SiteID equals site.SiteID into JoinedSite
                                       from site in JoinedSite.DefaultIfEmpty()
                                       select new SupplierCacheViewModel
                                       {
                                           CID = spend.CID,
                                           Duns = spend.Duns, //.Replace("\0", "").Trim(),
                                           ERPSupplierID = spend.ERPSupplierID.Trim(),
                                           Gdis_org_entity_ID = site != null ? site.Gdis_org_entity_ID : 0

                                       };


                result = MultipleLeftJoin.ToList();

            }


            result.ForEach(z => z.Duns = z.Duns.Replace("\0", "").Trim());
            return result;
        }

        private List<SelectListItem> GetSupplierListData()
        {
            List<SelectListItem> suppliers;
            using (Repository repository = new Repository())
            {
                suppliers = (from supplier in repository.Context.SPFS_SUPPLIERS
                             where supplier.SPFS_Active == true
                             select new SelectListItem { Value = supplier.CID.ToString(), Text = supplier.Name }).ToList();
            }
            return suppliers;
        }
        private List<SelectListItem> GetSupplierListDataCID(List<SelectListItem> selectSuppliers)
        {
            List<SelectListItem> suppliers;
            using (Repository repository = new Repository())
            {
                suppliers = selectSuppliers.Select(r => new SelectListItem { Text = r.Text + " CID:" + r.Value, Value = r.Value }).ToList();
            }
            return suppliers;
        }
        private List<SelectSiteGDIS> GetSiteListData()
        {
            List<SelectSiteGDIS> sites;
            using (Repository repository = new Repository())
            {
                sites = (from site in repository.Context.SPFS_SITES
                         where (site.SPFS_Active ==true)
                         select new SelectSiteGDIS { Gdis_org_entity_ID = site.Gdis_org_entity_ID, SiteID = site.SiteID, Gdis_org_Parent_ID = site.Gdis_org_Parent_ID, Name = site.Name }).ToList();
            }
            return sites;
        }
    }
}