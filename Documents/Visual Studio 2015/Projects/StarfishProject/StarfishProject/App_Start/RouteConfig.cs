using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StarfishProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Automate", action = "Index", id = UrlParameter.Optional }
            );

           /* routes.MapRoute(
                name: "data",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Automate", action = "GetTable", id = UrlParameter.Optional }
            );*/
            
        }
    }
}
