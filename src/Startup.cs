using Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Sketch.Models;
using System.Collections.Generic;
using System.Linq;

namespace Sketch
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<SketchDbContext>(options =>
                {
                    options
                        .UseNpgsql(Configuration.GetConnectionString("DBConnection"));

                    if (Env.IsDevelopment())
                    {
                        options
                            .UseLoggerFactory(ConsoleLoggerFactory)
                            .EnableSensitiveDataLogging();
                    }
                })
                .AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment _, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring start up with environment: {EnvironmentName}", Env.EnvironmentName);

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Migrate(app, logger, Env.IsDevelopment());
        }

        public static readonly ILoggerFactory ConsoleLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });

        public static void Migrate(IApplicationBuilder app, ILogger<Startup> logger, bool executeSeedDb = false)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<SketchDbContext>();

            // always execute possible missing migrations
            if (context.Database.GetPendingMigrations().ToList().Any())
            {
                logger.LogInformation("Applying migrations...");
                context.Database.Migrate();
            }

            // seeding DB only when asked
            if (!executeSeedDb)
            {
                return;
            }

            logger.LogInformation("Seeding the database...");
            SeedDb(context, logger);
        }

        private static void SeedDb(SketchDbContext context, ILogger<Startup> logger)
        {
            if (context.GameRooms.Any())
            {
                logger.LogInformation("Database has already been seeded. Skipping it...");
                return;
            }

            logger.LogInformation("Saving entities...");
            var player = new List<Player>()
            {
                new Player { Username = "player1" }
            };
            var gameRooms = new List<GameRoom>
            {
                new GameRoom
                {
                    Name = "General 1", Type = GameRoomType.General,
                    Players = new List<Player> { new Player { Username = "player2" } }
                },
                new GameRoom { Name = "General 2", Type = GameRoomType.General },
                new GameRoom { Name = "Animals", Type = GameRoomType.Animals },
                new GameRoom { Name = "Harry Potter", Type = GameRoomType.HarryPotter },
            };
            var words = new List<Word>()
            {
                new Word { Content = "horse", GameRoomType = GameRoomType.Animals },
                new Word { Content = "frog", GameRoomType = GameRoomType.Animals },
                new Word { Content = "paltypus", GameRoomType = GameRoomType.Animals },
                new Word { Content = "spring", GameRoomType = GameRoomType.General },
                new Word { Content = "circus", GameRoomType = GameRoomType.General },
                new Word { Content = "battery", GameRoomType = GameRoomType.General },
                new Word { Content = "thief", GameRoomType = GameRoomType.General },
                new Word { Content = "palace", GameRoomType = GameRoomType.General },
                new Word { Content = "toast", GameRoomType = GameRoomType.General },
                new Word { Content = "muggle", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "blasted-ended skrewt", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Wingardium Leviosa", GameRoomType = GameRoomType.HarryPotter },
            };
            context.AddRange(gameRooms);
            context.AddRange(words);

            logger.LogInformation("Database has been seeded successfully.");
            context.SaveChanges();
        }
    }
}
