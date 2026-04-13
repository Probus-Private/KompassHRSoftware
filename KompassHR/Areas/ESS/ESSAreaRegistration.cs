using System.Web.Mvc;

namespace KompassHR.Areas.ESS
{
    public class ESSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ESS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ESS_default",
                "ESS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}