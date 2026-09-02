using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScreenTime.Infrastructure.Identity;
using ScreenTime.Infrastructure;
using ScreenTime.Infrastructure.Data;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Infrastructure services (DbContext, repositories, JWT service, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing from configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };
});

// Configure ASP.NET Core Identity.
// AddIdentityCore, not AddIdentity: this API authenticates with bearer tokens only.
// AddIdentity internally calls AddAuthentication and would overwrite the JWT default
// schemes configured above with its cookie schemes.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Password settings (adjust based on your security requirements)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings (prevent brute force attacks)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings (important for JWT later)
    options.SignIn.RequireConfirmedEmail = false; // Set to true when email is configured
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole<Guid>>()                 // RoleManager + role-aware user store
.AddEntityFrameworkStores<ScreenTimeDbContext>()
.AddSignInManager()                              // AuthController injects SignInManager
.AddDefaultTokenProviders(); // Required for password reset, email confirmation tokens

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

    // Seed a development test user so login can be tested
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await ScreenTime.Infrastructure.Data.UserSeeder.SeedTestUserAsync(userManager);
}

app.UseHttpsRedirection();

// IMPORTANT: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors(FrontendCorsPolicy);

app.Run();
