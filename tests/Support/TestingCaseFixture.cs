using Infrastructure.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Sketch.Infrastructure.Connection;
using Sketch.Models;
using Sketch.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests.Support
{
    public class TestingCaseFixture<TStartup> : IDisposable where TStartup : class
    {
        // private testing properties
        private readonly IDbContextTransaction _transaction;

        public IServiceProvider Services { get; }

        // properties used by testing classes
        protected readonly HttpClient Client;
        protected SketchDbContext DbContext { get; }
        protected IServerConnection Server { get; }

        protected TestingCaseFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseEnvironment("Testing")
                .UseStartup<TStartup>()
                .ConfigureTestServices(services =>
                {
                    services.Replace(
                        new ServiceDescriptor(
                            typeof(IWordService),
                            typeof(TestWordService),
                            ServiceLifetime.Scoped));
                });

            // constructs the testing server with the WebHostBuilder configuration
            // Startup class configures injected mocked services, and middleware (ConfigureServices, etc.)
            var server = new TestServer(builder);
            Services = server.Host.Services;

            // resolve a DbContext instance from the container and begin a transaction on the context.
            Client = server.CreateClient();
            Server = Services.GetRequiredService<IServerConnection>();
            DbContext = Services.GetRequiredService<SketchDbContext>();
            _transaction = DbContext.Database.BeginTransaction();
        }

        public void Dispose()
        {
            if (_transaction == null)
            {
                return;
            }

            _transaction.Rollback();
            _transaction.Dispose();
        }
    }

    public class TestWordService : IWordService
    {
        public Task<Word> PickWord(GameRoomType type)
        {
            return Task.FromResult(new Word { Content = "TestWord" });
        }
    }
}
