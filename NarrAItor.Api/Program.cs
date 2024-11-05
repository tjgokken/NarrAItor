using NarrAItor.Api.Services;
using NarrAItor.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
// Disable Browser Link in development
builder.WebHost.UseSetting("DetailedErrors", "true");
builder.WebHost.UseSetting("SuppressStatusMessages", "true");
#endif

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
        policy.WithOrigins("https://localhost:7279") // Blazor app URL
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Register services
builder.Services.AddScoped<OpenAIService>();
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS before authorization
app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

app.Run();