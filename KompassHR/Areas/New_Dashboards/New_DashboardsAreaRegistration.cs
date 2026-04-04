using System.Web.Mvc;

namespace KompassHR.Areas.New_Dashboards
{
    public class New_DashboardsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "New_Dashboards";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "New_Dashboards_default",
                "New_Dashboards/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}