using AutoMapper;
using EmployeeManagement.BusinessEngine.Contracts;
using EmployeeManagement.BusinessEngine.Implementaion;
using EmployeeManagement.Common.ConstantsModels;
using EmployeeManagement.Common.EmailOperationModels;
using EmployeeManagement.Common.Mappings;
using EmployeeManagement.Data.Contracts;
using EmployeeManagement.Data.DataContext;
using EmployeeManagement.Data.DbModels;
using EmployeeManagement.Data.Implementaion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.UI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(lang => { lang.ResourcesPath = "Resources"; });
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
            services.Configure<RequestLocalizationOptions>(conf =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("es"),
                    new CultureInfo("fr"),
                    new CultureInfo("tr")
                };

                conf.DefaultRequestCulture = new RequestCulture("en");
                conf.SupportedCultures = supportedCultures;
                conf.SupportedUICultures = supportedCultures;

            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")=="Production")
            {
                services.AddDbContext<UdemyEmployeeManagementContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("AzureConnection")));
            }
            else
            {
                services.AddDbContext<UdemyEmployeeManagementContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("IdentityConnection")));

            }
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            services.BuildServiceProvider().GetService<UdemyEmployeeManagementContext>()?.Database.Migrate();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'


            services.AddSingleton<IEmailSender, EmailSender>();
            services.Configure<EmailOptions>(Configuration);
            //*********************************************************//
            services.AddScoped<IEmployeeLeaveTypeBusinessEngine, EmployeeLeaveTypeBusinessEngine>();
            services.AddScoped<IEmployeeLeaveRequestBusinessEngine, EmployeeLeaveRequestBusinessEngine>();
            services.AddScoped<IEmployeeLeaveAssignBusinessEngine, EmployeeLeaveAssignBusinessEngine>();
            services.AddScoped<IWorkOrderBusinessEngine, WorkOrderBusinessEngine>();
            services.AddScoped<IEmployeeBusinessEngine, EmployeeBusinessEngine>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //*********************************************************//
            services.AddAutoMapper(typeof(Maps));
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<UdemyEmployeeManagementContext>();

            services.AddIdentity<Employee, IdentityRole>().AddDefaultTokenProviders()
                .AddEntityFrameworkStores<UdemyEmployeeManagementContext>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddMvc();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            UserManager<Employee> userManager, RoleManager<IdentityRole> roleManager)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            SeedData.Seed(userManager, roleManager);
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            //var supportedCultures = new[] { "en", "fr", "es", "tr" };
            //var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[3])
            //    .AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
            //app.UseRequestLocalization(localizationOptions);

            app.UseRequestLocalization(app.ApplicationServices
                .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
