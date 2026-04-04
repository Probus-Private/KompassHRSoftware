using System.Web;
using System.Web.Optimization;

namespace KompassHRESS
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Core jQuery and validation
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Modernizr
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Bootstrap and respond.js
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            // CSS Styles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/assets/css/default/app.min.css"));

            // Custom script bundle including all required plugins and custom JS
            bundles.Add(new ScriptBundle("~/bundles/MyJqueryJs").Include(
                      "~/assets/plugins/Loader/Loader.js",
                      "~/assets/plugins/sweetalert/dist/sweetalert.min.js",
                      "~/assets/plugins/parsleyjs/dist/parsley.min.js",
                      "~/assets/SweetAlert/sweetalert2.all.min.js",
                      "~/assets/js/vendor.min.js",
                      "~/assets/js/app.min.js",

                      // Flot charts
                      "~/assets/plugins/flot/source/jquery.canvaswrapper.js",
                      "~/assets/plugins/flot/source/jquery.colorhelpers.js",
                      "~/assets/plugins/flot/source/jquery.flot.js",
                      "~/assets/plugins/flot/source/jquery.flot.saturated.js",
                      "~/assets/plugins/flot/source/jquery.flot.browser.js",
                      "~/assets/plugins/flot/source/jquery.flot.drawSeries.js",
                      "~/assets/plugins/flot/source/jquery.flot.uiConstants.js",
                      "~/assets/plugins/flot/source/jquery.flot.time.js",
                      "~/assets/plugins/flot/source/jquery.flot.resize.js",
                      "~/assets/plugins/flot/source/jquery.flot.pie.js",
                      "~/assets/plugins/flot/source/jquery.flot.crosshair.js",
                      "~/assets/plugins/flot/source/jquery.flot.categories.js",
                      "~/assets/plugins/flot/source/jquery.flot.navigate.js",
                      "~/assets/plugins/flot/source/jquery.flot.touchNavigate.js",
                      "~/assets/plugins/flot/source/jquery.flot.hover.js",
                      "~/assets/plugins/flot/source/jquery.flot.touch.js",
                      "~/assets/plugins/flot/source/jquery.flot.selection.js",
                      "~/assets/plugins/flot/source/jquery.flot.symbol.js",
                      "~/assets/plugins/flot/source/jquery.flot.legend.js",

                      // Datepicker
                      "~/assets/plugins/bootstrap-datepicker/dist/js/bootstrap-datepicker.js",

                      // Dashboard JS
                      "~/assets/js/demo/dashboard-v3.js",

                      // Vector Map
                      "~/assets/plugins/jvectormap-next/jquery-jvectormap.min.js",
                      "~/assets/plugins/jvectormap-content/world-mill.js",
                      "~/assets/js/demo/map-vector.demo.js",

                      // ChartJS
                      "~/assets/plugins/chart.js/dist/chart.min.js",
                      "~/assets/js/demo/chart-js.demo.js",

                      // DataTables core and extensions
                      "~/assets/plugins/datatables.net/js/jquery.dataTables.min.js",
                      "~/assets/plugins/datatables.net-bs5/js/dataTables.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-responsive/js/dataTables.responsive.min.js",
                      "~/assets/plugins/datatables.net-responsive-bs5/js/responsive.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-colreorder/js/dataTables.colReorder.min.js",
                      "~/assets/plugins/datatables.net-colreorder-bs5/js/colReorder.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-keytable/js/dataTables.keyTable.min.js",
                      "~/assets/plugins/datatables.net-keytable-bs5/js/keyTable.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-rowreorder/js/dataTables.rowReorder.min.js",
                      "~/assets/plugins/datatables.net-rowreorder-bs5/js/rowReorder.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-select/js/dataTables.select.min.js",
                      "~/assets/plugins/datatables.net-buttons/js/dataTables.buttons.min.js",
                      "~/assets/plugins/datatables.net-buttons-bs5/js/buttons.bootstrap5.min.js",
                      "~/assets/plugins/datatables.net-buttons/js/buttons.colVis.min.js",
                      "~/assets/plugins/datatables.net-buttons/js/buttons.flash.min.js",
                      "~/assets/plugins/datatables.net-buttons/js/buttons.html5.min.js",
                      "~/assets/plugins/datatables.net-buttons/js/buttons.print.min.js",

                      // PDF & ZIP export support
                      "~/assets/plugins/pdfmake/build/pdfmake.min.js",
                      "~/assets/plugins/pdfmake/build/vfs_fonts.js",
                      "~/assets/plugins/jszip/dist/jszip.min.js",

                      // Tags
                      "~/assets/plugins/tag-it/js/tag-it.min.js"
            ));

            // Enable bundling and minification
            BundleTable.EnableOptimizations = true;
        }
    }
}
