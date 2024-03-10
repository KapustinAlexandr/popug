
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.Reflection;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mxm.Kafka;
using Popug.Billing.Api.Consumers;
using Popug.Billing.Api.Data;
using Popug.Billing.Api.Logic;

namespace Popug.Billing.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // ���������� �����������
        builder.Services.AddKeycloakAuthentication();

        // �������
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerKeycloak();

        // DAL
        builder.Services.AddLinqToDBContext<BillingDb>((provider, options) =>
            options
                .UsePostgreSQL("Host=db-auth;Port=5432;Database=billing;Username=keycloak;Password=keycloak;")
                .UseDefaultLogging(provider));

        // Logic
        builder.Services.AddTransient<BillingLogic>();

        // Kafka
        builder.Services
            .AddConsumer<AuthEventsConsumer>(builder.Configuration)
            .AddConsumer<TaskTrackerTaskStreamingConsumer>(builder.Configuration)
            .AddConsumer<TaskTrackerTaskAssignedConsumer>(builder.Configuration)
            .AddConsumer<TaskTrackerTaskCompletedConsumer>(builder.Configuration)
            .AddHostedService<KafkaConsumersStartupService>()
            .AddKafkaProducer(builder.Configuration);

        var app = builder.Build();

        app.UseForwardedHeaders();
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId("popug-task-api");
            });
        }

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Lax
        });

        app.MapControllers();

        app.Run();
    }
}

public static class ConfigExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
    {
        // https://dev.to/kayesislam/integrating-openid-connect-to-your-application-stack-25ch
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.MetadataAddress = "http://auth:8080/realms/popug/.well-known/openid-configuration";
                x.Authority = "http://localhost:8080";
                x.Audience = "popug-task-api";
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    NameClaimType = "preferred_username"
                };
            });

        services.AddAuthorization(o =>
        {
            o.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    public static IServiceCollection AddSwaggerKeycloak(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Popug Billing API", Version = "v1" });
            options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("http://localhost:8080/realms/popug/protocol/openid-connect/auth"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "openid" },
                            { "profile", "profile" }
                        }
                    }
                }
            });

            OpenApiSecurityScheme keycloakSecurityScheme = new()
            {
                Reference = new OpenApiReference
                {
                    Id = "Keycloak",
                    Type = ReferenceType.SecurityScheme,
                },
                In = ParameterLocation.Header,
                Name = "Bearer",
                Scheme = "Bearer",
            };

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { keycloakSecurityScheme, Array.Empty<string>() },
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}