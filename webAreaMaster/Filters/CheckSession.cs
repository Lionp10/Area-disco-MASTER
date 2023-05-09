using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace webAreaMaster.Filters
{
    public class CheckSession : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["Session"] != null)
            {
                return true;
            }
            else
            {
                httpContext.Response.Redirect("~/Acceso/Login");
                return false;
            }
        }
    }
}