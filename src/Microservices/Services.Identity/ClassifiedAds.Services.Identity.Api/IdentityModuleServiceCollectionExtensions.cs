﻿using ClassifiedAds.Services.Identity;
using ClassifiedAds.Services.Identity.Entities;
using ClassifiedAds.Services.Identity.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityModuleServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString, sql =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    sql.MigrationsAssembly(migrationsAssembly);
                }
            }))
                .AddScoped(typeof(IUserRepository), typeof(UserRepository))
                .AddScoped(typeof(IRoleRepository), typeof(RoleRepository));

            services.AddIdentity<User, Role>(options =>
                    {
                        options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
                    })
                    .AddUserManager<UserManager<User>>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailConfirmationTokenProvider<User>>("EmailConfirmation");
            services.AddTransient<IUserStore<User>, UserStore>();
            services.AddTransient<IRoleStore<Role>, RoleStore>();

            ConfigureOptions(services);

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });

            services.AddMessageHandlers(Assembly.GetExecutingAssembly());

            return services;
        }

        public static IServiceCollection AddIdentityModuleCore(this IServiceCollection services, string connectionString, string migrationsAssembly = "")
        {
            services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString, sql =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    sql.MigrationsAssembly(migrationsAssembly);
                }
            }))
                .AddScoped(typeof(IUserRepository), typeof(UserRepository))
                .AddScoped(typeof(IRoleRepository), typeof(RoleRepository));

            services.AddIdentityCore<User>(options =>
                    {
                        options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
                    })
                    .AddUserManager<UserManager<User>>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailConfirmationTokenProvider<User>>("EmailConfirmation");
            services.AddTransient<IUserStore<User>, UserStore>();
            services.AddTransient<IRoleStore<Role>, RoleStore>();

            ConfigureOptions(services);

            services.AddMessageHandlers(Assembly.GetExecutingAssembly());

            return services;
        }

        private static void ConfigureOptions(IServiceCollection services)
        {
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(2);
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            services.Configure<PasswordHasherOptions>(option =>
            {
                option.IterationCount = 10000;
            });
        }

        public static void MigrateIdentityDb(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.Migrate();
            }
        }
    }
}
