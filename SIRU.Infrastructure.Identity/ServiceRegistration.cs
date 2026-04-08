using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SIRU.Core.Application.Interfaces.Accounts;
using SIRU.Core.Application.Interfaces.Auth;
using SIRU.Core.Application.Interfaces.Users;
using SIRU.Core.Domain.Settings;
using SIRU.Infrastructure.Identity.Contexts;
using SIRU.Infrastructure.Identity.Seeds;
using SIRU.Infrastructure.Identity.Services;
using System.Text;

namespace SIRU.Infrastructure.Identity;

public static class ServicesRegistration
{
    public static void AddIdentityLayerIoc(this IServiceCollection services, IConfiguration config)
    {
        GeneralConfiguration(services, config);

        #region Configurations

        services.Configure<TokenSettings>(config.GetSection("TokenSettings"));

        #endregion

        #region Identity

        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequiredLength = 8;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;

            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            opt.Lockout.MaxFailedAccessAttempts = 5;

            opt.User.RequireUniqueEmail = true;
            opt.SignIn.RequireConfirmedEmail = true;
        });

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>(TokenOptions.DefaultProvider);

        services.Configure<DataProtectionTokenProviderOptions>(opt =>
        {
            opt.TokenLifespan = TimeSpan.FromHours(12);
        });

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(opt =>
        {
            //Cambiar true cuando pase a produccion
            opt.RequireHttpsMetadata = false;
            opt.SaveToken = false;
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidIssuer = config["TokenSettings:Issuer"],
                ValidAudience = config["TokenSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenSettings:SecretKey"] ?? "")),
            };
            opt.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = af =>
                {
                    af.HttpContext.Items["auth_error"] = "Token inválido";
                    return Task.CompletedTask;
                },
                OnChallenge = c =>
                {
                    c.HandleResponse();
                    c.Response.StatusCode = 401;
                    c.Response.ContentType = "application/json";
                    var result = JsonConvert.SerializeObject(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = c.HttpContext.Items["auth_error"]?.ToString() ?? "No estás autenticado en el sistema",
                        Status = c.Response.StatusCode,
                        Instance = c.HttpContext.Request.Path
                    });

                    return c.Response.WriteAsync(result);
                },
                OnForbidden = c =>
                {
                    c.Response.StatusCode = 403;
                    c.Response.ContentType = "application/json";
                    var result = JsonConvert.SerializeObject(new ProblemDetails
                    {
                        Title = "Forbidden",
                        Detail = "No estás autorizado para acceder a este recurso",
                        Status = c.Response.StatusCode,
                        Instance = c.HttpContext.Request.Path
                    });
                    return c.Response.WriteAsync(result);
                }
            };
        });

        #endregion

        #region Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRoleService, RoleService>();
        #endregion

    }

    public static async Task RunIdentitySeedAsync(this IServiceProvider service)
    {
        using var scope = service.CreateScope();
        var servicesProvider = scope.ServiceProvider;
        var userManager = servicesProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userService = servicesProvider.GetRequiredService<IUserService>();

        await DefaultRoles.SeedAsync(roleManager);
        await DefaultSuperAdmin.SeedAsync(userManager, userService);
    }

    #region Private methods

    private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
    {
        #region Contexts
        var connectionString = config.GetConnectionString("AuthDbConnection");
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
                });
        });
        #endregion
    }

    #endregion
}