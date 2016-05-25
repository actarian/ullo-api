using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Routing;
using Ullo.Api.Views;

namespace Ullo.Api
{
    public class PagedController : ApiController
    {
        [DataContract]
        public class FilterSearchItemView
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Value { get; set; }
        }

        [DataContract]
        public class FilterRequestView
        {
            public FilterRequestView()
            {
                this.Page = 1;
                this.Size = 10;
            }
            public FilterRequestView(FilterRequestView source)
            {
                this.Page = source.Page > 0 ? source.Page : 1;
                this.Size = source.Size > 0 ? source.Size : 10;
                this.Status = source.Status;
                this.DateFrom = source.DateFrom;
                this.DateTo = source.DateTo;
                this.Search = source.Search;
            }

            [DataMember]
            public int Page { get; set; }
            [DataMember]
            public int Size { get; set; }
            [DataMember]
            public int? Status { get; set; }
            [DataMember]
            public string DateFrom { get; set; }
            [DataMember]
            public string DateTo { get; set; }
            [DataMember]
            public string Search
            {
                get
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this.SearchList, new BrowserJsonFormatter().SerializerSettings);
                }
                set
                {
                    this.SearchList = (List<FilterSearchItemView>)Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(List<FilterSearchItemView>));
                }
            }
            public List<FilterSearchItemView> SearchList;


            public FilterRequestView previousRequest(int pages)
            {
                if (this.Page > 1)
                {
                    var previous = new FilterRequestView(this);
                    previous.Page -= 1;
                    return previous;
                } else
                {
                    return null;
                }
            }

            public FilterRequestView nextRequest(int pages)
            {
                if (this.Page < pages)
                {
                    var next = new FilterRequestView(this);
                    next.Page += 1;
                    return next;
                } else
                {
                    return null;
                }
            }
        }

        [DataContract]
        public class FilterResponseView : FilterRequestView
        {
            public FilterResponseView(FilterRequestView request)
            {
                this.Page = request.Page;
                this.Size = request.Size;
                this.Status = request.Status;
                this.Search = request.Search;
                this.DateFrom = request.DateFrom;
                this.DateTo = request.DateTo;
            }
            [DataMember]
            public int Pages { get; set; }
            [DataMember]
            public int Count { get; set; }
            [DataMember]
            public string PrevPageLink { get; set; }
            [DataMember]
            public string NextPageLink { get; set; }
        }

        public IEnumerable<dynamic> getPage(string route, IEnumerable<dynamic> items, FilterRequestView requestView)
        {
            int Count = items.Count();
            int Pages = Count > 0 ? (int)Math.Ceiling(Count / (double)requestView.Size) : 0;

            var pagination = new FilterResponseView(requestView);
            pagination.Pages = Pages;
            pagination.Count = Count;

            var previous = requestView.previousRequest(Pages);
            var next = requestView.nextRequest(Pages);

            var urlHelper = new UrlHelper(Request);

            pagination.PrevPageLink = previous != null ? urlHelper.Link(route, new HttpRouteValueDictionary(previous)) : "";
            pagination.NextPageLink = next != null ? urlHelper.Link(route, new HttpRouteValueDictionary(next)) : "";

            System.Web.HttpContext.Current.Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(pagination, new BrowserJsonFormatter().SerializerSettings));

            return items.Skip(pagination.Size * (pagination.Page - 1)).Take(pagination.Size).ToList();
        }

        public static IEnumerable<ListOptionView> EnumToListOptionView<T>(T enumType) where T : Type
        {
            foreach (var value in Enum.GetValues(enumType))
            {
                var name = value.ToString();
                yield return new ListOptionView { Id = (int)Enum.Parse(enumType, name), Name = name, };
            }
        }
    }

    public static class Extensions
    {
        public static void RemoveWhere<T>(this ICollection<T> Coll, Func<T, bool> Criteria)
        {
            List<T> forRemoval = Coll.Where(Criteria).ToList();
            foreach (T obj in forRemoval)
            {
                Coll.Remove(obj);
            }
        }
    }
}