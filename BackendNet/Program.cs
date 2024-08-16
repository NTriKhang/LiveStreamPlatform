using BackendNet.DAL;
using BackendNet.Hubs;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BackendNet",
                Version = "v1",
                Description = "This is a sample API for demonstration purposes."
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
            {
                Description = "Api key needed to access the endpoints. Authorization: Bearer xxxx",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Authorization"
                            },
                        },
                        new string[] {}
                    }
            });
        });
        builder.Services.AddCors(option =>
        {
            option.AddPolicy("AllowFE", builder =>
            {
                builder.WithOrigins("http://localhost:4200", "https://localhost:7104/", "https://stream.hightfive.click")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
            });
        });

        builder.Services.Configure<LiveStreamDatabaseSetting>(
            builder.Configuration.GetSection("LiveStreamDatabase"));
        builder.Services.Configure<EmailSetting>(
            builder.Configuration.GetSection("EmailServerSetting"));
        builder.Services.AddSingleton<ILiveStreamDatabaseSetting>(sp =>
                        sp.GetRequiredService<IOptions<LiveStreamDatabaseSetting>>().Value);
        builder.Services.AddSingleton(sp =>
                        sp.GetRequiredService<IOptions<EmailSetting>>().Value);

        builder.Services.AddScoped<IMongoContext, MongoContext>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(cfg =>
        {
            return ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis")!);
        });

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IVideoRepository, VideoRepository>();
        builder.Services.AddScoped<IRoomRepository, RoomRepository>();
        builder.Services.AddScoped<IChatliveRepository, ChatliveRepository>();
        builder.Services.AddScoped<IFollowRepository, FollowRepository>();
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IVideoService, VideoService>();
        builder.Services.AddScoped<IRoomService, RoomService>();
        builder.Services.AddScoped<IAwsService, AwsService>();
        builder.Services.AddScoped<IStreamService, StreamService>();
        builder.Services.AddScoped<IChatliveService, ChatliveService>();
        builder.Services.AddScoped<IFollowService, FollowService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ICourseService, CourseService>();

        builder.Services.AddAuthentication(cfg => {
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x => {
            x.RequireHttpsMetadata = false;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8
                    .GetBytes("this is my top jwt secret key for authentication and i append it to have enough lenght")
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddSignalR();
        builder.Services.AddAutoMapper(typeof(Program));

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient();
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
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackendNet");
        });

        app.UseCors("AllowFE");
        app.UseHttpsRedirection();

        app.UseMiddleware<JwtCookieMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapHub<StreamHub>("/hub");
        app.MapHub<ChatLiveHub>("/chatHub");
        app.Run();
    }
}