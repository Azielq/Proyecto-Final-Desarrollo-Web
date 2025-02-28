using System.Web;
using System.Web.Optimization;

namespace Proyecto_Final_Desarrollo_Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/libs").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/react-bootstrap.js",
                      "~/Scripts/react-bootstrap-table.js",
                      "~/Scripts/bootstrap.bundle.min.js",
                      "~/Scripts/sweetalert2.all.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/react-bootstrap-table.min.css",
                       "~/Content/bootstrap-icons/bootstrap-icons.css",
                        "~/Content/bootstrap-icons/bootstrap-icons.min.css",
                      "~/Content/site.css"));
        }
    }
}
