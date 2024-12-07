using DevL2.WebAPI.HealthCheck;
using DevL2.WebAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace DevL2.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        builder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("DatabaseHealthCheck");
        builder.Services.AddHealthChecks()
            .AddCheck<ThirdPartyServiceHealthCheck>("ThirdPartyServiceHealthCheck");
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.Run();
    }
}