using TodoListApp.WebApp.Services;

namespace TodoListApp.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Register TodoListWebApiService for dependency injection
        builder.Services.AddHttpClient<ITodoListWebApiService, TodoListWebApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7065/"); // Update with your WebApi URL
        });

        builder.Services.AddScoped<ITaskWebApiService, TaskWebApiService>();
        builder.Services.AddHttpClient<ITaskWebApiService, TaskWebApiService>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7065/"); // Update with your WebApi URL
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
