using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;

namespace API
{
    public class Program
    {

        // We could use the cli to push migrations to the database after using cli commands to create them or we can 
        // automatically push them to the database every time we save (NOT NECESSARY, you can still use cli commands)
        // We will modifiy the Main method in order to do automatic migration pushes
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // The 'using' keyword makes it so that when the method finishes running then it will be disposed of and wont
            // be left hanging around. we want to dispose it because this is where well be storing any of our services
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            try
            {
                // We are getting this as a service because we added it as such in API/Startup.cs
                // We are using 'GetRequiredService' so that we can populate the database and use it inside 'context'
                // This will grab all the database models in Persistence/DataContext.cs and migrate them
                var context = services.GetRequiredService<DataContext>();
                await context.Database.MigrateAsync();
                await Seed.SeedData(context);
                // Because the Persistence/Seed.cs method to populate the databse if it is empty is asynchronous
                // we have to add to await keyword to the migrations and when we get the seed data (NOT NECESSARY)  
            }
            catch (Exception e)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(e, "An error occured during migration");
            }

            // After successfully migrating then we run 'host' which will run the actual application
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
