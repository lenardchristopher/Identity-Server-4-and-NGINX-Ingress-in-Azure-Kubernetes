﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Hosting;

namespace identity_server_4
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
                options.AuthenticationDisplayName = "Windows";
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddTestUsers(TestUsers.Users);

            // in-memory, code config
            //builder.AddInMemoryIdentityResources(Config.GetIdentityResources());
            //builder.AddInMemoryApiResources(Config.GetApis());
            //builder.AddInMemoryClients(Config.GetClients());

            // in-memory, json config
            builder.AddInMemoryIdentityResources(Configuration.GetSection("IdentityResources"));
            builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            builder.AddInMemoryClients(Configuration.GetSection("clients"));

            // if (Environment.IsDevelopment())
            // {
                builder.AddDeveloperSigningCredential();
            // }
            // else
            // {
            //     throw new Exception("need to configure key material");
            // }

            services.AddAuthentication();
                // .AddGoogle(options =>
                // {
                //     options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                //     // register your IdentityServer with Google at https://console.developers.google.com
                //     // enable the Google+ API
                //     // set the redirect URI to http://localhost:5000/signin-google
                //     options.ClientId = "copy client ID from Google here";
                //     options.ClientSecret = "copy client secret from Google here";
                // });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseForwardedHeaders();

            app.UseStaticFiles();
            app.UseRouting();

            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}