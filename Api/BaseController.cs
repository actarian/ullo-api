using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

using Ullo.Models;

namespace Ullo.Api
{
    public class BaseController : PagedController
    {
        private ApplicationUserManager _users;
        private ApplicationRoleManager _roles;
        public ApplicationUserManager users
        {
            get
            {
                return _users ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _users = value;
            }
        }
        public ApplicationRoleManager roles
        {
            get
            {
                return _roles ?? HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roles = value;
            }
        }
        public UlloContext db;        
        public BaseController() : base()
        {
            db = new UlloContext();
            db.Configuration.LazyLoadingEnabled = false;            
        }        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                users.Dispose();
                roles.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}