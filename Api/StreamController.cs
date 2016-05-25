using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity.Migrations;
using System.Web.Http.Cors; // we import the http cors service for enabling cross-domain
using Ullo.Models; // we import our Models namespace here
using Ullo.Api.Views;
// Using ObjectQuery.ToTraceString to see the generated SQL query that has been actually submitted to SQL Server reveals the mystery:

namespace Ullo.Api
{
    // [EnableCors(origins: "dev.ullowebapp:8081", headers: "*", methods: "*", exposedHeaders: "X-Pagination")] // we enable the cors for the whole controller, in production change origins * with client url http://mywebclient.azurewebsites.net
    // [Authorize]

    public class StreamController : BaseController
    {

        [Route("api/stream/anonymous", Name = "GetStreamAnonymous")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> GetStreamAnonymous()
        {            
            var items = db.Posts.Include(x => x.User).Include(x => x.Dish).Include(x => x.Picture).Select(x => new PostView {
                Id = x.Id,
                Created = x.Created,
                User = new User {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Dish = new DishView {
                    Id = x.Dish.Id,
                    Name = x.Dish.Name,
                    Price = x.Dish.Price,
                    isVeganFriendly = x.Dish.isVeganFriendly,
                    Yes = x.Dish.Yes,
                    No = x.Dish.No,
                    Created = x.Created,
                    Categories = x.Dish.Categories.Select(c => new CategoryView {
                        Id = c.Id,
                        Name = c.Name,
                    }).ToList(),
                    Vote = db.Votes.Where(v => v.DishId == x.Dish.Id).Select(v => new VoteView {
                        DishId = v.DishId,
                        Like = v.Like,
                        Created = v.Created
                    }).FirstOrDefault(),
                },
                Picture = new PictureView {
                    Id = x.Picture.Id,
                    Guid = x.Picture.Guid,
                    Name = x.Picture.Name,
                    Route = x.Picture.Route,
                    Created = x.Picture.Created,
                },

            }).OrderByDescending(x => x.Id).Take(10);
            return Request.CreateResponse(HttpStatusCode.OK, await items.ToListAsync());
        }

        [Route("api/stream", Name = "GetStream")]
        [HttpGet]
        [Authorize]
        public async Task<HttpResponseMessage> GetStream()
        {
            ApplicationUser user = await users.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }

            var items = db.Posts.Include(x => x.User).Include(x => x.Dish).Include(x => x.Picture).Select(x => new PostView {
                Id = x.Id,
                Created = x.Created,
                User = new User {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Dish = new DishView {
                    Id = x.Dish.Id,
                    Name = x.Dish.Name,
                    Price = x.Dish.Price,
                    isVeganFriendly = x.Dish.isVeganFriendly,
                    Yes = x.Dish.Yes,
                    No = x.Dish.No,
                    Created = x.Created,                
                    Categories = x.Dish.Categories.Select(c => new CategoryView {
                        Id = c.Id,
                        Name = c.Name,
                    }).ToList(),
                    Vote = db.Votes.Where(v => v.DishId == x.Dish.Id && v.UserId == user.Id).Select(v => new VoteView {
                        DishId = v.DishId,
                        Like = v.Like,
                        Created = v.Created
                    }).FirstOrDefault(),
                    /*
                    User = new User {
                        UserName = x.Dish.User.UserName,
                        FacebookId = x.Dish.User.FacebookId,
                        Route = x.User.Route,
                    },
                    Pictures = x.Dish.Pictures.Select(c => new PictureView {
                        Id = c.Id,
                        Guid = x.Picture.Guid,
                        Name = c.Name,
                        Route = c.Route,
                        Created = c.Created,
                    }).ToList(),
                     * */
                },
                Picture = new PictureView {
                    Id = x.Picture.Id,
                    Guid = x.Picture.Guid,
                    Name = x.Picture.Name,
                    Route = x.Picture.Route,
                    Created = x.Picture.Created,
                },
                
            }).OrderByDescending(x => x.Id).Take(10);

            return Request.CreateResponse(HttpStatusCode.OK, await items.ToListAsync());
        }

        [Route("api/stream/paged", Name = "GetStreamPaged")]
        [HttpGet]
        [Authorize]
        public async Task<HttpResponseMessage> GetStreamPaged([FromUri] FilterRequestView request)
        {
            if (request == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await users.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }
            IQueryable<PostView> items = db.Posts.Include(x => x.User).Include(x => x.Dish).Include(x => x.Picture).Select(x => new PostView {
                Id = x.Id,
                Created = x.Created,
                User = new User {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Dish = new DishView {
                    Id = x.Dish.Id,
                    Name = x.Dish.Name,
                    Price = x.Dish.Price,
                    isVeganFriendly = x.Dish.isVeganFriendly,
                    Yes = x.Dish.Yes,
                    No = x.Dish.No,
                    Created = x.Created,                
                    Categories = x.Dish.Categories.Select(c => new CategoryView {
                        Id = c.Id,
                        Name = c.Name,
                    }).ToList(),
                    Vote = db.Votes.Where(v => v.DishId == x.Dish.Id && v.UserId == user.Id).Select(v => new VoteView {
                        DishId = v.DishId,
                        Like = v.Like,
                        Created = v.Created
                    }).FirstOrDefault(),
                    /*
                    User = new User {
                        UserName = x.Dish.User.UserName,
                        FacebookId = x.Dish.User.FacebookId,
                        Route = x.User.Route,
                    },
                    Pictures = x.Dish.Pictures.Select(c => new PictureView {
                        Id = c.Id,
                        Guid = x.Picture.Guid,
                        Name = c.Name,
                        Route = c.Route,
                        Created = c.Created,
                    }).ToList(),
                    */
                },
                Picture = new PictureView {
                    Id = x.Picture.Id,
                    Guid = x.Picture.Guid,
                    Name = x.Picture.Name,
                    Route = x.Picture.Route,
                    Created = x.Picture.Created,
                },

            });
            if (request.SearchList != null)
            {
                foreach (FilterSearchItemView s in request.SearchList)
                {
                    switch (s.Name)
                    {
                        case "name":
                            items = items.Where(r => r.User.UserName != null && r.User.UserName.Contains(s.Value));
                            break;
                    }
                }
            }
            items = items.OrderByDescending(r => r.Id);
            var pagedItems = getPage("GetDishesPaged", items, request);
            return Request.CreateResponse(HttpStatusCode.OK, pagedItems);
        }

        [Route("api/post/{id}", Name = "GetPostById")]
        [HttpGet]
        [Authorize]
        public async Task<HttpResponseMessage> GetPostById(int? id)
        {
            if (id == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            Dish item = await db.Dishes.Include(x => x.Categories).Include(x => x.Pictures).SingleOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            // var itemView = new ItemEditView(item);
            // itemView.EditingOptions = getEditingOptions();
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

        [Route("api/post/route/{route}", Name = "GetPostByRoute")]
        [HttpGet]
        [Authorize]
        public async Task<HttpResponseMessage> GetPostByRoute(string route)
        {
            if (route == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            Dish item = await db.Dishes.Include(x => x.Categories).Include(x => x.Pictures).SingleOrDefaultAsync(i => i.Route == route);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            // var itemView = new ItemEditView(item);
            // itemView.EditingOptions = getEditingOptions();
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

        [Route("api/post", Name = "AddPost")]
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> AddPost([FromBody] PostAddView model)
        {
            if (model == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            var user = db.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.MethodNotAllowed, "Unknown User");
                // throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }
            if (ModelState.IsValid)
            {
                Picture picture = Picture.getPictureFromBase64(model.PictureBase64);
                if (picture == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Picture");
                }
                try
                {
                    foreach (Category category in model.Dish.Categories)
                    {
                        db.Categories.Attach(category);
                    }
                    decimal Price = 0.0M;
                    Decimal.TryParse(model.Dish.Price.Replace(",", "."), NumberStyles.Currency, CultureInfo.InvariantCulture, out Price);
                    Dish dish = new Dish {
                        Categories = model.Dish.Categories,
                        Name = model.Dish.Name,
                        Route = IdentityModels.getNameAsRoute(model.Dish.Name),
                        Price = Price,
                        isVeganFriendly = model.Dish.isVeganFriendly,
                        Created = DateTime.Now
                    };                    
                    if (model.Dish.Id != null) {
                        db.Pictures.Add(picture);
                        await db.SaveChangesAsync();
                        dish.Id = (int)model.Dish.Id;
                        db.Dishes.Attach(dish);
                        dish.Pictures.Add(picture);    
                    } else
                    {
                        dish.User = user;
                        dish.Pictures.Add(picture);
                        db.Dishes.Add(dish);
                    }
                    await db.SaveChangesAsync();
                    Post post = new Post {
                        Picture = picture,
                        UserId = user.Id,
                        DishId = dish.Id,
                        PictureId = picture.Id,                        
                        Created = DateTime.Now
                    };
                    db.Posts.Add(post);
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new PostView {
                        Id = post.Id,
                        Created = post.Created,
                    });
                } catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

    }
}