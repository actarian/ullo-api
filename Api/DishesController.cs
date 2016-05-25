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
    [Authorize]
    public class DishesController : BaseController
    {        
        [Route("api/dishes", Name = "GetDishes")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDishes()
        {
            ApplicationUser user = await users.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }
            var items = db.Dishes.Include(x => x.User).Include(x => x.Categories).Include(x => x.Pictures).Select(x => new DishView
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                isVeganFriendly = x.isVeganFriendly,
                Yes = x.Yes,
                No = x.No,
                Created = x.Created,                
                User = new User
                {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Categories = x.Categories.Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name,
                }).ToList(),
                Pictures = x.Pictures.Select(c => new PictureView
                {
                    Id = c.Id,
                    Guid = c.Guid,
                    Name = c.Name,
                    Route = c.Route,
                    Created = c.Created,
                }).ToList(),
                Vote = db.Votes.Where(v => v.DishId == x.Id && v.UserId == user.Id).Select(v => new VoteView
                {
                    DishId = v.DishId,
                    Like = v.Like,
                    Created = v.Created
                }).FirstOrDefault()
            }).Take(10).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        [Route("api/dishes/paged", Name = "GetDishesPaged")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDishesPaged([FromUri] FilterRequestView request)
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
            IQueryable<DishView> items = db.Dishes.Include(x => x.User).Include(x => x.Categories).Include(x => x.Pictures).Select(x => new DishView
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                isVeganFriendly = x.isVeganFriendly,
                Yes = x.Yes,
                No = x.No,
                Created = x.Created,
                User = new User
                {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Categories = x.Categories.Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name,
                }).ToList(),
                Pictures = x.Pictures.Select(c => new PictureView
                {
                    Id = c.Id,
                    Guid = c.Guid,
                    Name = c.Name,
                    Route = c.Route,
                    Created = c.Created,
                }).ToList(),
                Vote = db.Votes.Where(v => v.DishId == x.Id && v.UserId == user.Id).Select(v => new VoteView
                {
                    DishId = v.DishId,
                    Like = v.Like,
                    Created = v.Created
                }).FirstOrDefault()
            });
            if (request.SearchList != null)
            {
                foreach (FilterSearchItemView s in request.SearchList)
                {
                    switch (s.Name)
                    {
                        case "UserName":
                            items = items.Where(r => r.User.UserName != null && r.User.UserName == s.Value);
                            break;
                        case "Category":
                            items = items.Where(r => r.Categories.FirstOrDefault().Name == s.Value);
                            break;
                    }
                }
            }
            items = items.OrderByDescending(r => r.Id);
            var pagedItems = getPage("GetDishesPaged", items, request);
            return Request.CreateResponse(HttpStatusCode.OK, pagedItems);
        }

        [Route("api/dishes/{id}", Name = "GetDishById")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDishById(int? id)
        {
            if (id == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = await users.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }
            DishDetailView item = await db.Dishes.Include(x => x.User).Include(x => x.Categories).Include(x => x.Pictures).Select(x => new DishDetailView {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                isVeganFriendly = x.isVeganFriendly,
                Yes = x.Yes,
                No = x.No,
                Created = x.Created,
                User = new User {
                    UserName = x.User.UserName,
                    FacebookId = x.User.FacebookId,
                    Route = x.User.Route,
                },
                Categories = x.Categories.Select(c => new CategoryView {
                    Id = c.Id,
                    Name = c.Name,
                }).ToList(),
                Pictures = x.Pictures.Select(c => new PictureView {
                    Id = c.Id,
                    Guid = c.Guid,
                    Name = c.Name,
                    Route = c.Route,
                    Created = c.Created,
                }).ToList(),
                Vote = db.Votes.Where(v => v.DishId == x.Id && v.UserId == user.Id).Select(v => new VoteView {
                    DishId = v.DishId,
                    Like = v.Like,
                    Created = v.Created
                }).FirstOrDefault(),
                Votes = db.Votes.Where(v => v.DishId == x.Id).Select(v => new VoteDetailView {
                    User = db.Users.Where(u => u.Id == v.UserId).Select(u => new User {
                        UserName = u.UserName,
                        FacebookId = u.FacebookId,
                        Route = u.Route,
                    }).FirstOrDefault(),
                    DishId = v.DishId,
                    Like = v.Like,
                    Created = v.Created
                }).OrderByDescending(v=> v.Created).Take(10).ToList(),
            }).SingleOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            // var itemView = new ItemEditView(item);
            // itemView.EditingOptions = getEditingOptions();
            return Request.CreateResponse(HttpStatusCode.OK, item);
        }

        [Route("api/dishes/route/{route}", Name = "GetDishByRoute")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDishByRoute(string route)
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

        [Route("api/dishes", Name = "AddDish")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddDish([FromBody] DishAddView model)
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
                    foreach (Category category in model.Categories)
                    {
                        db.Categories.Attach(category);
                    }
                    decimal Price = 0.0M;                    
                    Decimal.TryParse(model.Price.Replace(",","."), NumberStyles.Currency, CultureInfo.InvariantCulture, out Price);
                    Dish dish = new Dish {
                        Categories = model.Categories,
                        Name = model.Name,
                        Route = IdentityModels.getNameAsRoute(model.Name),
                        Price = Price,
                        isVeganFriendly = model.isVeganFriendly,
                        Created = DateTime.Now
                    };
                    dish.Pictures.Add(picture);
                    dish.User = user;
                    db.Dishes.Add(dish);
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new DishView {
                        Id = dish.Id,
                        Name = dish.Name,
                        Price = dish.Price,
                        isVeganFriendly = dish.isVeganFriendly,
                        Yes = dish.Yes,
                        No = dish.No,
                        Categories = dish.Categories.Select(c => new CategoryView {
                            Id = c.Id,
                            Name = c.Name,
                        }).ToList(),
                        Pictures = dish.Pictures.Select(c => new PictureView {
                            Id = c.Id,
                            Name = c.Name,
                            Route = c.Route,
                            Created = c.Created,
                        }).ToList(),
                    });
                } catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Route("api/dishes/vote", Name = "VoteDish")]
        [HttpPost]
        public async Task<HttpResponseMessage> VoteDish([FromBody] VoteView model)
        {
            if (model == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            ApplicationUser user = await users.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
            }
            if (ModelState.IsValid)
            {
                Vote item = new Vote
                {
                    DishId = model.DishId,
                    UserId = user.Id,
                    Like = model.Like,
                    Created = DateTime.Now
                };
                db.Votes.AddOrUpdate(item);
                await db.SaveChangesAsync();
                Dish dish = await db.Dishes.Include(x => x.Categories).Include(x => x.Pictures).SingleOrDefaultAsync(i => i.Id == model.DishId);
                dish.Yes = db.Votes.Where(x => x.DishId == dish.Id && x.Like == true).Count();
                dish.No = db.Votes.Where(x => x.DishId == dish.Id && x.Like == false).Count();
                await db.SaveChangesAsync();
                if (dish == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new DishView
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Price = dish.Price,
                    isVeganFriendly = dish.isVeganFriendly,
                    Yes = dish.Yes,
                    No = dish.No,
                    Created = dish.Created,
                    Categories = dish.Categories.Select(c => new CategoryView
                    {
                        Id = c.Id,
                        Name = c.Name,
                    }).ToList(),
                    Pictures = dish.Pictures.Select(c => new PictureView
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Route = c.Route,
                        Created = c.Created,
                    }).ToList(),
                    Vote = new VoteView
                    {
                        DishId = item.DishId,
                        Like = item.Like,
                        Created = item.Created
                    }
                });
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);  
        }

        [Route("api/dishes/autocomplete", Name = "AutocompleteDish")]
        [HttpPost]
        public async Task<HttpResponseMessage> AutocompleteDish([FromBody] AutocompleteRequestView model)
        {
            if (model == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Data");
            }
            if (ModelState.IsValid)
            {
                List<ListOptionView> items = await db.Dishes.Where(x => x.Name != null && x.Name.Contains(model.Phrase)).Take(5).Select(x => new ListOptionView {
                    Id = x.Id,
                    Name = x.Name,
                }).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }        
    }
}