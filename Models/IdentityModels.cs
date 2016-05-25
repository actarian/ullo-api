using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ullo.Models
{
    public partial class IdentityModels
    {
        public static string getNameAsRoute(string name)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return Regex.Replace(textInfo.ToLower(name), @"[^A-Za-z0-9_\.~]+", "-");
        }
    }

    public class UlloContext : IdentityDbContext<ApplicationUser>
    {     // DbContext
        public UlloContext()
            : base("UlloContext", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<UlloContext>());
        }
        static UlloContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<UlloContext, Ullo.Migrations.Configuration>());
        }

        /*
        static UlloContext() {
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
            Database.SetInitializer<UlloContext>(new ApplicationDbInitializer());
        }
        */

        public static UlloContext Create()
        {
            return new UlloContext();
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Picture> Pictures { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // POSTS
            modelBuilder.Entity<Post>()
                        .HasOptional<ApplicationUser>(s => s.User)
                        .WithMany(s => s.Posts)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<Post>()
                        .HasRequired<Dish>(s => s.Dish)
                        .WithMany()
                        .HasForeignKey(s => s.DishId);
            modelBuilder.Entity<Post>()
                        .HasRequired<Picture>(s => s.Picture)
                        .WithMany()
                        // .Map(a => a.MapKey("PictureId"));
                        .HasForeignKey(s => s.PictureId);

            // DISHES
            modelBuilder.Entity<Dish>()
                         .HasMany<Category>(s => s.Categories)
                         .WithMany(c => c.Dishes)
                         .Map(cs =>
                         {
                             cs.MapLeftKey("DishId");
                             cs.MapRightKey("CategoryId");
                             cs.ToTable("DishCategories");
                         });
            modelBuilder.Entity<Dish>()
                        .HasOptional<ApplicationUser>(s => s.User)
                        .WithMany(s => s.Dishes)
                        .HasForeignKey(s => s.UserId);
            modelBuilder.Entity<Dish>()
                       .HasMany<Picture>(s => s.Pictures)
                       .WithMany()
                       .Map(cs =>
                       {
                           cs.MapLeftKey("DishId");
                           cs.MapRightKey("PictureId");
                           cs.ToTable("DishPictures");
                       });

            base.OnModelCreating(modelBuilder);
        }

    }
}