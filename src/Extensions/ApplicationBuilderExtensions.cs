using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sketch.Infrastructure.Connection;
using Sketch.Services;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Sketch.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseGameServer(
            this IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var logger = context
                            .RequestServices.GetService<ILogger<IApplicationBuilder>>();
                        var server = context
                            .RequestServices.GetService<IServerConnection>();

                        var connection = new PlayerConnection(webSocket);
                        var commandString = await connection.ReceiveString();
                        connection.Id = Guid.Parse(commandString);
                        server.AddPlayerConnection(connection);

                        using var loggingScope = logger.BeginScope(new { PlayerId = connection.Id });
                        using (var scope = context.RequestServices.CreateScope())
                        {
                            var service = scope.ServiceProvider.GetService<IGameService>();
                            await service.NewPlayer(connection.Id);
                        }

                        bool exitCommand = false;
                        while (!exitCommand)
                        {
                            try
                            {
                                commandString = await connection.ReceiveString();
                            }
                            catch (WebSocketException ex)
                            {
                                if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                                {
                                    logger.LogWarning(ex.Message);
                                }
                            }

                            if (!connection.IsConnected)
                            {
                                break;
                            }

                            using var scope = context.RequestServices.CreateScope();
                            var service = scope.ServiceProvider.GetService<IGameService>();
                            exitCommand = await service.NewCommand(connection.Id, commandString);
                        }

                        using (var scope = context.RequestServices.CreateScope())
                        {
                            var service = scope.ServiceProvider.GetService<IGameService>();
                            await service.PlayerLeaves(connection.Id);
                        }

                        logger.LogInformation("Player disconnected");
                        server.RemovePlayerConnection(connection);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

            return builder;
        }

        private static IDisposable InitializaLogger(
            ILogger logger, PlayerConnection connection)
        {
            var data = new Dictionary<string, object>
            {
                { "PlayerId", connection.Id }
            };
            return logger.BeginScope(data);
        }
    }
}
