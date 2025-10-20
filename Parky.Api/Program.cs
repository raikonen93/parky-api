using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Parky.Application.Interfaces;
using Parky.Application.Mapping;
using Parky.Infrastructure.Context;
using Parky.Infrastructure.Handlers;
using Parky.Infrastructure.Services;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var isLocalIdentity = builder.Configuration.GetValue<bool>("IsLocalIdentity", true);

//SeriLog Settings+Coreletion Id
builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation(options => options.Headers.Add("X-Correlation-ID"));

builder.Host.UseSerilog((context, config) => config
    .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationIdHeader("X-Correlation-ID"));


builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
//Add swagger 
builder.Services.AddSwaggerGen(c =>
{
    if (isLocalIdentity)
    {
        c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            Description = "Enter your local username and password"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Basic"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
    else
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Parky API", Version = "v1" });

        c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://{builder.Configuration["Auth0:Domain"]}/authorize"),
                    TokenUrl = new Uri($"https://{builder.Configuration["Auth0:Domain"]}/oauth/token"),
                    Scopes = new Dictionary<string, string>
            {
                { "openid", "OpenID Connect scope" }
            }
                }
            },
            Description = "Auth0 OAuth2.0 Authorization"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "OAuth2"
                }
            },
            new[] { "openid" }
        }
    });
    }
});

//Adding health check
builder.Services.AddHealthChecks();

//adding db context
builder.Services.AddDbContext<ParkyDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//add mapping
builder.Services.AddAutoMapper(config => { }, typeof(ParkyMappingProfile));

//add authentication
builder.Services.AddAuthentication(options =>
{
    if (isLocalIdentity)
    {
        options.DefaultAuthenticateScheme = "BasicAuthentication";
        options.DefaultChallengeScheme = "BasicAuthentication";
    }
    else
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
})
.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null)
.AddJwtBearer(options =>
{
    var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
    options.Authority = domain;
    options.Audience = builder.Configuration["Auth0:Audience"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "parky-roles"
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParkyDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        if (isLocalIdentity)
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parky API v1"); ;
        }
        else
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parky API v1");
            c.OAuthClientId(builder.Configuration["Auth0:ClientId"]);
            c.OAuthClientSecret(builder.Configuration["Auth0:ClientSecret"]);
            c.OAuthUsePkce();
            c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                { "audience", builder.Configuration["Auth0:Audience"]! },
                { "prompt", "login" }
            });
            c.OAuthAppName("Parky API Swagger UI");
        }
    });
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

app.Run();
