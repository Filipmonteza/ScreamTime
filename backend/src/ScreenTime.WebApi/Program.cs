using Microsoft.EntityFrameworkCore;
using ScreenTime.Infrastructure;
using ScreenTime.Infrastructure.Data;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    // The Vite dev server. Add the deployed origin here when there is one.
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// TODO: Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Applies pending migrations and seeds, so a fresh clone only needs
    // `dotnet run` to get a working database.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(FrontendCorsPolicy);

app.Run();
