using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.IO;
using Newtonsoft;

using System.Web.Http.Cors; // we import the http cors service for enabling cross-domain

using Ullo.Models;
using Ullo.Api.Views;

namespace Ullo.Api
{
    // [EnableCors(origins: "*", headers: "*", methods: "*")] // we enable the cors for the whole controller, in production change origins * with client url http://mywebclient.azurewebsites.net

    public class PicturesController : BaseController
    {
        [Route("api/pictures/", Name = "PicturesPaged")]
        [HttpGet]
        public HttpResponseMessage PicturesPaged([FromUri] FilterRequestView request)
        {
            if (request == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            var items = from i in db.Pictures
                        select new PictureView
                        {
                            Id = i.Id,
                            Guid = i.Guid,
                            Name = i.Name,
                            Created = i.Created,
                        };
            if (request.SearchList != null)
            {
                foreach (FilterSearchItemView s in request.SearchList)
                {
                    switch (s.Name)
                    {
                        case "name":
                            items = items.Where(r => r.Name != null && r.Name.Contains(s.Value));
                            break;
                    }
                }
            }
            items = items.OrderByDescending(r => r.Created);
            var pagedItems = getPage("PicturesPaged", items, request);
            return Request.CreateResponse(HttpStatusCode.OK, pagedItems);
        }

        [Route("api/pictures/", Name = "PicturesAdd")]
        [HttpPost]
        public async Task<HttpResponseMessage> PicturesAdd()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }
            try
            {
                var provider = GetMultipartProvider();
                var result = await Request.Content.ReadAsMultipartAsync(provider);
                List<Picture> assets = new List<Picture>();
                foreach (MultipartFileData mfd in result.FileData)
                {
                    // On upload, files are given a generic name like "BodyPart_26d6abe1-3ae1-416a-9429-b35f15e6e5d5"
                    // so this is how you can get the original file name
                    var originalFileName = GetDeserializedFileName(mfd);
                    // uploadedFileInfo object will give you some additional stuff like file length,
                    // creation time, directory name, a few filesystem methods etc..
                    var uploadedFileInfo = new FileInfo(mfd.LocalFileName);
                    string mimeType = mfd.Headers.ContentType.MediaType;
                    var asset = Picture.getPictureWithInfos(originalFileName, mimeType, uploadedFileInfo);
                    assets.Add(asset);
                    db.Pictures.Add(asset);
                    // Through the request response you can return an object to the Angular controller
                    // You will be able to access this in the .success callback through its data attribute
                    // If you want to send something to the .error callback, use the HttpStatusCode.BadRequest instead
                }
                if (provider.FormData.AllKeys.Contains("uploadData"))
                {
                    string val = Uri.UnescapeDataString(provider.FormData.GetValues("uploadData").FirstOrDefault() ?? String.Empty);
                    UploadDataModel upload = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadDataModel>(val);
                    switch (upload.Model)
                    {
                        case "Dish":
                            var item = await db.Dishes.FindAsync(upload.Id);
                            if (item != null)
                            {
                                foreach (Picture m in assets)
                                {
                                    item.Pictures.Add(m);
                                }
                                await db.SaveChangesAsync();
                            }
                            break;
                    }
                }
                await db.SaveChangesAsync();
                return this.Request.CreateResponse(HttpStatusCode.OK, assets);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/pictures/{id}", Name = "PicturesGet")]
        [HttpGet]
        public async Task<HttpResponseMessage> PicturesGet(int? id)
        {
            if (id == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            Picture item = await db.Pictures.SingleOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var itemView = new PictureEditView(item);

            return Request.CreateResponse(HttpStatusCode.OK, itemView);
        }

        [Route("api/pictures/{id}", Name = "PicturesPut")]
        [HttpPut]
        public async Task<HttpResponseMessage> PicturesPut(int id, [FromBody]Picture updatePicture)
        {
            if (updatePicture == null || updatePicture.Id != id)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                var item = await db.Pictures.SingleOrDefaultAsync(p => p.Id == id);
                if (item == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                item.Name = updatePicture.Name;
                await db.SaveChangesAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = id });
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Route("api/pictures/{id}", Name = "PicturesDelete")]
        [HttpDelete]
        public async Task<HttpResponseMessage> PicturesDelete(int? id)
        {
            if (id == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            Picture item = await db.Pictures.SingleOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var assetFileInfo = new FileInfo(String.Format("{0}{1}", HttpContext.Current.Server.MapPath(Settings.MediaFolder), item.Route));
            if (assetFileInfo.Exists)
            {
                assetFileInfo.Delete();
            }
            db.Pictures.Remove(item);
            await db.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.OK, new { Id = id });
        }
        
        #region Methods

        // You could extract these two private methods to a separate utility class since
        // they do not really belong to a controller class but that is up to you
        private MultipartFormDataStreamProvider GetMultipartProvider()
        {
            var assetFolder = HttpContext.Current.Server.MapPath(Settings.UploadFolder);
            // Directory.CreateDirectory(assetFolder);
            return new MultipartFormDataStreamProvider(assetFolder);
        }

        // Extracts Request FormatData as a strongly typed model
        private object GetFormData<T>(MultipartFormDataStreamProvider result)
        {
            if (result.FormData.HasKeys())
            {
                var unescapedFormData = Uri.UnescapeDataString(result.FormData.GetValues(0).FirstOrDefault() ?? String.Empty);
                if (!String.IsNullOrEmpty(unescapedFormData))
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(unescapedFormData);
            }
            return null;
        }

        private string GetDeserializedFileName(MultipartFileData fileData)
        {
            var fileName = GetFileName(fileData);
            return Newtonsoft.Json.JsonConvert.DeserializeObject(fileName).ToString();
        }

        public string GetFileName(MultipartFileData fileData)
        {
            return fileData.Headers.ContentDisposition.FileName;
        }

        #endregion
    }
}
