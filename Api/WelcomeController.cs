using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Cors; // we import the http cors service for enabling cross-domain

namespace Ullo.Api
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")] // we enable the cors for the whole controller, in production change origins * with client url http://mywebclient.azurewebsites.net

    public class WelcomeController : ApiController
    {
        [Route("api/welcome", Name = "Welcome")]
        [HttpGet] // we limit this method to receive calls only from GET VERB with the attribute [HttpGet]
        public HttpResponseMessage Welcome()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new {
                Api = "Ullo",
                Version = "1.0",
                Description = "Ullo WebApi2 for Ullo App",
                Author = "Websolute",
                Settings = new {
                    Folders = new {
                        Media = Settings.MediaFolder,
                        Upload = Settings.UploadFolder,
                        Share = Settings.ShareFolder
                    },
                    Media = new {
                        Xs = Settings.MediaXs,
                        Sm = Settings.MediaSm,
                        Md = Settings.MediaMd,
                        Lg = Settings.MediaLg
                    }
                }
            });
        }
    }
}
