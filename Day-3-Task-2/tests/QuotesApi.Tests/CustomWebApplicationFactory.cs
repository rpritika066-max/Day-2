using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Repositories;

namespace QuotesApi.Tests;

public class CustomWebApplicationFactory 
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services
                .Single(d =>
                    d.ServiceType == typeof(ICollectionRepository));

            services.Remove(descriptor);

            services.AddScoped<ICollectionRepository, FakeCollectionRepository>();
        });
    }
}