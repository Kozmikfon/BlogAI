using BlogProject.Application.Services;
using BlogProject.Application.Stores;
using BlogProject.BackgroundJobs;
using BlogProject.Core.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BlogProject.Application.Agents;
using Hangfire;
using Hangfire.MemoryStorage; // ? hangfire memory
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---

builder.Services.AddControllers();

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
builder.Services.AddSingleton<InMemoryBlogStore>();
builder.Services.AddSingleton<InMemoryCommentStore>();
builder.Services.AddScoped<BlogAgentService>();

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

// Hangfire (In-Memory kullanýyoruz)
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseDefaultTypeSerializer()
          .UseMemoryStorage());

builder.Services.AddHangfireServer(); // Worker baþlat

// Background Service (manuel tetikleme için hâlâ mevcut)
builder.Services.AddHostedService<BlogGenerationService>();

// --- App Build ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware sýrasý önemli
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard(); // ?? Hangfire arayüzü
});

// ?? Hangfire zamanlama
RecurringJob.AddOrUpdate<BlogAgentJob>(
    "blog-12",
    job => job.GenerateScheduledBlog("12:00"),
    "0 12 * * *", TimeZoneInfo.Local);

RecurringJob.AddOrUpdate<BlogAgentJob>(
    "blog-02",
    job => job.GenerateScheduledBlog("02:00"),
    "0 2 * * *", TimeZoneInfo.Local);

app.Run();
