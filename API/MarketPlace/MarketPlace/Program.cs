using Microsoft.EntityFrameworkCore;
using Marketplace.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with in-memory database
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
{
    options.UseInMemoryDatabase("MarketplaceDb");
});

// Allow cross-origin requests (open policy for now)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure the database is created (for InMemory provider)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();