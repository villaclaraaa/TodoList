using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Contexts;
using TodoListApp.WebApi.Services;
using TodoListApp.WebApi.Services.Authentication;

namespace TodoListApp.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        _ = builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("Bearer", null);
        builder.Services.AddAuthorization();

        builder.Services.AddDbContext<TodoListDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TodoListDb")));

        // Register the TodoListDatabaseService
        builder.Services.AddScoped<ITodoListDatabaseService, TodoListDatabaseService>();
        // Register the TaskDatabaseService
        builder.Services.AddScoped<ITaskDatabaseService, TaskDatabaseService>();

        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
