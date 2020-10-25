using Infrastructure.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

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

        protected TestingCaseFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TStartup>();

            // constructs the testing server with the WebHostBuilder configuration
            // Startup class configures injected mocked services, and middleware (ConfigureServices, etc.)
            var server = new TestServer(builder);
            Services = server.Host.Services;

            // resolve a DbContext instance from the container and begin a transaction on the context.
            Client = server.CreateClient();
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
}
