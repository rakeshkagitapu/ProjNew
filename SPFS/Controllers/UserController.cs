using SPFS.DAL;
using SPFS.Helpers;
using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using SPFS.Model;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace SPFS.Controllers
{

    public class UserController : BaseController
    {
        public ActionResult Index1()
        {
            //Clear Tempdata of Search Criteria          
            IntializeSearchCriteria();
            return RedirectToAction("Index");
        }

        private void IntializeSearchCriteria()
        {
            TempData["UserSearchCriteria"] = new GeneralSearchCriteria { Active = 2, Page = 1, SearchText = string.Empty, CurrentSort = " " };
        }

        // GET: User
        public ActionResult Index()
        {
            CreateViewBags();
            List<UserListViewModel> users = new List<UserListViewModel>();

            if (TempData["UserSearchCriteria"] == null)
            { IntializeSearchCriteria(); }
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
            users = GetUsers(objSearch);
            users = SortByInput(objSearch.CurrentSort, users);
            int pageNumber = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View(users.ToPagedList(pageNumber, SPFSConstants.UserPageSize));
        }

        private void CreateViewBags()
        {
            List<SelectListItem> roles;
            using (Repository roleRep = new Repository())
            {
                var role = (from rol in roleRep.Context.SPFS_ROLES
                            select new SelectListItem { Value = rol.RoleID.ToString(), Text = rol.RoleName });
                Utilities util = new Utilities();
                if (util.GetCurrentUser().RoleID == 3)
                {
                    role = role.Where(item => item.Value != "1");
                }
                roles = role.ToList();
            }
            ViewBag.Roles = roles;


            List<SelectListItem> list = new List<SelectListItem>() { new SelectListItem { Value = "1", Text = "Active" }, new SelectListItem { Value = "0", Text = "Inactive" },
             new SelectListItem { Value = "2", Text = "All"} };
            ViewBag.ActiveList = list;
        }

        public ActionResult Create(string username)
        {
            CreateViewBags();
            UserViewModel user = new UserViewModel() { SPFS_Active = true };

            if (!string.IsNullOrEmpty(username))
            {
                if (TempData["AdUsers"] != null)
                {
                    List<UserViewModel> adResult = (List<UserViewModel>)TempData["AdUsers"];
                    if (adResult.Count > 0)
                    {
                        TempData["AdUsers"] = adResult;
                        user = adResult.Where(item => item.UserName.Equals(username)).FirstOrDefault();
                    }
                }

                user = new UserViewModel() { UserName = user.UserName, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, SPFS_Active = true };
                TempData["AdUsers"] = null;
            }

            return View(user);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel usr)
        {
            using (Repository rep = new Repository())
            {
                if (rep.Context.SPFS_USERS.Any(u => u.UserName == usr.UserName))
                {
                    ModelState.AddModelError(PropertyHelper.ToPropertyString((UserViewModel u) => u.UserName), "User name already exists. Use a different user name.");
                }
                if (rep.Context.SPFS_USERS.Any(u => u.Email == usr.Email))
                {
                    ModelState.AddModelError(PropertyHelper.ToPropertyString((UserViewModel u) => u.Email), "Email already exists. Use a different Email.");
                }
            }


            if (ModelState.IsValid)
            {
                using (Repository rep = new Repository())
                {
                    Utilities util = new Utilities();

                    SPFS_USERS userInfo = new SPFS_USERS()
                    {
                        UserName = usr.UserName,
                        First_Name = usr.FirstName,
                        MiddleName = usr.MiddleName,
                        Last_Name = usr.LastName,
                        Email = usr.Email,
                        SPFS_Active = usr.SPFS_Active,
                        RoleID = usr.RoleID,
                        Active_Date = DateTime.Now.Date,
                        Created_date = DateTime.Now.Date,
                        Created_by = util.GetCurrentUser().UserName
                    };


                    //  TryUpdateModel(userInfo);
                    //TryUpdateModel(userInfo," " ,new string[]
                    // {
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.FirstName),
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.MiddleName),
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.LastName),
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.UserName),
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.Email),
                    //    PropertyHelper.ToPropertyString((UserViewModel u) => u.SPFS_Active),
                    // });
                    rep.Context.SPFS_USERS.Add(userInfo);
                    rep.Context.SaveChanges();

                }



            }
            EditViewBags();
            return View(usr);
        }

        public ActionResult Edit(int? userID)
        {
            EditViewBags();
            ViewData["Title"] = "Edit User";

            if (!userID.HasValue || userID <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserViewModel user = new UserViewModel();
            using (Repository rep = new Repository())
            {
                user = (from usr in rep.Context.SPFS_USERS
                        join tmprole in rep.Context.SPFS_ROLES on usr.RoleID equals tmprole.RoleID into tpRole
                        from role in tpRole.DefaultIfEmpty()
                        where usr.UserID == userID
                        select new UserViewModel()
                        {
                            UserName = usr.UserName,
                            Email = usr.Email,
                            FirstName = usr.First_Name,
                            MiddleName = usr.MiddleName,
                            LastName = usr.Last_Name,
                            RoleName = role != null ? role.RoleName.Trim() : string.Empty,
                            RoleID = usr.RoleID,
                            SPFS_Active = usr.SPFS_Active ? true : false,
                        }).FirstOrDefault();

                if (user == null)
                {
                    return HttpNotFound();
                }
            }
            return View(user);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserViewModel userViewModel)
        {
            using (Repository rep = new Repository())
            {
                if (!rep.Context.SPFS_ROLES.Any(r => r.RoleID == userViewModel.RoleID))
                {
                    ModelState.AddModelError(PropertyHelper.ToPropertyString((UserViewModel u) => u.RoleName), "Please Select a Role.");
                }

            }

            if (ModelState.IsValid)
            {
                using (Repository rep = new Repository())
                {
                    bool checkUserExist = false;

                    //TO do: business logic for validation

                    if (!checkUserExist)
                    {
                        var user = rep.Context.SPFS_USERS.Where(c => c.UserID == userViewModel.UserID).FirstOrDefault();
                        if (TryUpdateModel(user))
                        {
                            user.Modified_by = new Utilities().GetCurrentUser().UserName;
                            user.Modified_date = DateTime.Now;

                            rep.Context.SaveChanges();
                        }
                    }
                    else
                    {
                        //User existed.
                    }
                }
            }
            EditViewBags();
            return RedirectToAction("Index");
        }
        private void EditViewBags()
        {
            List<SelectListItem> roles;
            using (Repository roleRep = new Repository())
            {
                var role = (from rol in roleRep.Context.SPFS_ROLES
                            select new SelectListItem { Value = rol.RoleID.ToString(), Text = rol.RoleName });
                Utilities util = new Utilities();
                if (util.GetCurrentUser().RoleID == 3)
                {
                    role = role.Where(item => item.Value != "1");
                }
                roles = role.ToList();

            }
            ViewBag.Roles = roles;
        }
        /// <summary>
        /// Method to Get Users by Search
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="userViewModel"></param>
        /// <returns></returns>
        //private List<UserListViewModel> GetUserBySearch(int? pageno)
        //{
        //    GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
        //    objSearch.Page = pageno;
        //    TempData["UserSearchCriteria"] = objSearch;
        //    return GetUsers(objSearch);
        //}

        private List<UserListViewModel> GetUsers(GeneralSearchCriteria ObjSearch)
        {
            List<UserListViewModel> users;
            using (Repository rep = new Repository())
            {
                var user = (from usr in rep.Context.SPFS_USERS
                            join tmprole in rep.Context.SPFS_ROLES on usr.RoleID equals tmprole.RoleID into tpRole
                            from role in tpRole.DefaultIfEmpty()
                            select new UserListViewModel()
                            {
                                UserID = usr.UserID,
                                UserName = usr.UserName,
                                Email = usr.Email,
                                FirstName = usr.First_Name,
                                MiddleName = usr.MiddleName,
                                LastName = usr.Last_Name,
                                RoleName = role != null ? role.RoleName : string.Empty,
                                SPFS_Active = usr.SPFS_Active ? true : false,
                                UserSites = (from usrsite in rep.Context.SPFS_USERSITES
                                             join site in rep.Context.SPFS_SITES on usrsite.SiteID equals site.SiteID
                                             where usrsite.UserID == usr.UserID
                                             select site.Name).ToList()
                            });

                if (ObjSearch.Active.HasValue)
                {
                    if (ObjSearch.Active.Value == 1)
                    {
                        user = user.Where(item => item.SPFS_Active);
                    }
                    else if (ObjSearch.Active.Value == 0)
                        user = user.Where(item => !item.SPFS_Active);

                }
                if (!string.IsNullOrEmpty(ObjSearch.SearchText))
                {
                    user = user.Where(item => item.UserName.Contains(ObjSearch.SearchText)
                                              || item.FirstName.Contains(ObjSearch.SearchText));
                }

                users = SortByInput(ObjSearch.CurrentSort, user.ToList());
            }
            return users;
        }

        /// <summary>
        /// Method to Get Users by sort
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetUsersBySort(int? id, string search, string sortby)
        {
            int pageno = 1;
            List<UserListViewModel> userViewModel = null;
            ViewData["ActiveId"] = id.HasValue ? id.Value : 0;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
            objSearch.Active = id;
            objSearch.CurrentSort = sortby;
            objSearch.SearchText = search;
            TempData["UserSearchCriteria"] = objSearch;

            userViewModel = GetUsers(objSearch);
            pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return PartialView("_UserList", userViewModel.ToPagedList(pageno, SPFSConstants.UserPageSize));
        }
        /// <summary>
        /// Method to Sort the Users by input 
        /// </summary>
        /// <param name="sortby"></param>
        /// <param name="userViewModel"></param>
        /// <returns></returns>
        private List<UserListViewModel> SortByInput(string sortby, List<UserListViewModel> userViewModel)
        {
            ViewData["NameSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("UserName Asc") ? "UserName desc" : "UserName Asc";
            ViewData["EmailSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Email") ? "Email desc" : "Email Asc";
            ViewData["FirstNameSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("FirstName Asc") ? "FirstName desc" : "FirstName Asc";
            ViewData["LastNameSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("LastName Asc") ? "LastName desc" : "LastName Asc";
            ViewData["RoleSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("Role Asc") ? "Role desc" : "Role Asc";
            ViewData["IsActiveSort"] = String.IsNullOrEmpty(sortby) || sortby.Equals("IsActive Asc") ? "IsActive desc" : "IsActive Asc";
            ViewData["ActiveSort"] = sortby;
            switch (sortby)
            {
                case "UserName desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.UserName).ToList();
                    break;
                case "UserName Asc":
                    userViewModel = userViewModel.OrderBy(s => s.UserName).ToList();
                    break;
                case "Email desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.Email).ToList();
                    break;
                case "Email Asc":
                    userViewModel = userViewModel.OrderBy(s => s.Email).ToList();
                    break;
                case "FirstName desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.FirstName).ToList();
                    break;
                case "FirstName Asc":
                    userViewModel = userViewModel.OrderBy(s => s.FirstName).ToList();
                    break;
                case "LastName desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.LastName).ToList();
                    break;
                case "LastName Asc":
                    userViewModel = userViewModel.OrderBy(s => s.LastName).ToList();
                    break;
                case "Role desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.RoleName).ToList();
                    break;
                case "Role Asc":
                    userViewModel = userViewModel.OrderBy(s => s.RoleName).ToList();
                    break;
                case "IsActive desc":
                    userViewModel = userViewModel.OrderByDescending(s => s.SPFS_Active).ToList();
                    break;
                case "IsActive Asc":
                    userViewModel = userViewModel.OrderBy(s => s.SPFS_Active).ToList();
                    break;
                default:
                    userViewModel = userViewModel.OrderBy(s => s.LastName).ThenBy(ss => ss.FirstName).ToList();
                    break;
            }
            return userViewModel;

        }

        /// <summary>
        /// Method to retrieve Users by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetUserByActiveOrInActive(int? id, string search)
        {
            int pageno = 1;
            List<UserListViewModel> userViewModel = null;

            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
            objSearch.Active = id;
            objSearch.SearchText = search;
            userViewModel = GetUsers(objSearch);
            pageno = objSearch.Page.Value;
            TempData["UserSearchCriteria"] = objSearch;
            return PartialView("_UserList", userViewModel.ToPagedList(pageno, SPFSConstants.UserPageSize));
        }
        /// <summary>
        /// Method to retrieve Users by input 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <returns></returns>
        public ActionResult GetUserBySearch(int? id, string search)
        {
            try
            {
                int pageno = 1;
                List<UserListViewModel> userViewModel = null;

                GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
                objSearch.Active = id;
                objSearch.SearchText = search;
                userViewModel = GetUsers(objSearch);
                //pageno = objSearch.Page.Value;
                TempData["UserSearchCriteria"] = objSearch;
                return PartialView("_UserList", userViewModel.ToPagedList(pageno, SPFSConstants.UserPageSize));
            }
            catch (Exception)
            {
                Utilities util = new Utilities();
                string msg = "error occured while loading users information";
                throw new Exception(util.GetDivElements(msg, "alert alert-danger", "Error ! "));


            }

        }
        /// <summary>
        /// Method to retrieve users by page
        /// </summary>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="sortby"></param>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public ActionResult SearchByPage(int? page)
        {

            CreateViewBags();
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
            objSearch.Page = page.HasValue ? page : objSearch.Page;
            TempData["UserSearchCriteria"] = objSearch;

            List<UserListViewModel> userViewModel = null;
            userViewModel = GetUsers(objSearch);
            userViewModel = SortByInput(objSearch.CurrentSort, userViewModel);
            int pageno = objSearch.Page.HasValue ? objSearch.Page.Value : 1;
            return View("Index", userViewModel.ToPagedList(pageno, SPFSConstants.UserPageSize));
        }

        public ActionResult GetUsersbyLastNameFromAD(string lastName)
        {

            List<UserViewModel> user = new List<UserViewModel>();

            user = new UserADService().ADUserSearch(lastName);

            if (user.Count > 0)
                TempData["AdUsers"] = user;
            return PartialView("_listofSearchedADUsers", user);
        }
        //public ActionResult FillUserInfoFromAD(string userName)
        //{
        //    CreateViewBags();
        //    UserViewModel user = null;
        //    if (TempData["AdUsers"] != null)
        //    {
        //        List<UserViewModel> adResult = (List<UserViewModel>)TempData["AdUsers"];
        //        if (adResult.Count > 0)
        //        {
        //            TempData["AdUsers"] = adResult;
        //            user = adResult.Where(item => item.UserName.Equals(userName)).FirstOrDefault();
        //        }
        //    }
        //    UserViewModel userViewModel = new UserViewModel();

        //        userViewModel = new UserViewModel() { UserName = user.UserName, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, SPFS_Active = true };

        //    return View("Create", userViewModel);
        //}

        /// <summary>
        /// Used to Export Items to Excel.
        /// </summary>
        /// <param name="fileName"></param>
        public void ExportData(string fileName)
        {
            GeneralSearchCriteria objSearch = (GeneralSearchCriteria)TempData["UserSearchCriteria"];
            TempData.Keep("UserSearchCriteria");
           
            using (Repository ExpRep = new Repository())
            {
                var result = (from usrsite in ExpRep.Context.SPFS_USERSITES
                              join site in ExpRep.Context.SPFS_SITES on usrsite.SiteID equals site.SiteID
                              join user in ExpRep.Context.SPFS_USERS on usrsite.UserID equals user.UserID
                              join tmprole in ExpRep.Context.SPFS_ROLES on user.RoleID equals tmprole.RoleID
                              select new UserExportViewModel
                              {
                                  UserID = usrsite.UserID,
                                  UserName = user.UserName,
                                  FirstName = user.First_Name,
                                  LastName = user.Last_Name,
                                  MiddleName = user.MiddleName,
                                  RoleName = tmprole.RoleName.Trim(),
                                  Email = user.Email,
                                  SPFS_Active = user.SPFS_Active,
                                  UserSiteName = site.Name
                              });

                if (objSearch.Active.HasValue)
                {
                    if (objSearch.Active.Value == 1)
                    {
                        result = result.Where(item => item.SPFS_Active);
                    }
                    else if (objSearch.Active.Value == 0)
                        result = result.Where(item => !item.SPFS_Active);

                }
                if (!string.IsNullOrEmpty(objSearch.SearchText))
                {
                    result = result.Where(item => item.UserName.Contains(objSearch.SearchText)
                                              || item.FirstName.Contains(objSearch.SearchText));
                }

                base.ExportToCSV(result.ToList().GetCSV(), fileName + DateTime.Now.ToShortDateString());

            }

           
            
            }
        }
    }