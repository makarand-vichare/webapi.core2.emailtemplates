﻿using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Net.Core.DomainServices.IdentityStores;
using Net.Core.IDomainServices.AutoMapper;
using Net.Core.ViewModels.Identity.WebApi;
using NJsonSchema;
using NLog;
using NSwag.AspNetCore;
using System;
using System.IO;
using System.Reflection;
using WebApi.Core2.ActionFilters;
using WebApi.Core2.ContentNegotiationFormatters;

namespace WebApi.Core2
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

            Configuration = configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<IdentityUserViewModel, IdentityRoleViewModel>().AddDefaultTokenProviders();
            services.AddTransient<IUserStore<IdentityUserViewModel>, CustomUserStore>();
            services.AddTransient<IRoleStore<IdentityRoleViewModel>, CustomRoleStore>();

            services.AddMvc(config =>
            {
                // Add XML Content Negotiation
                config.RespectBrowserAcceptHeader = true;
                config.InputFormatters.Add(new XmlSerializerInputFormatter());
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                config.OutputFormatters.Add(new CsvOutputFormatter());
            });

            services.AddAutoMapper(Assembly.GetAssembly(typeof(ModelAutoMapperProfiler)));

            services.AddApiVersioning(o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));
            services.ConfigureCors();
            services.ConfigureLoggerService();
            services.ConfigureIISIntegration();
            services.ConfigureAuthentication(Configuration);
            services.AddSwagger();
            services.AddScoped<ModelValidationAttribute>();

            return services.ConfigureIoc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ConfigureCustomExceptionMiddleware(env);
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            if (env.IsDevelopment())
            {
                app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
                {
                    settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                });
           }

            app.UseMvc();
            app.Run(async (context) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Page not found");
            });
        }
    }
}
