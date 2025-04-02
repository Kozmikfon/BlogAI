using BlogProject.Application.Services;
using BlogProject.Application.Stores;
using BlogProject.BackgroundJobs;
using BlogProject.Core.Entities; // eðer ek olarak gerekiyorsa
using Microsoft.OpenApi.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---

builder.Services.AddControllers();

// Swagger (dökümantasyon)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Blog API", Version = "v1" });
});

//  OpenAI için HttpClient ile servis kaydý
builder.Services.AddHttpClient<OpenAIService>();






// Arka plan blog üretici servis
builder.Services.AddHostedService<BlogGenerationService>();

// Bellekte blog saklayan store
builder.Services.AddSingleton<InMemoryBlogStore>();
builder.Services.AddSingleton<InMemoryCommentStore>();
// CORS: React frontend için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// OpenAI API key'i gizli tut (user-secrets)
builder.Configuration.AddUserSecrets<Program>();

//jwt
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


var app = builder.Build();

// --- Middleware ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
