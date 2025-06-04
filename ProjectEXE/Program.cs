using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Hubs;
using ProjectEXE.Models;
using ProjectEXE.Services;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;

namespace ProjectEXE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<RevaContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));


            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ISessionService, SessionService>();


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();

            // DI
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IShopService, ShopService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            // UserService nên ???c ??ng ký v?i Scoped lifetime
            builder.Services.AddScoped<IUserService, UserService>();

            // EmailService có th? là Transient ho?c Scoped
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<IShopOrderService, ShopOrderService>();
            builder.Services.AddScoped<IPackageService, PackageService>();
            builder.Services.AddScoped<IShopProductService, ShopProductService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IPayOsService, PayOsService>();
            builder.Services.AddScoped<IOrderConfirmationService, OrderConfirmationService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IShopServiceOfAdmin, ShopServiceOfAdmin>();
           

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(options =>
                            {
                                options.LoginPath = "/Account/Login";
                                options.LogoutPath = "/Account/Logout";
                                options.AccessDeniedPath = "/Account/AccessDenied";
                                options.Cookie.Name = "RevaAuth";
                                options.Cookie.HttpOnly = true;
                                options.Cookie.SameSite = SameSiteMode.Lax;
                                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                                options.ExpireTimeSpan = TimeSpan.FromDays(10);
                                options.SlidingExpiration = true;
                            });

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<PaymentHub>("/paymentHub");

            app.Run();
        }
    }
}
