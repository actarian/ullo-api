using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Ullo.Tasks;

using System.Web;


[assembly: OwinStartup(typeof(Ullo.Startup))]

namespace Ullo
{
    public class Settings
    {
        public static string AzureHostName { get; private set; }
        public static string ApiFolder { get; private set; }
        public static string MediaFolder { get; private set; }
        public static string UploadFolder { get; private set; }
        public static string ShareFolder { get; private set; }
        public static int MediaXs { get; private set; }
        public static int MediaSm { get; private set; }
        public static int MediaMd { get; private set; }
        public static int MediaLg { get; private set; }
        public static int MediaXl { get; private set; }
        public static string CorsOrigins { get; private set; }
        public static string CorsMethods { get; private set; }
        public static string CorsHeaders { get; private set; }
        public static string CorsExposedHeaders { get; private set; }

        public static void Init()
        {

            String websiteSiteName = Environment.ExpandEnvironmentVariables("%WEBSITE_SITE_NAME%");

            Settings.AzureHostName = String.Format(ConfigurationManager.AppSettings["AzureHostName"], websiteSiteName);

            Settings.ApiFolder = ConfigurationManager.AppSettings["ApiFolder"];
            Settings.MediaFolder = ConfigurationManager.AppSettings["MediaFolder"];
            Settings.UploadFolder = ConfigurationManager.AppSettings["UploadFolder"];
            Settings.ShareFolder = ConfigurationManager.AppSettings["ShareFolder"];

            int xs = 50, sm = 100, md = 350, lg = 640, xl = 1660;
            Int32.TryParse(ConfigurationManager.AppSettings["MediaXs"], out xs);
            Int32.TryParse(ConfigurationManager.AppSettings["MediaSm"], out sm);
            Int32.TryParse(ConfigurationManager.AppSettings["MediaMd"], out md);
            Int32.TryParse(ConfigurationManager.AppSettings["MediaLg"], out lg);
            Int32.TryParse(ConfigurationManager.AppSettings["MediaXl"], out xl);

            Settings.MediaXs = xs;
            Settings.MediaSm = sm;
            Settings.MediaMd = md;
            Settings.MediaLg = lg;

            Settings.CorsOrigins = ConfigurationManager.AppSettings["CorsOrigins"].Replace(" ", "");
            Settings.CorsMethods = ConfigurationManager.AppSettings["CorsMethods"].Replace(" ", "");
            Settings.CorsHeaders = ConfigurationManager.AppSettings["CorsHeaders"].Replace(" ", "");
            Settings.CorsExposedHeaders = ConfigurationManager.AppSettings["CorsExposedHeaders"].Replace(" ", "");

        }
    }

    /*
     * OWIN STARTUP
     * Owin uses the software design principle of Convention over configuration 
     * to assume that your project has a class named Startup, with a method named Configuration, 
     * that accepts one parameter of the type IAppBuilder, 
     * which will add components into the Owin application pipeline.
     * */
    public partial class Startup {

        public void Configuration(IAppBuilder app)
        {

            Settings.Init();

            ConfigureAuth(app);

            if (ConfigurationManager.AppSettings["HangfireEnabled"] == "1")
            {
                String currentConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["UlloContext"].ConnectionString;
                GlobalConfiguration.Configuration.UseSqlServerStorage(currentConnectionString);

                app.UseHangfireDashboard("/hangfire", new DashboardOptions {
                    AuthorizationFilters = new[] { new CustomOwinAuthorizationFilter() }
                });
                app.UseHangfireServer();

                Schedule.Start();
            } 
            
        }
    }

    public class CustomOwinAuthorizationFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            var context = new OwinContext(owinEnvironment);
            var user = context.Authentication.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated; // && user.IsInRole("Admin");
        }
    }

}
