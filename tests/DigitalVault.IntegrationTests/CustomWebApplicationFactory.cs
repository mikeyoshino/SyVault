using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DigitalVault.Infrastructure.Data;
using DigitalVault.Application.Interfaces;
using Moq;

namespace DigitalVault.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove existing context configuration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Microsoft.EntityFrameworkCore.DbContextOptions<DigitalVault.Infrastructure.Data.ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add In-Memory DB
            services.AddDbContext<DigitalVault.Infrastructure.Data.ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
                options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Replace IStorageService with Mock
            var storageDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DigitalVault.Application.Interfaces.IStorageService));
            if (storageDescriptor != null)
            {
                services.Remove(storageDescriptor);
            }

            var mockStorage = new Moq.Mock<DigitalVault.Application.Interfaces.IStorageService>();
            mockStorage.Setup(x => x.GenerateUploadPresignedUrlAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<TimeSpan>()))
                       .ReturnsAsync("http://mock-s3-url/upload");

            services.AddScoped(_ => mockStorage.Object);

            // Auth Setup
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Note: Hangfire might still throw if it tries to connect to real Postgres. 
            // We can replace the Hangfire service but it's configured in Program.cs 'AddHangfire'.
            // For now, let's see if InMemory EF allows app startup (Hangfire uses its own connection string usually)
        });
    }
}
