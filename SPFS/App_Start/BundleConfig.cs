using System.Web;
using System.Web.Optimization;

namespace SPFS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            //             "~/Scripts/jquery-ui.js"                        
            //          ));


            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/jquery-ui-{version}.js",
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/jquery.bootstrap-duallistbox.js",
                       "~/Scripts/bootstrap-select.js",
                      "~/Scripts/respond.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/themes/base/jquery-ui.css",
                      "~/Content/themes/smoothness/jquery-ui-1.8.23.custom.css",
                      "~/Content/bootstrap-duallistbox.css",
                       "~/Content/bootstrap-select.css",
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/tooltip").Include(
                       "~/Scripts/tooltip.js"));
            //bundles.Add(new ScriptBundle("~/bundles/bootstrapSelect").Include(
            //           "~/Scripts/bootstrap-select.js",
            //             "~/Scripts/bootstrap-select.min.js"
            //      ));
        }
    }
}
