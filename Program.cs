using Allup.Areas.Admin.Data;
using Allup.DAL;
using Allup.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Allup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddLanguageService();

            builder.Services.AddDbContext<AppDbContext>(options=>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            Constants.RootPath = builder.Environment.WebRootPath;
            Constants.FlagPath = Path.Combine(Constants.RootPath,"assets","images","flag");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseRequestLocalization(locOptions.Value);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                  name: "areas",
                  pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}"
                );
            });

            app.Run();
        }
    }
}