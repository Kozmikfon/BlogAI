using BlogProject.Application.Services;
using BlogProject.Application.Stores;
using BlogProject.BackgroundJobs;
using BlogProject.Core.Entities; // e�er ek olarak gerekiyorsa
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---

builder.Services.AddControllers();

// Swagger (d�k�mantasyon)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Blog API", Version = "v1" });
});

//  OpenAI i�in HttpClient ile servis kayd�
builder.Services.AddHttpClient<OpenAIService>();






// Arka plan blog �retici servis
builder.Services.AddHostedService<BlogGenerationService>();

// Bellekte blog saklayan store
builder.Services.AddSingleton<InMemoryBlogStore>();
builder.Services.AddSingleton<InMemoryCommentStore>();
// CORS: React frontend i�in
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// OpenAI API key'i gizli tut (user-secrets)
builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

// --- Middleware ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
