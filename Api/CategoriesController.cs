using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Caching;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

using System.Data.Entity;
using System.Web.Http.Cors; // we import the http cors service for enabling cross-domain

using Ullo.Models;
using Ullo.Api.Views;

namespace Ullo.Api
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")] // we enable the cors for the whole controller, in production change origins * with client url http://mywebclient.azurewebsites.net
    public class CategoriesController : CachedController
    {        
        [Route("api/categories", Name = "GetCategories")]
        [HttpGet]
        public HttpResponseMessage GetCategories()
        {
            CategoryView[] categories = null;
            if (!HttpContext.Current.IsDebuggingEnabled && cache.Get("Categories.Get") != null)
            {
                categories = cache.Get("Categories.Get") as CategoryView[];
                return Request.CreateResponse(HttpStatusCode.OK, categories);
            }
            var items = db.Categories.Select(x=> new CategoryView {
                Id = x.Id,
                Name = x.Name,
            }).ToList();
            categories = items.ToArray();
            cache.Add("Categories.Get", categories, policy);
            return Request.CreateResponse(HttpStatusCode.OK, categories);
        }

        [Route("api/categories/{id}", Name = "GetCategoryById")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCategoryById(int? id)
        {
            if (id == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var item = await db.Categories.Select(x => new CategoryView {
                Id = x.Id,
                Name = x.Name,
            }).SingleOrDefaultAsync(i => i.Id == (int)id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

    }
}