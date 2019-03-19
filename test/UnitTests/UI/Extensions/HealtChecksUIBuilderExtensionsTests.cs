﻿using FluentAssertions;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;
using Xunit;
namespace UnitTests.UI.Extensions
{
    public class HealtChecksUIBuilderExtensionsTests
    {
        [Fact]
        public void UseDefaultStore_configure_sql_lite_store()
        {
            var path = Assembly
                .GetAssembly(typeof(HealtChecksUIBuilderExtensionsTests))
                .Location;

            var webHost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                        .UseDefaultStore();
                })
                .UseContentRoot(Path.GetDirectoryName(path));

            var host = webHost.Build();

            var db = host.Services
                .GetRequiredService<HealthChecksDb>();

            var connection = db.Database.GetDbConnection();

            var connectionString = Path.Combine(Path.GetDirectoryName(path), "healthchecksdb");

            connection.ConnectionString
                .Should()
                .Be($"Data Source={connectionString}");

            db.Database.ProviderName
                .Should()
                .Be("Microsoft.EntityFrameworkCore.Sqlite");
        }

        [Fact]
        public void ConfigureStore_enable_custom_db_configuration()
        {
            var path = Assembly
                .GetAssembly(typeof(HealtChecksUIBuilderExtensionsTests))
                .Location;

            var webHost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI()
                        .ConfigureStore(setup=>
                        {
                            setup.UseInMemoryDatabase("healthchecksdb",null);
                        },ServiceLifetime.Singleton);
                })
                .UseContentRoot(Path.GetDirectoryName(path));

            var host = webHost.Build();

            var db = host.Services
                .GetRequiredService<HealthChecksDb>();

            db.Database.ProviderName
                .Should().Be("Microsoft.EntityFrameworkCore.InMemory");
        }
    }

    class DefaultStartup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

        }
    }
}
