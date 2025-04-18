using BlogProject.Application.Services;
using BlogProject.BackgroundJobs;
using BlogProject.Core.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BlogProject.Application.Agents;
using Hangfire;
using Hangfire.MemoryStorage; // ← hangfire memory
using System.Globalization;
using BlogProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BlogProject.Application.Repositories;
using BlogProject.Infrastructure.Repositories;



var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddScoped<CommentService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Blog API", Version = "v1" });
});

// HttpClient'lar
builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddHttpClient<PexelsService>();
builder.Services.AddHttpClient();

// Blog üretici agent ve store'lar

builder.Services.AddScoped<BlogAgentService>();


builder.Services.AddScoped<IBlogRepository, BlogRepository>();


// JWT
builder.Services.AddScoped<JwtService>();
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Hangfire (In-Memory kullanıyoruz)
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseDefaultTypeSerializer()
          .UseMemoryStorage());

builder.Services.AddHangfireServer(); // Worker başlat

// Background Service (manuel tetikleme için hâlâ mevcut)
//builder.Services.AddHostedService<BlogGenerationService>();


// --- App Build ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware sırası önemli
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard(); // 🚀 Hangfire arayüzü
});

// 🧠 Hangfire zamanlama
RecurringJob.AddOrUpdate<BlogAgentService>(
    "ai-blog-generator-00",
    service => service.GenerateSmartBlogAndSave("teknoloji"),
    "0 0 * * *", // 00:00
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
    }
);

RecurringJob.AddOrUpdate<BlogAgentService>(
    "ai-blog-generator-02",
    service => service.GenerateSmartBlogAndSave("teknoloji"),
    "0 2 * * *", // 02:00
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
    }
);




app.Run();
