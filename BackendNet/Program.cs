using BackendNet.DAL;
using BackendNet.Hubs;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository.IRepositories;
using BackendNet.Services;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(option =>
        {
            option.AddPolicy("AllowFE", builder =>
            {
                builder.WithOrigins("http://localhost:4200", "https://localhost:7104/")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
            });
        });
        builder.Services.Configure<LiveStreamDatabaseSetting>(
            builder.Configuration.GetSection("LiveStreamDatabase"));
        builder.Services.AddSingleton<ILiveStreamDatabaseSetting>(sp =>
                        sp.GetRequiredService<IOptions<LiveStreamDatabaseSetting>>().Value);
        builder.Services.AddScoped<IMongoContext, MongoContext>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IVideoRepository, VideoRepository>();
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();
        builder.Services.AddScoped<IChatliveRepository, ChatliveRepository>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IVideoService, VideoService>();
        builder.Services.AddScoped<IRoomService, RoomService>();
        builder.Services.AddScoped<IAwsService, AwsService>();
        builder.Services.AddScoped<IStreamService, StreamService>();
        builder.Services.AddScoped<IChatliveService, ChatliveService>();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Events.OnRedirectToLogin = (context) =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });
        builder.Services.AddSignalR();
        //builder.Services.AddSignalR(e =>
        //{
        //    e.MaximumReceiveMessageSize = 102400000;
        //    e.EnableDetailedErrors = true;
        //    e.KeepAliveInterval = TimeSpan.FromMinutes(5);
        //}).AddJsonProtocol(option =>
        //{
        //    option.PayloadSerializerOptions.PropertyNamingPolicy = null;
        //});
        //builder.WebHost.ConfigureKestrel(serverOption =>
        //{
        //    serverOption.ListenAnyIP(80);
        //    serverOption.ListenAnyIP(443, option =>
        //    {
        //        string filePathConfig = "/app/wwwroot/https"!;
        //        var filePaths = Directory.GetFiles(filePathConfig).ToList()[0];

        //        option.UseHttps(filePaths, "password");
        //    });
        //});
        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AllowFE");
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapHub<StreamHub>("/hub");
        app.MapHub<ChatLiveHub>("/chatHub");
        app.Run();
    }
}