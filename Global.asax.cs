using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

using System.Threading;
using System.Globalization;
using System.Web.Mvc;

namespace Ullo
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");

            GlobalConfiguration.Configure(WebApiConfig.Register);
            
            AreaRegistration.RegisterAllAreas();

            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }
    }
}
