using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdSupplies.Api.App.Tests;

public sealed class RequestCultureTests
{
    [Theory]
    [InlineData("", "en")]
    [InlineData("?culture=!", "en")]
    [InlineData("?culture=fr", "en")]
    [InlineData("?culture=", "en")]
    [InlineData("?culture=%20", "en")]
    [InlineData("?culture=en", "en")]
    [InlineData("?culture=cs", "cs")]
    [InlineData("?culture=CS", "cs")]
    public async Task RequestCulture_SelectsSupportedCultureOrDefault(string query, string expectedCulture)
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddTransient<IStartupFilter, CultureProbeStartupFilter>()));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync($"/__tests/culture{query}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cultures = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(new[] { expectedCulture, expectedCulture }, cultures);
    }

    private sealed class CultureProbeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => application =>
        {
            next(application);
            application.Run(context => context.Response.WriteAsJsonAsync(new[]
            {
                CultureInfo.CurrentCulture.Name,
                CultureInfo.CurrentUICulture.Name
            }));
        };
    }
}
