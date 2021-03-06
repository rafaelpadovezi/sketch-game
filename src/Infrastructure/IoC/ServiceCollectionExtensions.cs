﻿using Microsoft.Extensions.DependencyInjection;
using Sketch.Business;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database;
using Sketch.Infrastructure.Database.Repositories;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Services;
using Sketch.Services.Interfaces;

namespace Sketch.Infrastructure.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationCore(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IRepository<>), typeof(EntityRepository<>))
                .AddScoped<IPlayerRepository, PlayerRepository>()
                .AddScoped<IPlayerService, PlayerService>()
                .AddScoped<IGameRoomRepository, GameRoomRepository>()
                .AddScoped<IGeneralRoomService, GeneralRoomService>()
                .AddScoped<IGameRoomService, GameRoomService>()
                .AddScoped<IGameService, GameService>()
                .AddScoped<IWordService, WordService>()
                .AddScoped<IRoundService, RoundService>()
                .AddSingleton<IGameLifeCycle, GameLifeCycle>()
                .AddSingleton<IServerConnection, ServerConnection>();
        }
    }
}
