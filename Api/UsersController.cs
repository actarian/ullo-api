using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Security.Claims;

using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Caching;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

using Facebook;

using System.Web.Http.Cors; // we import the http cors service for enabling cross-domain
using Ullo.Models;
using Ullo.Api.Views;

namespace Ullo.Api {

    // [EnableCors(origins: "*", headers: "*", methods: "*")] // we enable the cors for the whole controller, in production change origins * with client url http://mywebclient.azurewebsites.net

    /// <summary>
    ///  The Users controller retrieve a single user.
    /// </summary>
    ///
    
    [AllowAnonymous]

    public class UsersController : BaseController {


        [Authorize]
        [Route("api/users", Name = "GetUsers")]
        [HttpGet] // we limit this method to receive calls only from GET VERB with the attribute [HttpGet]
        public HttpResponseMessage GetUsers() {
            // var user = await users.FindByEmailAsync("lzampetti@gmail.com");
            // return Request.CreateResponse(HttpStatusCode.OK, user);

            var items = users.Users.Select(x => new UserDetail {
                Email = x.Email,
                UserName = x.UserName,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Route = x.Route,

            }).Take(10).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        [Authorize]
        [Route("api/users/route/{route}", Name = "GetUserByRoute")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetUserByRoute(string route)
        {
            if (route == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var item = await users.Users.Include(x => x.Dishes).Select(x => new UserDetail {
                Email = x.Email,
                UserName = x.UserName,
                FirstName = x.FirstName,
                LastName = x.LastName,
                FacebookId = x.FacebookId,
                Route = x.Route,

            }).SingleOrDefaultAsync(i => i.Route == route);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

        [Authorize]
        [Route("api/users/count", Name = "Count")]
        [HttpGet]
        public async Task<HttpResponseMessage> Count()
        {
            try
            {
                int count = await users.Users.CountAsync();
                return Request.CreateResponse(HttpStatusCode.OK, new { users = count });
            } catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/users/current", Name = "Current")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Current()
        {
            try
            {
                var indentityName = HttpContext.Current.User.Identity.Name;
                var user = await users.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    var account = new UserDetail();
                    account.UserName = user.UserName;
                    account.FirstName = user.FirstName;
                    account.LastName = user.LastName;
                    account.FacebookId = user.FacebookId;
                    account.isAuthenticated = true;
                    account.Route = user.Route;
                    var rolesForUser = users.GetRoles(user.Id);
                    account.isAdmin = rolesForUser.Contains("Admin");
                    return Request.CreateResponse(account);
                } else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }
            } catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [Route("api/users/signin", Name = "SignIn")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SignIn(UserSignIn model) {
            if (model == null) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            if (ModelState.IsValid) {
                // This doesn't count login failures towards lockout only two factor authentication
                // To enable password failures to trigger lockout, change to shouldLockout: true
                var result = await SignInHelper.PasswordSignIn(model.UserName, model.Password, true, false);                
                switch (result) {
                    case Ullo.Models.SignInStatus.Success:
                        var user = await users.FindByNameAsync(model.UserName);
                        await SignInHelper.SignInAsync(user, true, false);
                        var account = new UserDetail();
                        account.UserName = user.UserName;
                        account.FirstName = user.FirstName;
                        account.LastName = user.LastName;
                        account.FacebookId = user.FacebookId;
                        account.isAuthenticated = true;
                        account.Route = user.Route;
                        var rolesForUser = users.GetRoles(user.Id);
                        account.isAdmin = rolesForUser.Contains("Admin");
                        return Request.CreateResponse(account);
                    case Ullo.Models.SignInStatus.LockedOut:
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "LockedOut");
                    case Ullo.Models.SignInStatus.RequiresTwoFactorAuthentication:
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "RequiresTwoFactorAuthentication");
                    case Ullo.Models.SignInStatus.Failure:
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Failure");
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid");
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Route("api/users/signinwithfacebook", Name = "SignInWithFacebook")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SignInWithFacebook(FacebookSignIn model) {
            if (model == null) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            if (ModelState.IsValid) {
                var client = new FacebookClient(model.AccessToken);
                Facebook.JsonObject me = (Facebook.JsonObject)client.Get("/me?fields=name,first_name,last_name,id,email");
                
                ApplicationUser user = db.Users.Where(x => x.FacebookId == model.UserID).FirstOrDefault();
                if (user != null) {
                    user.FacebookToken = model.AccessToken;
                    await db.SaveChangesAsync();

                    // if (user.FacebookToken == model.AccessToken) {
                        await SignInHelper.SignInAsync(user, true, true);
                        var account = new UserDetail();
                        account.UserName = user.UserName;
                        account.FirstName = me["first_name"].ToString(); // user.FirstName;
                        account.LastName = user.LastName;
                        account.FacebookId = user.FacebookId;
                        account.isAuthenticated = true;
                        account.Route = user.Route;
                        var rolesForUser = users.GetRoles(user.Id);
                        account.isAdmin = rolesForUser.Contains("Admin");
                        return Request.CreateResponse(account);
                    /*
                    } else {
                        return Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Facebook token differs");
                    }
                     * */
                } else {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Facebook id not found");
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Route("api/users/signup", Name = "Signup")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Signup(UserSignUp model)
        {
            if (model == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    FacebookId = model.FacebookId,
                    FacebookPicture = model.FacebookPicture,
                    FacebookToken = model.FacebookToken,
                    Route = IdentityModels.getNameAsRoute(model.UserName),
                };
                var result = await users.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInHelper.SignInAsync(user, true, false);
                    var account = new UserDetail();
                    account.UserName = user.UserName;
                    account.FirstName = user.FirstName;
                    account.LastName = user.LastName;
                    account.FacebookId = user.FacebookId;
                    account.isAuthenticated = true;
                    account.Route = user.Route;
                    var rolesForUser = users.GetRoles(user.Id);
                    account.isAdmin = rolesForUser.Contains("Admin");
                    return Request.CreateResponse(account);
                    // var code = await users.GenerateEmailConfirmationTokenAsync(user.Id);
                    // send email confirmation token
                } else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, result.Errors.FirstOrDefault());
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Route("api/users/signout", Name = "SignOut")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> SignOut()
        {
            try {
                var user = await users.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    try
                    {
                        var client = new FacebookClient(user.FacebookToken);
                        Facebook.JsonObject response = (Facebook.JsonObject)client.Delete("/me/permissions");
                    } catch (Exception ex)
                    {
                        // null
                    }
                }
                AuthenticationManager.SignOut();
                var account = new UserDetail();
                account.isAuthenticated = false;
                return Request.CreateResponse(account);
            } catch (Exception ex) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
       
        private IAuthenticationManager AuthenticationManager {
            get {
                return System.Web.HttpContext.Current.Request.GetOwinContext().Authentication;
            }
        }

        private SignInHelper _helper;

        private SignInHelper SignInHelper {
            get {
                if (_helper == null) {
                    _helper = new SignInHelper(users, AuthenticationManager);
                }
                return _helper;
            }
        }
        
    }
}