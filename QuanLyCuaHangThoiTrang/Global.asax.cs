using QuanLyCuaHangThoiTrang.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace QuanLyCuaHangThoiTrang
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer<QuanLyCuaHangThoiTrangDbContext>(new DropCreateDatabaseIfModelChanges<QuanLyCuaHangThoiTrangDbContext>());

        }
      
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            var AccCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (AccCookie != null)
            {
                var authTicket = FormsAuthentication.Decrypt(AccCookie.Value);
                var Aut = authTicket.UserData.Split(new Char[] { ',' });
                var userPrincipal = new GenericPrincipal(new GenericIdentity(authTicket.Name), Aut);
                Context.User = userPrincipal;
            }
        }
    }
}
