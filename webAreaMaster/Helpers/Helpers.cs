using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using webAreaMaster.Models;

namespace webAreaMaster.Helpers
{
    public static class Helpers
    {
        public static bool ItsAuthorized(this HtmlHelper helper, params string[] allowedroles)
        {
            var userID = HttpContext.Current.Session["Session"];
            if (userID != null)
            {
                using (var context = new sistemaAreaMasterEntities())
                {
                    var roleData = (from u in context.usuarioData
                                    join r in context.usuarioRolData on u.userRolID equals r.rolID
                                    where u.userID == (int)userID
                                    select new
                                    {
                                        r.rolNombre
                                    }).FirstOrDefault();

                    foreach (var role in allowedroles)
                    {
                        if (role.ToUpper() == roleData.rolNombre.ToUpper()) return true;
                    }
                }
            }
            bool authorize = false;
            return authorize;
        }
    }

    public static class IdentityExtensions
    {
        public static string GetUserId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? claim.Value : null;
        }
    }

}