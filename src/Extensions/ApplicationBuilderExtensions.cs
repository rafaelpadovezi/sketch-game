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
                        var service = context.
                            RequestServices.GetService<IGameService>();
                        var logger = context
                            .RequestServices.GetService<ILogger<IApplicationBuilder>>();

                        var connection = new PlayerConnection(webSocket);
                        var commandString = await connection.ReceiveString();
                        connection.Id = Guid.Parse(commandString);

                        using var scope = logger.BeginScope(new { PlayerId = connection.Id });

                        await service.NewConnection(connection);

                        bool exitCommand = false;
                        while (!exitCommand)
                        {
                            commandString = await connection.ReceiveString();
                            service = context.
                                RequestServices.GetService<IGameService>();
                            exitCommand = await service.NewCommand(connection, commandString);
                        }
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
