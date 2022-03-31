﻿using System.Data.Entity.Migrations;

namespace Exchange.Server.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<Exchange.Server.Database.LettersDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Exchange.Server.Database.LettersDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
