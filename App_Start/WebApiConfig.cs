using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Ullo
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // ENABLING CORS
            /*
            var cors = new EnableCorsAttribute("localhost:8081", "*", "*", "X-Pagination");
            cors.SupportsCredentials = true;
            config.EnableCors(cors); 
            */

            // config.EnableCors();
                        
            // config.EnableCors(new EnableCorsAttribute("*", "*", "GET, POST, PUT, DELETE, OPTIONS"));
            // Access-Control-Allow-Headers: Accept, Origin, Content-Type
            // Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
            
            // Web API configuration and services
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            // settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // ignore null values;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Use camel case for JSON data.
            config.Formatters.Add(new BrowserJsonFormatter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            /*
            config.Routes.MapHttpRoute(
                name: "WelcomeApi",
                routeTemplate: "api/",
                defaults: new { controller = "Welcome", id = RouteParameter.Optional }
            ); 
            */
        }
    }

    public class BrowserJsonFormatter : JsonMediaTypeFormatter
    {
        public BrowserJsonFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            // this.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; // pretty print;
            this.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; // ignore null values;
            this.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            this.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            this.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;            
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
    }

}
