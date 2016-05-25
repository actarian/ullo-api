namespace Ullo.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;
    using Ullo.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Ullo.Models.UlloContext>
    {
        public static bool MIGRATION = true;
        public static bool DATALOSS = false;

        public Configuration()
        {
            AutomaticMigrationsEnabled = MIGRATION;
            AutomaticMigrationDataLossAllowed = DATALOSS;
        }

        protected override void Seed(UlloContext context)
        {
            //  This method will be called after migrating to the latest version.
            if (DATALOSS)
            {
                ApplicationUser firstUser = Ullo.Migrations.Seeder.ReseedAdmins(context);
                Ullo.Migrations.Seeder.SeedDishesTest(context, firstUser);
            }
        }
    }
}
