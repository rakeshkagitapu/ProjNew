
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;
using SPFS.Models;
using SPFS.DAL;
using SPFS.Model;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Web.UI;
using System.Globalization;
using System.Text;

namespace SPFS.Helpers
{
    /// <summary>
    /// Class Utilities.
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// The admin_ role
        /// </summary>
        public const string ADMIN_ROLE = "Admin";
        /// <summary>      
        public const string AUTHOR_ROLE = "Author";

        public const string SERVICEDESK_ROLE = "Service Desk";

        public const string APPROVED_ROLES = "Admin,Author,Service Desk";
        /// <summary>
        /// The edit _ user  roles
        /// </summary>
        public const string ADMIN_OFFICE_USERS_ROLES = "Admin,Service Desk";

        /// <summary>
        ///Allowed Author controller
        /// </summary>
        public const string AlLOW_ACCESS_AUTHOR_CONTROLLERS = "Home,Supplier,Site,Account";

        /// <summary>
        ///Allowed Admin controller
        /// </summary>
        public const string AlLOW_ACCESS_ADMIN_CONTROLLERS = "Home,Supplier,Site,User,Account";

        /// <summary>
        ///Allowed ServiceDesk controller
        /// </summary>
        public const string AlLOW_ACCESS_SERVICEDESK_CONTROLLERS = "Home,User,Account,UserSites";

        /// <summary>
        /// The cannot_ fin d_ user
        /// </summary>
        public const string CANNOT_FIND_USER = "Cannot find user. Please contact administrator.";

        /// <summary>
        /// The cannot_ fin d_ user
        /// </summary>
        public const string NO_RESULT = "No results found. Please contact administrator.";

        public enum Excelfields
        {
            CID,
            DUNS,
            ERP_SUPPLIER_ID,
            Inbound
        }
        // public const List<string> Excelfields = new List<string>(){ "CID", "DUNS", "ERP_SUPPLIER_ID", "Inbound", "OnTime Quantity", "OnTime Quantity Due","Premium Freight" };
        /// <summary>
        /// The _current user name
        /// </summary>
        //private string _currentUserName = "";
        /// <summary>
        /// Gets or sets the name of the current user.
        /// </summary>
        /// <value>The name of the current user.</value>
        public string CurrentUserName
        {
            get
            {
                if (HttpContext.Current.User == null || HttpContext.Current.User.Identity == null || String.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                    throw new Exception("Current User is null");

                return StripDomainName(HttpContext.Current.User.Identity.Name);
            }
        }


        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns>User.</returns>
        /// <exception cref="Exception"></exception>
        public SPFS_USERS GetCurrentUser()
        {
            SPFS_USERS user = null;
            using (Repository rep = new Repository())
            {
                try
                {

                    user = rep.Context.SPFS_USERS.Where(p => p.UserName == CurrentUserName).FirstOrDefault();

                }
                catch (Exception ex)
                {

                    throw;
                }



            }
            return user;
        }

        /// <summary>
        /// Gets the roles CSV.
        /// </summary>
        /// <param name="roles">The roles.</param>
        /// <returns>System.String.</returns>
        public string GetRolesCSV(string[] roles)
        {
            return String.Join(",", roles);
        }

        /// <summary>
        /// Determines whether [is current user in role] [the specified role].
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns><c>true</c> if [is current user in role] [the specified role]; otherwise, <c>false</c>.</returns>
        public bool IsCurrentUserInRole(string role)
        {
            string roles = GetRolesForCurrentUser();

            if (roles != null)
                return roles.Equals(role, StringComparison.CurrentCultureIgnoreCase);
            else
                return false;
        }

        /// <summary>
        /// Gets the roles for current user.
        /// </summary>
        /// <returns>System.String[].</returns>
        public string GetRolesForCurrentUser(string user = "")
        {
            try
            {
                string userName = string.Empty;

                if (!string.IsNullOrEmpty(user))
                {
                    userName = StripDomainName(user);
                }
                else
                {
                    userName = CurrentUserName;
                }
                using (Repository rep = new Repository())
                {
                    return rep.Context.SPFS_USERS.Include("SPFS_ROLES").Where(u => u.UserName == userName).FirstOrDefault().SPFS_ROLES.RoleName.Trim();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Strips the name of the domain.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="Exception">Invalid username.</exception>
        private string StripDomainName(string userName)
        {
            if (!String.IsNullOrEmpty(userName))
                if (userName.Contains('\\'))
                    return userName.Split('\\')[1];
                else
                    return userName;
            else
            {
                throw new Exception("Invalid username. - " + userName + " -");
            }
        }

        /// <summary>
        /// Determines whether this instance is admin.
        /// </summary>
        /// <returns><c>true</c> if this instance is admin; otherwise, <c>false</c>.</returns>
        public bool IsAdmin()
        {
            return IsCurrentUserInRole(Utilities.ADMIN_ROLE);
        }

        /// <summary>
        /// Gets the person location identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        //public int GetPersonLocationID()
        //{
        //    DVS_PCardContext db = new DVS_PCardContext();
        //    int locationID = db.Users.Where(p => p.ADUserID == CurrentUserName).First().LocationID.HasValue ? db.Users.Where(p => p.ADUserID == CurrentUserName).First().LocationID.Value : 0;
        //    return locationID;
        //}

        /// <summary>
        /// Gets the application minimum date.
        /// </summary>
        /// <returns>DateTime.</returns>
        public static DateTime GetApplicationMinDate()
        {
            return Convert.ToDateTime(ConfigurationManager.AppSettings["ApplicationMinDate"]);
        }

        /// <summary>
        /// Creates the entity with values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <returns>T.</returns>
        public static T CreateEntityWithValues<T>(DbPropertyValues values) where T : new()
        {
            T entity = new T();
            Type type = typeof(T);

            foreach (var name in values.PropertyNames)
            {
                var property = type.GetProperty(name);
                property.SetValue(entity, values.GetValue<object>(name));
            }
            return entity;
        }

        /// <summary>
        /// Gets the inner most exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns>Exception.</returns>
        public static Exception GetInnerMostException(Exception ex)
        {
            Exception innerException = ex;
            while (innerException.InnerException != null)
            {
                innerException = innerException.InnerException;
            }

            return innerException;
        }

        public string GetDivElements(string msg, string classvalue, string text)
        {
            // Initialize StringWriter instance.
            StringWriter stringWriter = new StringWriter();

            // Put HtmlTextWriter in using block because it needs to call Dispose.
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {

                // Some strings for the attributes.
                string classValue = classvalue;  // "alert alert-danger";
                                                 // string roleValue = "alert";

                // The important part:
                writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
                writer.RenderBeginTag(HtmlTextWriterTag.Div); // Begin #1


                writer.RenderBeginTag(HtmlTextWriterTag.Strong); // Begin #2
                                                                 //writer.Write("Error ! ");
                writer.Write(text);
                writer.RenderEndTag(); // End #2

                writer.Write(msg);

                writer.RenderEndTag(); // End #1
            }
            // Return the result.
            return stringWriter.ToString();
        }
        public List<SelectListItem> GetMonths(bool isUpload)
        {
            List<SelectListItem> months = new List<SelectListItem>();
            if (isUpload)
            {

                var uploadMonths = Enumerable.Range(0, 3).Select(i => DateTime.Now.AddMonths(i - 3).ToString("MMMM")).Distinct();

                foreach( var  month in uploadMonths)
                {
                    var monthValue = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;
                    months.Add(new SelectListItem { Value = monthValue.ToString(), Text = month.ToString() });
                }
            }
            else
            {
                var manualMonths = Enumerable.Range(0, 12).Select(i => DateTime.Now.AddMonths(i - 12).ToString("MMMM")).Distinct();

                foreach (var month in manualMonths)
                {
                    var monthValue = DateTime.ParseExact(month, "MMMM", CultureInfo.InvariantCulture).Month;
                    months.Add(new SelectListItem { Value = monthValue.ToString(), Text = month.ToString() });
                }
            }
           
            return months;
        }

        public List<SelectListItem> GetYears(bool isUpload)
        {
            List<SelectListItem> years = new List<SelectListItem>();
            if (isUpload)
            {

                var uploadYears = Enumerable.Range(0, 3).Select(i => DateTime.Now.AddMonths(i - 3).ToString("yyyy")).Distinct();

                foreach (var year in uploadYears)
                {
                    years.Add(new SelectListItem { Value = year.ToString(), Text = year.ToString() });
                }
            }
            else
            {
                var manualYears = Enumerable.Range(0, 12).Select(i => DateTime.Now.AddMonths(i - 12).ToString("yyyy")).Distinct();

                foreach (var year in manualYears)
                {
                    years.Add(new SelectListItem { Value = year.ToString(), Text = year.ToString() });
                }
            }
            return years;
        }
       
        //public List<SelectListItem> GetMonths(bool isUpload)
        //{
        //    List<SelectListItem> months = new List<SelectListItem>();
        //    if (isUpload)
        //    {

        //        string[] localizedMonths = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.MonthNames;

        //        for (int month = 0; month < 12; month++)
        //        {
        //            // SelectListItem monthListItem = new SelectListItemlocalizedMonths[month], invariantMonths[month]);
        //            months.Add(new SelectListItem { Value = (month + 1).ToString(), Text = localizedMonths[month] });
        //        }
        //    }
        //    return months;
        //}

        //public List<SelectListItem> GetYears()
        //{
        //    List<SelectListItem> years = new List<SelectListItem>();
        //    int year = DateTime.Now.Year;
        //    int yearsAdd = year + 5;
        //    int yearsSub = year - 5;
        //    for (int i = yearsSub; i < yearsAdd; i++)
        //    {
        //        years.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
        //    }
        //    return years;
        //}

    }


    [DirectoryObjectClass("user")]
    [DirectoryRdnPrefix("CN")]
    public class CustomUserPrincipal : UserPrincipal
    {
        public CustomUserPrincipal(PrincipalContext context)
            : base(context) { }

        [DirectoryProperty("anr")]
        public string Anr
        {
            get { return (string)ExtensionGet("anr")[0]; }
            set { ExtensionSet("anr", value); }
        }
    }

    public class UserADService
    {
        //private string path = string.Empty;
        private string domain1 = string.Empty;
        private string domain2 = string.Empty;
        private string domain3 = string.Empty;
        private string domain4 = string.Empty;
        //private string username = string.Empty;
        //private string pwd = string.Empty;

        public UserADService()
        {
            // path = ConfigurationManager.AppSettings["LDAPPath"];

            //path = ConfigurationManager.AppSettings["LDAPPath"];
            domain1 = ConfigurationManager.AppSettings["Domain1"];
            domain2 = ConfigurationManager.AppSettings["Domain2"];
            domain3 = ConfigurationManager.AppSettings["Domain3"];
            domain4 = ConfigurationManager.AppSettings["Domain4"];
            //username = ConfigurationManager.AppSettings["LDAPUserName"];
            //pwd = ConfigurationManager.AppSettings["LDAPPassword"];
        }

        public List<UserViewModel> ADUserSearch(string userName)
        {
            var users = new List<UserViewModel>();

            int i = 0;
            switch (i)
            {
                case 0:
                    {
                        var usrs = new List<UserViewModel>();
                        usrs = DomainSearch(userName, domain1);
                        users.AddRange(usrs);
                    }
                    goto case 1;
                case 1:
                    {
                        var usrs = new List<UserViewModel>();
                        usrs = DomainSearch(userName, domain2);
                        users.AddRange(usrs);
                    }
                    goto case 2;
                case 2:
                    {
                        var usrs = new List<UserViewModel>();
                        usrs = DomainSearch(userName, domain3);
                        users.AddRange(usrs);
                    }
                    goto case 3;
                case 3:
                    {
                        var usrs = new List<UserViewModel>();
                        usrs = DomainSearch(userName, domain4);
                        users.AddRange(usrs);
                    }
                    break;
                default:
                    break;
            }


            return users;

        }

        private List<UserViewModel> DomainSearch(string userName, string domain)
        {
            List<UserViewModel> usrs = new List<UserViewModel>();
            using (var domainContext = new PrincipalContext(ContextType.Domain, domain))
            {
                var searchParams = new CustomUserPrincipal(domainContext);
                searchParams.Anr = userName;
                var ps = new PrincipalSearcher();
                ps.QueryFilter = searchParams;

                var results = ps.FindAll();

                foreach (UserPrincipal user in results)
                {
                    var adUser = new UserViewModel();

                    adUser.FirstName = user.GivenName;
                    adUser.MiddleName = String.IsNullOrEmpty(user.MiddleName) ? String.Empty : user.MiddleName.Substring(0, 1);
                    adUser.LastName = user.Surname;
                    adUser.Email = user.EmailAddress;
                    adUser.UserName = user.SamAccountName;

                    usrs.Add(adUser);
                }
            }
            return usrs;
        }
    }
    //public class UserADService
    //{
    //    private string path = string.Empty;
    //    private string domain = string.Empty;
    //    private string username = string.Empty;
    //    private string pwd = string.Empty;

    //    public UserADService()
    //    {           
    //        // path = ConfigurationManager.AppSettings["LDAPPath"];

    //        path = ConfigurationManager.AppSettings["LDAPPath"];
    //        domain = ConfigurationManager.AppSettings["Domain"];
    //        username = ConfigurationManager.AppSettings["LDAPUserName"];
    //        pwd = ConfigurationManager.AppSettings["LDAPPassword"];
    //    }

    //    public List<UserViewModel> ADUserSearch(string userName)
    //    {
    //        var users = new List<UserViewModel>();

    //        using (var domainContext = new PrincipalContext(ContextType.Domain, domain))
    //        {
    //            var searchParams = new CustomUserPrincipal(domainContext);
    //            searchParams.Anr = userName;
    //            var ps = new PrincipalSearcher();
    //            ps.QueryFilter = searchParams;

    //            var results = ps.FindAll();

    //            foreach (UserPrincipal user in results)
    //            {
    //                var adUser = new UserViewModel();

    //                adUser.FirstName = user.GivenName;
    //                adUser.MiddleName = String.IsNullOrEmpty(user.MiddleName) ? String.Empty : user.MiddleName.Substring(0, 1);
    //                adUser.LastName = user.Surname;
    //                adUser.Email = user.EmailAddress;
    //                adUser.UserName = user.SamAccountName;

    //                users.Add(adUser);
    //            }
    //        }

    //        return users;

    //    }

    //}
}