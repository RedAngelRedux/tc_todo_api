using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TodoLibrary.DataAccess;

namespace TodoApi.StartupConfig;

public static class DependencyInjectionExtensions
{
    public static void AddStandardServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter the Bearer Authorization string as follows: `Bearer Generated-JWT-Token`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
    public static void AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        builder.Services.AddSingleton<ITodoData, TodoData>();
    }
    public static void AddHealthCheckServices(this WebApplicationBuilder builder)
    {
        // Add Health Checks, including "can connect to SQL Server"
        string? cs = builder.Configuration.GetConnectionString("Default");
        cs = (string.IsNullOrEmpty(cs)) ? "l" : cs;
        builder.Services.AddHealthChecks()
            .AddSqlServer(cs);
    }
    public static void AddAuthServices(this WebApplicationBuilder builder)
    {

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add Authentication and Account for Missing Config Object
        string? aIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer");
        aIssuer = string.IsNullOrEmpty(aIssuer) ? "-" : aIssuer;

        string? aAudience = builder.Configuration.GetValue<string>("Authentication:Audience");
        aAudience = string.IsNullOrEmpty(aAudience) ? "-" : aAudience;

        string? aSecretkey = builder.Configuration.GetValue<string>("Authentication:SecretKey");
        aSecretkey = string.IsNullOrEmpty(aSecretkey) ? "-" : aSecretkey;

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = aIssuer,
                    ValidAudience = aAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(aSecretkey))
                };
            });

    }
}
