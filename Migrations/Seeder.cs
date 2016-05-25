using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Ullo.Models;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Ullo.Migrations
{
    public class Seeder
    {

        public static ApplicationUser addUserWithRole(UlloContext context, string userName, string userPassword, string roleName) {
            var hasher = new PasswordHasher();

            var role = context.Roles.SingleOrDefault(r => r.Name == roleName);
            if (role == null) {
                role = new IdentityRole { Name = roleName, Id = Guid.NewGuid().ToString() };
                context.Roles.AddOrUpdate(role);
                context.SaveChanges();
            }

            var user = context.Users.SingleOrDefault(u => u.UserName == userName);
            if (user == null) {
                user = new ApplicationUser {
                    UserName = userName,
                    PasswordHash = hasher.HashPassword(userPassword),
                    Email = userName,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                user.Roles.Add(new IdentityUserRole { RoleId = role.Id, UserId = user.Id });

                context.Users.AddOrUpdate(user);
                context.SaveChanges();
            }

            return user;

        }
        public static ApplicationUser ReseedAdmins(UlloContext context) {
            var firstUser = addUserWithRole(context, "Ullo", "abc123", "User");
            return firstUser;
        }

        public static void SeedDishes(UlloContext context, ApplicationUser firstUser)
        {
            context.Database.ExecuteSqlCommand("DELETE FROM Categories DBCC CHECKIDENT ('dbo.Categories', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Dishes DBCC CHECKIDENT ('dbo.Dishes', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Posts DBCC CHECKIDENT ('dbo.Posts', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Pictures DBCC CHECKIDENT ('dbo.Pictures', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Votes");

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.

            /** CATEGORIES **/
            List<Category> categories = new List<Category>();
            /*
            Category[] categories = {
                new Category { Name = "Piadine", Route = Identity.getNameAsRoute("Piadine") },
                new Category { Name = "Primi piatti in promo" },
                new Category { Name = "Primi piatti" },
                new Category { Name = "Secondi piatti caldi" },
                new Category { Name = "Secondi piatti di pesce caldi e freddi" },
                new Category { Name = "Secondi piatti freddi" },
                new Category { Name = "Insalatoni" },
                new Category { Name = "Insalatoni fino a 5 ingredienti" },
                new Category { Name = "Contorni freddi" },
                new Category { Name = "Contorni caldi" },
                new Category { Name = "Contorni caldi ordinabili entro le 11:00" },
                new Category { Name = "Desserts" }
            };
            context.Categories.AddOrUpdate(categories);
            */
            string[] categoryNames = {  "Piadine", "Primi piatti in promo", "Primi piatti", "Secondi piatti caldi", "Secondi piatti di pesce caldi e freddi", 
                            "Secondi piatti freddi", "Insalatoni", "Insalatoni fino a 5 ingredienti", "Contorni freddi", 
                            "Contorni caldi ordinabili entro le 11:00", "Desserts" };
            foreach (string category in categoryNames)
            {
                categories.Add(new Category { Name = category, Route = IdentityModels.getNameAsRoute(category) });
            }
            context.Categories.AddOrUpdate(categories.ToArray());
            context.SaveChanges();

            /** DISHES **/
            /*
            Dish[] dishes = {
                new Dish { Name = "Penne con Sugo di Salsiccia e Panna", Price = 3.20M },
                new Dish { Name = "Spaghetti con Pesto alla Genovese", Price = 3.20M },
                new Dish { Name = "Spaghetti con Aglio, Olio e Peperoncino", Price = 3.20M }                
            };
            dishes[0].Categories.Add(categories[1]);
            dishes[1].Categories.Add(categories[1]);
            dishes[2].Categories.Add(categories[1]);            
            context.Dishes.AddOrUpdate(dishes);
            */
            string[] dishNames = { "Penne con Sugo di Salsiccia e Panna", "Spaghetti con Pesto alla Genovese", "Spaghetti con Aglio, Olio e Peperoncino" };
            foreach (string dish in dishNames)
            {
                Dish d = new Dish { Name = dish, Route = IdentityModels.getNameAsRoute(dish), Price = 3.20M, Created = DateTime.Now };
                d.User = firstUser;
                d.Categories.Add(categories[1]);
                d.Pictures.Add(new Picture {
                    Name = dish,
                    Route = String.Format("/Media/{0}.jpg", IdentityModels.getNameAsRoute(dish)),
                    Guid = Guid.NewGuid(),
                    Created = DateTime.Now,
                    AssetType = Picture.assetTypeEnum.Picture                    
                });
                context.Dishes.AddOrUpdate(d);
            }
            context.SaveChanges();

        }

        /*
         * 
        Piadine Varie 2 ingredienti € 4,00, per ogni ingrediente aggiunto supplemento di € 0,50	
        - Indicare gli ingredienti della farcitura: NON ABBIAMO LO STRACCHINO e SALSICCIA. Per le Verdure alla Griglia la Piada deve essere ordinata entro le ore 11.00!!! Grazie
         * 
         * 
        Primi Piatti in Promo - € 3,20							
        Primi Piatti - € 4,00														
        Secondi Piatti Caldi															
        Secondi Piatti Pesce Caldi e Freddi			
        Secondo Piatti - Freddi						
        Insalatoni - € 5,00				
        Insalatoni - € 4,00					
        Insalatoni fino a 5 ingredienti € 4,00, per ogni ingrediente aggiunto supplemento di € 0,50	
        Contorni Freddi € 2,00					
        Contorni Caldi					
        Contorni Caldi con ORDINE ENTRO E NON OLTRE LE ORE 11.00						
        Macedonia o Frutta Tagliata € 2,00		
         * 
         * 
       	Penne con Sugo di Salsiccia e Panna	
        Spaghetti con Pesto alla Genovese	
        Spaghetti con Aglio, Olio e Peperoncino	
        Spaghetti con Sugo di Tonno in Bianco o Rosso - Specificare gentilmente!!!	
        Spaghetti alla Puttanesca	
        Gnocchetti di Patate con Ragù di Carne	
        Gnocchetti di Patate con Sugo di Pomodoro e Basilico	
         * 
         * 
        SUPER PRIMO!!! Spätzle di spinaci fatti in casa con speck e panna - € 5,00	
        SUPER PRIMO!!! Risotto cremoso salmone affumicato e provola - € 5,00	
        Penne con Gorgonzola	
        Penne all'ARRABBIATA	
        CAPPELLACCI RIPIENI con Ragù di Carne	
        Tortellini Pasticciati	
        Tortellini con Panna	
        Tortellini con Ragù di Carne	
        Risotto con Ragù di Carne	
        Ravioli con Sugo di Pomodoro e Basilico	
        Ravioli Burro e Salvia	
        Tagliatelline Casarecce fatte in casa con Ragù di Carne	
        Tagliatelline Casarecce fatte in casa al Limone	
        Pasta a scelta (abbiamo anche Tortiglioni di KAMUT - RISO INTEGRALE) con Ragù di Carne, al Pomodoro o in Bianco 	
         * 
         * 
        OTTIMO!!! Arrosto di Maiale + Contorno di Insalata Mista € 6,00 - Solo il Secondo € 5,00	
        DA NON PERDERE!!! SFOGLINO Sfizioso Salato farcito con Pomodoro e Mozzarella + Contorno di Insalata, Radicchio, Carote e Pomodori € 6,00 - Solo il Secondo € 5,00	
        NOSTRA PRODUZIONE!!! Hamburger di Manzo al 100% cotti alla Piastra Accompagnati + Contorno di Insalata Mista € 6,00 - Solo il Secondo € 5,00	
        RICETTA ORIGINAL  BY Russian Federation - Petto di Pollo alla Bolshevica + Contorno di Insalata Russa. € 6,00 - Solo il Secondo € 5,00	Stick di Filetto di Pollo Pastellati con Erbe Mediterranee con Salsa di Senape e Miele + Contorno di Insalata Russa. € 6,00 - Solo il Secondo € 5,00	Alette di Pollo all'Americana + Contorno di Spinaci Saltati in Padella. € 6,00 - Solo il Secondo € 5,00	Scaloppine di Maiale con Radicchio + Contorno di Spinaci Saltati in Padella. € 6,00 - Solo il Secondo € 5,00	Scaloppine di Maiale con Funghi + Contorno di Spinaci Saltati in Padella. € 6,00 - Solo il Secondo € 5,00	"Petto di pollo limone e capperi + Contorno Insalata Mista. € 5,50 - Solo il Secondo € 4,50
"	Petto di Pollo al Limone + Contorno Insalata Mista. € 5,00 - Solo il Secondo € 4,00	Petto di Pollo all'Aceto Balsamico + Insalata Russa. € 5,00 - Solo il Secondo € 4,00	Petto di Pollo all'Aceto di Vino Bianco + Contorno Insalata Mista. € 5,00 - Solo il Secondo € 4,00	Cotoletta di Pollo alla Milanese + Insalata Russa. € 6,00 - Solo il Secondo € 5,00	Petto di Pollo Impanato e Cotto al Forno + Contorno Insalata Mista. € 6,00 - Solo il Secondo € 5,00 (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Carne di Pollo, indicare la tipologia di preparazione, il condimento e la cottura. Anche tagliata di Pollo!!! Base € 4,50 + Altri Condimenti € 5,50	Filetti di Platessa cotti in Padella con Pendolini ed Olive Nere € 4,50	Carpaccio di Salmone Affumicato Norvegese con Isalata di Rucola, Verde e Pomodorini (Pepe + Sale + Olio + Glassa Balsamica) € 7,00	MITICI!!! ROTOLINI di Salmone Affumicato Norvegese con Philadelphia e Rucola con guarnizione di Pomodorini e Rucola € 6,00	Caprese con Pomodori e Mozzarella - € 4,00	Caprese con Pomodori e Mozzarella + Prosciutto Crudo - € 4,50	Bresaola Rucola e Scaglie di Grana (gr. 80) - € 4,50	Involtini di Bresaola con Ricotta e Rucola - € 5,00	Prosciutto Crudo e Mozzarella - € 5.00	Salumi Misti (Prosciutto Crudo e Cotto, Salame Milano e Lonza) - € 4,50	NEW ENTRY!!! Insalatona KENTUCKY con verde, bacon arrostito, salsa Caesar, nachos, pendolini + 1 Cotoletta di Pollo alla Milanese - € 5,00 (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Insalatona alla MILANESE con verde, radicchio, carote, mais, pendolini + 1 Cotoletta di Pollo alla Milanese - € 5,00 (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Insalatone con FILETTI DI SALMONE AFFUMICATO: Insalata Verde, Salmone Affumicato, Olive Nere Denocciolate, Feta Greca, Mentuccia, Olio di oliva e Curcuma - € 5,00	Insalatone con UOVA e PROSCIUTTO: Insalata Mista (Verde, Rucola), Uova Sodo, Fettine di Prosciutto Crudo il tutto condito con Olio, Aceto Bianco, Sale, Pepe Bianco e Aceto Balsamico.	Insalatona GOLOSA con verde, tonno, wurstel, uova sode, mais e patate lesse	Insalatona BOSCAIOLA con Prosciutto Cotto e Formaggio + Maionese 	Insalatona CESAR con Petto di Pollo e Scaglie di Grana	Insalatona NIZZARDA con Uovo Sodo e Tonno	Insalatone di POLLO: Insalata Verde, Carote, Mais, Petto di Pollo tagliato a Julienne	Insalatone a SCELTA: Indicare gli ingredienti desiderati con anche il condimento gradito. ABBIAMO IL TOFU!!!	Insalata Mista	Patate Lesse	Pomodori	Fagiolini	Insalata Russa di Nostra Produzione - THE ORIGINAL!!! - € 2,50	NOVITA'!!! Lenticchie con Pomodoro € 2,50	Patatine Fritte - Indicare se si vuole Ketchup e/o Maionese - € 2,50	Crocchette di Patate - € 2,50	Anelli di Cipolla - € 2,50	Spinaci saltati in padella - Indicare gentilmente il tipo di condimento, altrimenti le prepariamo scondite con solo sale - € 2,00	Verdure miste alla griglia (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Verdure miste gratinate (Ordinare entro e non oltre le ore 11.00. Grazie tante) - € 2,50	Carote cotte al Vapore (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Piselli saltati in Padella con Prosciutto Cotto (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Funghi Trifolati (Ordinare entro e non oltre le ore 11.00. Grazie tante)	Olive all'Ascolana cotte in Padella - € 4,00	Macedonia di Frutta Tagliata BASE con: Ananas, Mela, Pera, Kiwi	Ananas	Melone
         * 
         * 
        "Comunicazione Importante:
Gli Ordinativi dovranno pervenire entro e non oltre le ore 12:00 salvo diverse indicazioni ripotate nel Menù. Nel caso di urgenze si possono effettuare ulteriori
ordini entro le ore 12:30 telefonando al numero 0721/205856 oppure cell.: 366 4061762 (Andrea)"

         * 
        */

        static Random randomizer = new Random();

        public static void SeedDishesTest(UlloContext context, ApplicationUser firstUser)
        {
            context.Database.ExecuteSqlCommand("DELETE FROM Categories DBCC CHECKIDENT ('dbo.Categories', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Dishes DBCC CHECKIDENT ('dbo.Dishes', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Posts DBCC CHECKIDENT ('dbo.Posts', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Pictures DBCC CHECKIDENT ('dbo.Pictures', RESEED, 1)");
            context.Database.ExecuteSqlCommand("DELETE FROM Votes");

            string[] categoryNames = {  "Piadine", "Primi piatti in promo", "Primi piatti", "Secondi piatti caldi", "Secondi piatti di pesce caldi e freddi", 
                            "Secondi piatti freddi", "Insalatoni", "Insalatoni fino a 5 ingredienti", "Contorni freddi", 
                            "Contorni caldi ordinabili entro le 11:00", "Desserts" };

            List<Category> categories = new List<Category>();
            foreach (string category in categoryNames)
            {
                categories.Add(new Category { Name = category, Route = IdentityModels.getNameAsRoute(category) });
            }
            context.Categories.AddOrUpdate(categories.ToArray());
            context.SaveChanges();

            List<Dish> dishes = new List<Dish>();
            string[] dishNames = { "Penne con Sugo di Salsiccia e Panna", "Spaghetti con Pesto alla Genovese", "Spaghetti con Aglio, Olio e Peperoncino" };
            foreach (string dish in dishNames)
            {
                Dish d = new Dish { Name = dish, Route = IdentityModels.getNameAsRoute(dish), Price = 3.20M, Created = DateTime.Now };
                d.User = firstUser;
                d.Categories.Add(categories[1]);
                d.Pictures.Add(new Picture {
                    Name = dish,
                    Route = String.Format("/Media/{0}.jpg", IdentityModels.getNameAsRoute(dish)),
                    Guid = Guid.NewGuid(),
                    Created = DateTime.Now,
                    AssetType = Picture.assetTypeEnum.Picture
                });
                dishes.Add(d);
            }
            context.Dishes.AddOrUpdate(dishes.ToArray());
            context.SaveChanges();

            for (int i = 1; i <= 100; i++)
            {
                // int r = randomizer.Next(dishes.Count);
                Dish randomDish = dishes.ElementAt(i % dishes.Count);

                Post p = new Post { Created = DateTime.Now };
                p.User = firstUser;
                p.Dish = randomDish;
                p.Picture = randomDish.Pictures.ElementAt(0);
                context.Posts.AddOrUpdate(p);
            }
            context.SaveChanges();

        }
    }


}