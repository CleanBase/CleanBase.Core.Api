using CleanBase.Core.Api.Authorization;
using CleanBase.Core.Api.Swagger;
using CleanBase.Core.Data.Policies.Base;
using CleanBase.Core.Infrastructure.Extensions;
using CleanBase.Core.Infrastructure.Policies;
using CleanBase.Core.Security;
using CleanBase.Core.Services.Core;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Services.Storage;
using CleanBase.Core.Settings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CleanBase.Core.Api
{
    public abstract class StartupBase<TSetting> where TSetting : AppSettings
    {
        private const string CorsPolicyName = "AllOrigins";
        public IConfiguration Configuration { get; }
        protected bool IsSupportHsts => false;
        protected bool EnableAuth { get; set; } = true;
        protected TSetting Settings { get; set; }
        protected IServiceCollection Services { get; private set; }

        public StartupBase(IConfiguration configuration, string primaryConfigSection, bool enableAuth = true)
        {
            Configuration = configuration;
            EnableAuth = enableAuth;
            InitializeSettings(primaryConfigSection);
        }

        private void InitializeSettings(string primaryConfigSection)
        {
            var section = Configuration.GetSection(primaryConfigSection);
            Services.Configure<TSetting>(section);
            Settings = section.Get<TSetting>();
            AppSettings<TSetting>.Instance = Settings;
            Services.AddSingleton(typeof(AppSettings), Settings);
            Services.AddSingleton(Settings.GetType(), Settings);
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            Services = services;
            ConfigureCoreServices();
            ConfigureAuthentication();
            ConfigureMvc();
            ConfigureSwagger();
            ConfigureApplicationInsights();
            ConfigureCors();
            ConfigureFluentValidation();
            ConfigureAdditionalServices();
        }

        private void ConfigureCoreServices()
        {
            Services.AddSingleton<ILogger>(Serilog.Log.Logger);
            Services.AddScoped<ISmartLogger, SmartLogger>();
            Services.AddScoped<IIdentityProvider, IdentityProvider>();
            Services.AddScoped<IPolicyFactory, PolicyFactory>();
            Services.AddScoped<ICoreProvider, CoreProvider>();
            Services.AddScoped(typeof(IStorageService<>), typeof(StorageService<>));
            Services.RegisterDefaultInfrastructure();
        }

        private void ConfigureMvc()
        {
            Services.AddControllers(cfg =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                cfg.Filters.Add(new AuthorizeFilter(policy));
            });
        }

        private void ConfigureAuthentication()
        {
            if (!EnableAuth) return;

            IdentityModelEventSource.ShowPII = true;
            Services.AddSingleton<IAuthorizationHandler, RolePermissionHandler>();
            Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

            Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    UpdateOpenIDConfiguration();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Settings.Auth.Authority,
                        ValidateIssuer = true,
                        ValidateAudience = !string.IsNullOrEmpty(Settings.Auth.Audience),
                        ValidAudience = Settings.Auth.Audience,
                        RequireSignedTokens = !string.IsNullOrEmpty(Settings.Auth.Modulus),
                        ValidateIssuerSigningKey = !string.IsNullOrEmpty(Settings.Auth.Modulus),
                        IssuerSigningKey = !string.IsNullOrEmpty(Settings.Auth.Modulus) 
                            ? CryptoGraphyHelper.SigningKey(Settings.Auth.Modulus, Settings.Auth.Exponent) 
                            : null
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = OnTokenValidated
                    };
                });
        }

        private async Task OnTokenValidated(TokenValidatedContext context)
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            if (identity?.IsAuthenticated == true)
            {
                BuildAuthorizations(identity);
                var provider = context.HttpContext.RequestServices.GetService<IIdentityProvider>();
                var logger = context.HttpContext.RequestServices.GetService<ISmartLogger>();
                await provider.UpdateIdentity(context.Principal, (context.SecurityToken as JwtSecurityToken)?.RawData);
                logger.UserName = provider.Identity.UserName;
            }
        }

        private void ConfigureSwagger()
        {
            Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{Settings.AppId} API", Version = "v1" });
                c.SchemaFilter<EnumSchemaFilter>();
                if (!string.IsNullOrEmpty(Settings.PathBase))
                    c.DocumentFilter<BasePathFilter>(Settings.PathBase);
                if (EnableAuth)
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter 'Bearer' followed by a space and JWT",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                }
            });
        }

        private void ConfigureApplicationInsights()
        {
            if (!string.IsNullOrEmpty(Settings.ApplicationInsights.InstrumentationKey))
                Services.AddApplicationInsightsTelemetry(Settings.ApplicationInsights.InstrumentationKey);
        }

        private void ConfigureCors()
        {
            Services.AddCors(p => p.AddPolicy(CorsPolicyName, builder =>
            {
                if (string.IsNullOrEmpty(Settings.AllowedHosts) || Settings.AllowedHosts.Contains("*"))
                    builder.AllowAnyOrigin();
                else
                    builder.WithOrigins(Settings.AllowedHostsValues);
                builder.AllowAnyHeader().AllowAnyMethod();
            }));
        }

        private void ConfigureFluentValidation()
        {
            Services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters()
                    .AddValidatorsFromAssemblies(ValidatorAssemblies());
        }

        protected abstract IEnumerable<Assembly> ValidatorAssemblies();
        protected abstract void ConfigureAdditionalServices();
        protected abstract void BuildAuthorizations(ClaimsIdentity identity);
        protected abstract void UpdateOpenIDConfiguration();
    }
}
