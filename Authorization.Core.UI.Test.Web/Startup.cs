using Authorization.Core.UI.Test.Web.Data;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.UI.Test.Web
{
    /// <summary>
    /// Contains the configuration routines for the web appliction.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: When using the .Net 6.0 minimal API, configuration is performed in Program.cs. 
    /// It remains here because it is required by the WebApplicationFactory contained in the 
    /// integration test project. THIS CLASS IS NOT USED BY Authorization.Core.UI.Test.Web.
    /// </remarks>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString))
                .AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddAccessRightBasedAuthorization()
                .AddAuthorizationCoreUI(options => options.FriendlyAreaName = "Admin");

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
