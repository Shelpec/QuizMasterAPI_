using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizMasterAPI.Configurations;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Middleware;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Repositories;
using QuizMasterAPI.Services;
using System.Text;

using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 1) Очищаем провайдеры логирования
builder.Logging.ClearProviders();

// 2) Конфигурируем Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Можно выставить нужный уровень
    .WriteTo.Console()    // Логи в консоль
                          // RollingInterval.Day создаст для каждого дня свой файл (myapp-2023-01-28.txt и т.п.)
    .WriteTo.File("Logs/myapp-.txt",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7, // сколько файлов хранить
                  restrictedToMinimumLevel: LogEventLevel.Information) // например, с уровня "Information"
    .CreateLogger();

// 3) Говорим приложению использовать Serilog
builder.Host.UseSerilog(Log.Logger);

// Контроллеры
builder.Services.AddControllers();

// Подключение EF Core
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Identity
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuizDbContext>();

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = false;
    opts.SaveToken = true;
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Репозитории и сервисы
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<ITestService, TestService>();

builder.Services.AddScoped<IUserTestRepository, UserTestRepository>();
builder.Services.AddScoped<IUserTestService, UserTestService>();

builder.Services.AddScoped<IUserTestAnswerRepository, UserTestAnswerRepository>();
builder.Services.AddScoped<IUserTestAnswerService, UserTestAnswerService>();

// Swagger (документация)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware для глобального перехвата исключений
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Аутентификация + авторизация
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
