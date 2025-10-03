using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Identity.Contexts;
using TodoListApp.Identity.Models;
using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        _ = builder.Services.AddControllersWithViews();
        _ = builder.Services.AddRazorPages();

        _ = builder.Services.AddTransient<IEmailSender, EmailSender>();
        // Register TodoListWebApiService for dependency injection

        var secretBearerToken = builder.Configuration["Authentication:BearerToken"];
        _ = builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7065/"); // Update with your WebApi URL
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretBearerToken);
        });

        _ = builder.Services.AddScoped<ITaskWebApiService, TaskWebApiService>();
        _ = builder.Services.AddScoped<IUserService, UserService>();
        _ = builder.Services.AddHttpClient<ITaskWebApiService, TaskWebApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7065/"); // Update with your WebApi URL
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretBearerToken);
        });

        _ = builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

        _ = builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddDefaultTokenProviders();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            _ = app.UseHsts();
        }
        else
        {
            _ = app.UseDeveloperExceptionPage();
        }

        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();
        _ = app.MapRazorPages();
        _ = app.UseRouting();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();

        _ = app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}



