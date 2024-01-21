using Microsoft.AspNetCore.HttpOverrides;
using TrevorsRidesServer;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddSingleton(RideMatchingService.Instance);

        var app = builder.Build();
        IHostApplicationLifetime lifetime = app.Lifetime;
        ILogger logger = app.Logger;
        ILoggerFactory loggerFactory = LoggerFactory.Create(loggerbuilder => loggerbuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        RideMatchingService.Logger = loggerFactory.CreateLogger<RideMatchingService>();
        lifetime.ApplicationStopping.Register(() =>
        {
            logger.LogWarning("Application stopping");
        });
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseAuthentication();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseWebSockets();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        app.Run();
    }
}