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

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();


//builder.Services
//    .AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.ReferenceHandler =
//            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//        options.JsonSerializerOptions.MaxDepth = 64; // ��� ������, ��� 32
//    });


// ����������� EF Core:
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6; // ����������� ����� ������
    options.Password.RequireNonAlphanumeric = false; // ��������� ������� ����� !@#
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuizDbContext>();

// JWT
// ������ �������� JWT �� ������������
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

// ����������� � �������:
//builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
//builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<ITestService, TestService>();

builder.Services.AddScoped<IUserTestAnswerRepository, UserTestAnswerRepository>();
builder.Services.AddScoped<IUserTestAnswerService, UserTestAnswerService>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// ����������� ��������:
builder.Services.AddScoped<IUserTestAnswerService, UserTestAnswerService>();

// ��� ���� ���������:
builder.Services.AddScoped<IUserTestService, UserTestService>();

// ����� ������������ UserTestRepository, ���� ��� ��
builder.Services.AddScoped<IUserTestRepository, UserTestRepository>();

// ������������ ������������������ �����������:
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Swagger (������������):
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// �������� �������������� + �����������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();
