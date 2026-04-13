using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Filter
{
    public class LoginAuthentication : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if(!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
        //public void OnAuthorizationChallange(AuthorizationContext filterContext)
        //{
        //    if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
        //    {
        //        filterContext.Result = new ViewResult();
        //        //{
        //        //    ViewName = "Login";
        //        //};
        //    }
        //}
    }
}