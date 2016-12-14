using SPFS.DAL;
using SPFS.Helpers;
using SPFS.Model;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SPFS.Controllers
{
    public class UserSitesController : BaseController
    {
        // GET: UserSites
        public ActionResult Index(int? userId)
        {
            CreateListViewBags();
            var viewModel = GetSitesModel(userId);
            return View(viewModel);
        }

        private UserSitesViewModel GetSitesModel(int? userId)
        {
            var viewModel = new UserSitesViewModel();
            viewModel.UserID = userId;
            var siteIds = new List<int>();
            using (Repository SteRep = new Repository())
            {
                siteIds = (from sit in SteRep.Context.SPFS_USERSITES
                           where sit.UserID == userId 
                           select sit.SiteID).ToList();
            }
            viewModel.SelectedSiteIDs = siteIds;
            return viewModel;
        }

        private void CreateListViewBags()
        {
            List<SelectListItem> sites;
            List<SelectListItem> users;
            using (Repository UserRep = new Repository())
            {
                sites = (from ste in UserRep.Context.SPFS_SITES
                         where ste.SPFS_Active == true
                         select new SelectListItem { Value = ste.SiteID.ToString(), Text = ste.Name }).ToList();
                users = (from usr in UserRep.Context.SPFS_USERS
                         where usr.SPFS_Active == true
                         select new SelectListItem { Value = usr.UserID.ToString(), Text = usr.UserName }).ToList();
            }
            ViewBag.Users = users;
            ViewBag.Sites = sites;
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetSitesForUser(int? userId)
        {
            try
            {
                
                CreateListViewBags();
                var viewModel = GetSitesModel(userId);
                return PartialView("_UserSitesList", viewModel);
            }
            catch
            {
                Utilities util = new Utilities();
                string msg = "error occured while loading sites information";
                throw new Exception(util.GetDivElements(msg, "alert alert-danger", "Error ! "));
            }
        }
        [HttpPost]
        public ActionResult Index(UserSitesViewModel viewModel)
        {

            if (ModelState.IsValid)
            {

                using (Repository UserRep = new Repository())
                {
                    Utilities utils = new Utilities();
                    var usersiteIds = (from usr in UserRep.Context.SPFS_USERSITES
                                       where usr.UserID == viewModel.UserID
                                       select usr.SiteID).ToList();

                    var deleteIds = (from id in usersiteIds
                                     where viewModel.SelectedSiteIDs == null ? true
                                     : !viewModel.SelectedSiteIDs.Contains(id)
                                     select id).ToList();
                    if (viewModel.SelectedSiteIDs != null)
                    {

                        foreach (var siteId in viewModel.SelectedSiteIDs)
                        {
                            if (usersiteIds.Contains(siteId))
                            {
                                var userSite = UserRep.Context.SPFS_USERSITES.FirstOrDefault(usrSite => usrSite.UserID == viewModel.UserID && usrSite.SiteID == siteId);
                                userSite.Modified_by = utils.GetCurrentUser().UserName;
                                userSite.Modified_date = DateTime.Now;
                            }
                            else
                            {
                                var userSite = new SPFS_USERSITES()
                                {
                                    UserID = viewModel.UserID.Value,
                                    SiteID = siteId,
                                    Created_by = utils.GetCurrentUser().UserName,
                                    Created_date = DateTime.Now,
                                };
                                UserRep.Context.SPFS_USERSITES.Add(userSite);
                            }
                        }
                    }
                    if (deleteIds != null && deleteIds.Count > 0)
                    {
                        var deleteList = UserRep.Context.SPFS_USERSITES.Where(ite => deleteIds.Contains(ite.SiteID) && ite.UserID == viewModel.UserID).AsEnumerable();
                        UserRep.Context.SPFS_USERSITES.RemoveRange(deleteList);
                    }
                    UserRep.Context.SaveChanges();

                    return RedirectToAction("Index", new { userId = viewModel.UserID });
                }
            }
            CreateListViewBags();
            return View(viewModel);
        }

        // GET: UserSites
        public ActionResult SitesUser(int? siteId)
        {
            CreateListViewBags();
            var viewmodel = GetUsersModel(siteId);
            return View(viewmodel);
        }

        private SiteUsersViewModel GetUsersModel(int? siteId)
        {
            var viewmodel = new SiteUsersViewModel();
            viewmodel.SiteID = siteId;
            var userIds = new List<int>();
            using (Repository UsrRep = new Repository())
            {
                userIds = (from usr in UsrRep.Context.SPFS_USERSITES
                           where usr.SiteID == siteId
                           select usr.UserID).ToList();
            }
            viewmodel.SelectedUserIDs = userIds;
            return viewmodel;
        }



        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetUsersForSite(int? siteId)
        {
            try
            {
                CreateListViewBags();
                var viewmodel = GetUsersModel(siteId);
                return PartialView("_SiteUsersList", viewmodel);
            }
            catch (Exception)
            {
                Utilities util = new Utilities();
                string msg = "error occured while loading users information";
                throw new Exception(util.GetDivElements(msg, "alert alert-danger", "Error ! "));

            }



        }
        [HttpPost]
        public ActionResult SitesUser(SiteUsersViewModel viewmodel)
        {

            if (ModelState.IsValid)
            {

                using (Repository SiteRep = new Repository())
                {
                    Utilities utils = new Utilities();
                    var siteuserIds = (from usr in SiteRep.Context.SPFS_USERSITES
                                       where usr.SiteID == viewmodel.SiteID
                                       select usr.UserID).ToList();

                    var deleteUsrIds = (from id in siteuserIds
                                        where viewmodel.SelectedUserIDs == null ? true
                                     : !viewmodel.SelectedUserIDs.Contains(id)
                                        select id).ToList();
                    if (viewmodel.SelectedUserIDs != null)
                    {

                        foreach (var userId in viewmodel.SelectedUserIDs)
                        {
                            if (siteuserIds.Contains(userId))
                            {
                                var siteUser = SiteRep.Context.SPFS_USERSITES.FirstOrDefault(usrSite => usrSite.SiteID == viewmodel.SiteID && usrSite.UserID == userId);
                                siteUser.Modified_by = utils.GetCurrentUser().UserName;
                                siteUser.Modified_date = DateTime.Now;
                            }
                            else
                            {
                                var siteUser = new SPFS_USERSITES()
                                {
                                    SiteID = viewmodel.SiteID.Value,
                                    UserID = userId,
                                    Created_by = utils.GetCurrentUser().UserName,
                                    Created_date = DateTime.Now,
                                };
                                SiteRep.Context.SPFS_USERSITES.Add(siteUser);
                            }
                        }
                    }
                    if (deleteUsrIds != null && deleteUsrIds.Count > 0)
                    {
                        var deleteList = SiteRep.Context.SPFS_USERSITES.Where(ite => deleteUsrIds.Contains(ite.UserID) && ite.UserID == viewmodel.SiteID).AsEnumerable();
                        SiteRep.Context.SPFS_USERSITES.RemoveRange(deleteList);
                    }
                    SiteRep.Context.SaveChanges();

                    return RedirectToAction("SitesUser", new { userId = viewmodel.SiteID });
                }
            }
            CreateListViewBags();
            return View(viewmodel);
        }
    }
}