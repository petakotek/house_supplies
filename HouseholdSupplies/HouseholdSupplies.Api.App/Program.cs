using System.Globalization;
using FluentValidation;
using HouseholdSupplies.Api.App;
using HouseholdSupplies.Api.App.Extensions;
using HouseholdSupplies.Api.App.Middlewares;
using HouseholdSupplies.Api.App.Processors;
using HouseholdSupplies.Api.App.Validators;
using HouseholdSupplies.Api.BL.Installers;
using HouseholdSupplies.Api.BL.Mappers;
using HouseholdSupplies.Api.DAL.Extensions;
using HouseholdSupplies.Api.DAL.Installers;
using HouseholdSupplies.Common;
using HouseholdSupplies.Common.Extensions;
using HouseholdSupplies.Common.Models.Item;
using HouseholdSupplies.Common.Models.User;
using HouseholdSupplies.Common.Models.Place;
using HouseholdSupplies.Common.Models.ShoppingList;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder();

ConfigureOptions(builder.Services);
ConfigureCors(builder.Services);
ConfigureLocalization(builder.Services);

ConfigureOpenApiDocuments(builder.Services);
ConfigureDependencies(builder.Services, builder.Configuration);
ConfigureMapperly(builder.Services);
ConfigureValidators(builder.Services);
ConfigureAuthorization(builder.Services);

var app = builder.Build();

UseDevelopmentSettings(app);
UseSecurityFeatures(app);
UseLocalization(app);
app.UseRequestErrorResponses();
UseRouting(app);
UseAuthorization(app);
UseEndpoints(app);
UseOpenApi(app);

app.Lifetime.ApplicationStarted.Register(() =>
{
    foreach (var url in app.Urls)
    {
        app.Logger.LogInformation("Swagger UI available at: {SwaggerUrl}", $"{url.TrimEnd('/')}/swagger");
    }
});

app.Run();

void ConfigureOptions(IServiceCollection serviceCollection)
{
    serviceCollection.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
    serviceCollection.Configure<IdentityOptions>(builder.Configuration.GetSection(nameof(IdentityOptions)));
}

void ConfigureCors(IServiceCollection serviceCollection)
{
    serviceCollection.AddCors(options =>
    {
        options.AddDefaultPolicy(o =>
            o.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

void ConfigureLocalization(IServiceCollection serviceCollection)
{
    serviceCollection.AddLocalization(options => options.ResourcesPath = string.Empty);
}

void ConfigureOpenApiDocuments(IServiceCollection serviceCollection)
{
    serviceCollection.AddEndpointsApiExplorer();
    serviceCollection.AddOpenApiDocument(settings =>
    {
        settings.OperationProcessors.Add(new RequestCultureOperationProcessor());
        settings.OperationProcessors.Add(new ResponseMetadataOperationProcessor());
    });
}

void ConfigureDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentException("The connection string is missing");
    serviceCollection.AddInstaller<ApiDALInstaller>(connectionString);
    serviceCollection.AddInstaller<ApiBLInstaller>();
}

void ConfigureMapperly(IServiceCollection serviceCollection)
{
    serviceCollection.AddSingleton<ItemMapper>();
    serviceCollection.AddSingleton<UserMapper>();
    serviceCollection.AddSingleton<PlaceMapper>();
    serviceCollection.AddSingleton<ShoppingListMapper>();
    serviceCollection.AddSingleton<ShoppingListItemMapper>();
}

void ConfigureValidators(IServiceCollection serviceCollection)
{
    serviceCollection.AddScoped<IValidator<ItemDetailModel>, ItemDetailModelValidator>();
    serviceCollection.AddScoped<IValidator<UserListModel>, UserListModelValidator>();
    serviceCollection.AddScoped<IValidator<PlaceDetailModel>, PlaceDetailModelValidator>();
    serviceCollection.AddScoped<IValidator<ShoppingListDetailModel>, ShoppingListDetailModelValidator>();
}

void ConfigureAuthorization(IServiceCollection serviceCollection)
{
    serviceCollection.AddAuthorizationBuilder()
        .AddPolicy(ApiPolicies.PlaceAdmin, policy =>
            policy.RequireAuthenticatedUser().RequireRole(UserRoles.Admin))
        .AddPolicy(ApiPolicies.ShoppingListAdmin, policy =>
            policy.RequireAuthenticatedUser().RequireRole(UserRoles.Admin));

    serviceCollection.AddHttpContextAccessor();
}

void UseAuthorization(IApplicationBuilder application)
{
    application.UseAuthorization();
}

void UseEndpoints(WebApplication application)
{
    var endpointsBase = application.MapGroup("api");

    var identityOptions = application.Services.GetRequiredService<IOptions<IdentityOptions>>();

    endpointsBase.UseItemEndpoints();
    endpointsBase.UseUserEndpoints();
    endpointsBase.UsePlaceEndpoints(identityOptions);
    endpointsBase.UseShoppingListEndpoints(identityOptions);
}

void UseDevelopmentSettings(WebApplication application)
{
    var environment = application.Services.GetRequiredService<IWebHostEnvironment>();

    if (environment.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
}

void UseSecurityFeatures(IApplicationBuilder application)
{
    application.UseCors();
    application.UseHttpsRedirection();
}

void UseLocalization(IApplicationBuilder application)
{
    application.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture(new CultureInfo("en")),
        SupportedCultures = [new("en"), new("cs")]
    });

    application.UseRequestCulture();
}

void UseRouting(IApplicationBuilder application)
{
    application.UseRouting();
}

void UseOpenApi(IApplicationBuilder application)
{
    application.UseOpenApi();
    application.UseSwaggerUi();
}

public partial class Program
{
}
