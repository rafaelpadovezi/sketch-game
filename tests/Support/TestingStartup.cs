using AutoMapper;
using Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sketch;
using Sketch.Infrastructure.IoC;
using System.Linq;

namespace Tests.Support
{
    public class TestingStartUp
    {
        public TestingStartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // this is required to add the controllers of the main Hangman project
            var startupAssembly = typeof(Startup).Assembly;

            services
                .AddApplicationCore()
                .AddHttpContextAccessor()
                .AddDbContext<SketchDbContext>(options =>
                {
                    options
                        .UseNpgsql(Configuration.GetConnectionString("DBConnection"))
                        .UseLoggerFactory(Startup.ConsoleLoggerFactory)
                        .EnableSensitiveDataLogging();
                }, ServiceLifetime.Singleton)
                .AddAutoMapper(startupAssembly)
                // MVC Stuff
                .AddControllers()
                .AddApplicationPart(startupAssembly) // adds controllers from main project
                .AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Migrate();
        }

        /**
         * Applies possible missing migrations from the database.
         */
        private void Migrate()
        {
            // testing migrations
            var dbConnectionString = Configuration.GetConnectionString("DBConnection");
            var options = new DbContextOptionsBuilder<SketchDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            var context = new SketchDbContext(options);

            // always execute possible missing migrations
            if (!context.Database.GetPendingMigrations().ToList().Any())
            {
                return;
            }

            context.Database.Migrate();
        }
    }
}
