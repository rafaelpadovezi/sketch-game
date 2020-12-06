using AutoMapper;
using Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Sketch.Extensions;
using Sketch.Infrastructure.IoC;
using Sketch.Models;
using System;
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
                .AddCors()
                .AddApplicationCore()
                .AddHttpContextAccessor()
                .AddDbContext<SketchDbContext>(options =>
                {
                    options
                        .UseLazyLoadingProxies()
                        .UseNpgsql(Configuration.GetConnectionString("DBConnection"));

                    if (Env.IsDevelopment())
                    {
                        options
                            .UseLoggerFactory(ConsoleLoggerFactory)
                            .EnableSensitiveDataLogging();
                    }
                })
                .AddAutoMapper(typeof(Startup))
                // MVC Stuff
                .AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring start up with environment: {EnvironmentName}", Env.EnvironmentName);

            app.UseSerilogRequestLogging();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Migrate(app, logger, true);

            app.UseGameServer();
        }

        public static readonly ILoggerFactory ConsoleLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddDebug(); });

        public static void Migrate(IApplicationBuilder app, ILogger<Startup> logger, bool executeSeedDb)
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
            if (!executeSeedDb) return;

            logger.LogInformation("Seeding the database...");
            SeedDb(context, logger);
        }

        private static void SeedDb(SketchDbContext context, ILogger<Startup> logger)
        {
            logger.LogInformation(" Cleaning up old broken state");
            CleanUpOldBrokenState(context);

            if (context.GameRooms.Any())
            {
                logger.LogInformation("Database has already been seeded. Skipping it...");
                return;
            }

            logger.LogInformation("Saving entities...");
            var gameRooms = new List<GameRoom>
            {
                new GameRoom { Name = "General 1", Type = GameRoomType.General },
                new GameRoom { Name = "General 2", Type = GameRoomType.General },
                new GameRoom { Name = "Animals", Type = GameRoomType.Animals },
                new GameRoom { Name = "Harry Potter", Type = GameRoomType.HarryPotter },
            };
            var words = new List<Word>()
            {
                new Word { Content = "horse", GameRoomType = GameRoomType.Animals },
                new Word { Content = "frog", GameRoomType = GameRoomType.Animals },
                new Word { Content = "cat", GameRoomType = GameRoomType.Animals },
                new Word { Content = "seagull", GameRoomType = GameRoomType.Animals },
                new Word { Content = "ferret", GameRoomType = GameRoomType.Animals },
                new Word { Content = "monkey", GameRoomType = GameRoomType.Animals },
                new Word { Content = "ant", GameRoomType = GameRoomType.Animals },
                new Word { Content = "bee", GameRoomType = GameRoomType.Animals },
                new Word { Content = "paltypus", GameRoomType = GameRoomType.Animals },
                new Word { Content = "giraffe", GameRoomType = GameRoomType.Animals },
                new Word { Content = "starfish", GameRoomType = GameRoomType.Animals },
                new Word { Content = "shark", GameRoomType = GameRoomType.Animals },
                new Word { Content = "capybara", GameRoomType = GameRoomType.Animals },
                new Word { Content = "eagle", GameRoomType = GameRoomType.Animals },
                new Word { Content = "dragonfly", GameRoomType = GameRoomType.Animals },
                new Word { Content = "boar", GameRoomType = GameRoomType.Animals },
                new Word { Content = "cheetah", GameRoomType = GameRoomType.Animals },
                new Word { Content = "anteater", GameRoomType = GameRoomType.Animals },
                new Word { Content = "hyena", GameRoomType = GameRoomType.Animals },
                new Word { Content = "seal", GameRoomType = GameRoomType.Animals },
                new Word { Content = "penguin", GameRoomType = GameRoomType.Animals },

                new Word { Content = "spring", GameRoomType = GameRoomType.General },
                new Word { Content = "circus", GameRoomType = GameRoomType.General },
                new Word { Content = "battery", GameRoomType = GameRoomType.General },
                new Word { Content = "thief", GameRoomType = GameRoomType.General },
                new Word { Content = "palace", GameRoomType = GameRoomType.General },
                new Word { Content = "toast", GameRoomType = GameRoomType.General },
                new Word { Content = "dragonfly", GameRoomType = GameRoomType.General },
                new Word { Content = "starfish", GameRoomType = GameRoomType.General },
                new Word { Content = "baseball", GameRoomType = GameRoomType.General },
                new Word { Content = "golf", GameRoomType = GameRoomType.General },
                new Word { Content = "spoon", GameRoomType = GameRoomType.General },
                new Word { Content = "tail", GameRoomType = GameRoomType.General },
                new Word { Content = "photographer", GameRoomType = GameRoomType.General },
                new Word { Content = "penguin", GameRoomType = GameRoomType.General },
                new Word { Content = "baseball", GameRoomType = GameRoomType.General },
                new Word { Content = "Dumbo", GameRoomType = GameRoomType.General },
                new Word { Content = "ballet", GameRoomType = GameRoomType.General },
                new Word { Content = "christmas tree", GameRoomType = GameRoomType.General },
                new Word { Content = "banana peel", GameRoomType = GameRoomType.General },
                new Word { Content = "coffee", GameRoomType = GameRoomType.General },
                new Word { Content = "firefighter", GameRoomType = GameRoomType.General },
                new Word { Content = "waterfall", GameRoomType = GameRoomType.General },
                new Word { Content = "pokeball", GameRoomType = GameRoomType.General },

                new Word { Content = "muggle", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "blasted-ended skrewt", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Wingardium Leviosa", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Oliver Wood", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Severus Snape", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Dobby", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Dementor", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "prefect", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "garden gnome", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Nagini", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Expelliarmus", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Hippogriff", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Norbert", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Gringotts", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "Trevor", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "golden snitch", GameRoomType = GameRoomType.HarryPotter },
                new Word { Content = "seeker", GameRoomType = GameRoomType.HarryPotter },
            };
            context.AddRange(gameRooms);
            context.AddRange(words);

            logger.LogInformation("Database has been seeded successfully.");
            context.SaveChanges();
        }

        private static void CleanUpOldBrokenState(SketchDbContext context)
        {
            var players = context.Players.Where(x => x.IsActive);
            foreach (var player in players)
                player.IsActive = false;

            players = context.Players.Where(x => x.GameRoomId.HasValue);
            foreach (var player in players)
                player.GameRoomId = null;

            var turns = context.Turns.Where(x => !x.EndTimestamp.HasValue);
            foreach (var turn in turns)
                turn.EndTimestamp = DateTime.Now;

            var rounds = context.Rounds.Where(x => !x.EndTimestamp.HasValue);
            foreach (var round in rounds)
                round.EndTimestamp = DateTime.Now;

            context.SaveChanges();
        }
    }
}
