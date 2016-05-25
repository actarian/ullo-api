using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;
using Ullo.Models;

namespace Ullo.Api.Views
{

    #region Posts
    public class PostView
    {
        public int Id;
        public DateTime Created { get; set; }
        public User User;
        public DishView Dish;
        public PictureView Picture;
        public VoteView Vote;
        /*
        public virtual int Votes
        {
            get
            {
                return this.Yes + this.No;
            }
        }
        public virtual decimal Rating
        {
            get
            {
                return this.Votes > 0 ? (decimal)this.Yes / (decimal)this.Votes : 0;
            }
        }
        */
    }
    public class PostAddView
    {
        public DishAddView Dish;
        public string PictureBase64;
    }
    #endregion

    #region Dishes

    public class DishView : BaseView {
        public decimal Price;
        public bool isVeganFriendly;
        public int Yes;
        public int No;
        public DateTime Created;
        public User User;
        public VoteView Vote;
        public ICollection<CategoryView> Categories;
        public ICollection<PictureView> Pictures;
        /*
        public virtual int Votes
        {
            get
            {
                return this.Yes + this.No;
            }
        }
        public virtual decimal Rating
        {
            get
            {
                return this.Votes > 0 ? (decimal)this.Yes / (decimal)this.Votes : 0;
            }
        }
        */
    }

    public class DishDetailView : DishView
    {
        public ICollection<VoteDetailView> Votes;
        /*
        public virtual int Votes
        {
            get
            {
                return this.Yes + this.No;
            }
        }
        public virtual decimal Rating
        {
            get
            {
                return this.Votes > 0 ? (decimal)this.Yes / (decimal)this.Votes : 0;
            }
        }
        */
    }
    
    public class DishAddView {
        public int? Id;
        public string Name;
        public string Price;
        public bool isVeganFriendly;
        public string PictureBase64;
        public ICollection<Category> Categories;
    }
    #endregion

    #region Categories

    public class CategoryView : BaseView {

    }
    #endregion

    #region Pictures

    /*public class PictureView : BaseView {

    } */
    public class PictureView : BaseView {
        public Guid Guid;
        public DateTime Created;
    }

    public class PictureEditView {
        public PictureEditView() {
            this.EditingOptions = new Dictionary<string, ICollection<ListOptionView>>();
        }
        public PictureEditView(Picture item) {
            this.Name = item.Name;
            this.EditingOptions = new Dictionary<string, ICollection<ListOptionView>>();
        }
        public string Name;
        public Dictionary<string, ICollection<ListOptionView>> EditingOptions;
    }

    public class UploadDataModel {
        public int Id { get; set; }
        public string Model { get; set; }
        public Picture.assetTypeEnum PictureType { get; set; }
    }
    #endregion

    #region Votes
    public class VoteView {
        public int DishId;
        public bool Like;
        public DateTime Created;
    }
    public class VoteDetailView : VoteView
    {
        public User User;
    }
    #endregion

    #region Users

    public class FacebookMe
    {
        public string id;
        public string email;
        public string first_name;
        public string last_name;
        public string name;        
    }

    #endregion

    #region Generic
    public class BaseView {
        public int Id;
        public string Name;
        public string Route;
        public virtual string Key {
            get {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string camelCase = Regex.Replace(textInfo.ToTitleCase(this.Name), @"[^A-Za-z0-9_\.~]+", "");
                return camelCase.First().ToString().ToLower() + String.Join("", camelCase.Skip(1));
            }
        }
        /*
        public virtual string Route
        {
            get
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                return Regex.Replace(textInfo.ToLower(this.Name), @"[^A-Za-z0-9_\.~]+", "-");
            }
        }
        */
    }
    public class ListOptionView {
        public int Id;
        public string Name;
    }
    public class AutocompleteRequestView
    {
        public string Phrase;
    }    
    #endregion

    public class Views {
    }
}