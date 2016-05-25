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

namespace Ullo.Api
{
    public class CachedController : BaseController {
        public ObjectCache cache;
        public CacheItemPolicy policy;
        public CachedController() : base()
        {
            cache = MemoryCache.Default;
            policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
        }
    }
}