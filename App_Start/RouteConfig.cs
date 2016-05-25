
using System.Web.Mvc;
using System.Web.Routing;

namespace Ullo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            /*
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            */
            
            routes.MapRoute(
                "Default",
                "{*catchall}",
                new { controller = "App", action = "Index", code = UrlParameter.Optional }
            );

        }
    }

}