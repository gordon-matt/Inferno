using System.Threading.RateLimiting;
using Dependo.Autofac;
using Extenso.AspNetCore.OData;
using Inferno.Web.ContentManagement.Security;
using Inferno.Web.Infrastructure;
using InfernoCMS.Api.Infrastructure;
using InfernoCMS.Data;
using InfernoCMS.Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OData.Swagger.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new DependoAutofacServiceProviderFactory());

#region Services

var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddInfernoIdentity();

services.AddInfernoLocalization();

services
    .AddControllers(options =>
    {
        // Strip the inherited Stream action from every OData controller. Extenso
        // 10.x decorates it with a [Route] attribute that is not constrained to
        // a verb, which both confuses Swashbuckle and disrupts OData convention
        // routing for the standard CRUD methods on every derived controller.
        options.Conventions.Add(new OmitODataStreamActionConvention());
    })
    // Discover every controller-bearing assembly via the IRouterAssemblyMarker
    // pattern. This replaces explicit AddApplicationPart calls and makes new
    // referenced (or plugin) assemblies just-work as long as they ship a marker.
    .AddInfernoApplicationParts()
    .AddInfernoPlugins(configuration, builder.Environment)
    .AddOData((options, serviceProvider) =>
    {
        options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count();

        var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
        foreach (var registrar in registrars)
        {
            registrar.Register(options);
        }
    });

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddOdataSwaggerSupport();

services.AddHttpContextAccessor();

services
    .AddMemoryCache()
    .AddDistributedMemoryCache();

services.AddSwaggerGen(options =>
{
    const string jwtBearerSchemeId = "Bearer";

    // Ignore any ApiDescription that does not have an explicit HTTP method.
    // OData convention routing and certain attribute-routed endpoints can
    // produce descriptions with a null HttpMethod which would otherwise
    // cause Swashbuckle to throw an "Ambiguous HTTP method" error during
    // document generation.
    options.DocInclusionPredicate((_, api) => !string.IsNullOrEmpty(api.HttpMethod));

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inferno API",
        Version = "v1"
    });
    options.AddSecurityDefinition(jwtBearerSchemeId, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(jwtBearerSchemeId, document)] = []
    });
});

services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
services.AddResponseCompression();

services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddInfernoJwtBearer(configuration);

services.AddInfernoAuthorization(configuration);

// Register CMS-specific authorization policies (Blog, Pages, Menus, etc.). These are
// referenced from CMS OData controllers but aren't part of the core Identity module,
// so they're appended here via post-configuration of AuthorizationOptions.
services.Configure<AuthorizationOptions>(options => options.AddInfernoCmsPolicies());

// Rate limit the authentication endpoints to blunt brute-force attacks. The
// limiter is partitioned per client IP so a single attacker cannot starve the
// whole system.
services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

#endregion Services

var app = builder.Build();

#region Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inferno API V1"));

app.UseHttpsRedirection();

app.UseResponseCompression();

// Use odata route debug, /$odata
app.UseODataRouteDebug();

// Add OData /$query middleware
app.UseODataQueryRequest();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion Pipeline

app.Run();
