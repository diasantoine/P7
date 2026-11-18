using Dot.Net.WebApi.Data;
using Dot.Net.WebApi.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Dot.Net.WebApi.Security
{
    public static class SecurityExtensions
    {
        public const int MinimumPasswordLength = 8;

        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .Validate(
                    settings => settings.HasValidKey(),
                    $"Jwt:Key is missing or shorter than {JwtSettings.MinimumKeyLength} characters. "
                    + "Set it with: dotnet user-secrets set \"Jwt:Key\" \"<secret>\"")
                .ValidateOnStart();

            // AddIdentityCore (not AddIdentity): no cookie scheme, so a missing token gives 401 instead of a redirect.
            services.AddIdentityCore<User>(options => ConfigurePasswordPolicy(options.Password))
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<LocalDbContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            // Read the settings when the options are built, not while Program.cs runs,
            // so user secrets and test configuration are both taken into account.
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
                {
                    JwtSettings settings = jwtOptions.Value;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = settings.GetSigningKey(),
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            // Every endpoint requires an authenticated user unless it is marked [AllowAnonymous].
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddScoped<ITokenService, TokenService>();

            return services;
        }

        public static void ConfigurePasswordPolicy(PasswordOptions password)
        {
            password.RequiredLength = MinimumPasswordLength;
            password.RequireUppercase = true;
            password.RequireDigit = true;
            password.RequireNonAlphanumeric = true;
        }

        public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the token returned by POST /login."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}
