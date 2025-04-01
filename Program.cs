using BlogProject.Application.Services;
using BlogProject.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<OpenAIService>();
builder.Services.AddHostedService<BlogGenerationService>();
builder.Services.AddSingleton<InMemoryBlogStore>();

builder.Configuration.AddUserSecrets<Program>();
var openAiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});




var app = builder.Build();

// Configure the HTTP request pipeline.
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
