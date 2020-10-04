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

            Migrate(app, logger, Env.IsDevelopment());

            app.UseGameServer();
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
