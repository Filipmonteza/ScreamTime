using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ScreenTime.Infrastructure;
using ScreenTime.Infrastructure.Data;
using ScreenTime.Infrastructure.Identity;
using System.Text;
using System.Security.Claims;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Invalid request.";

            return new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ogiltig begäran",
                Detail = firstError
            });
        };
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ange din JWT-token. Exempel: \"eyJhbGciOi...\" (utan ordet Bearer, Swagger lägger till det åt dig)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail("Missing user id claim.");
                return;
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                context.Fail("User not found.");
                return;
            }

            // Only consider the account deactivated if LockoutEnd is set to DateTimeOffset.MaxValue
            // Temporary lockouts (e.g. due to failed login attempts) must NOT invalidate existing JWTs.
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value == DateTimeOffset.MaxValue)
            {
                context.Fail("User account is deactivated.");
                return;
            }
        }
    };
});

// Configure ASP.NET Core Identity.
// AddIdentityCore, not AddIdentity: this API authenticates with bearer tokens only.
// AddIdentity internally calls AddAuthentication and would overwrite the JWT default
// schemes configured above with its cookie schemes.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // Password settings
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

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true when email is configured
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole<Guid>>()                 // RoleManager + role-aware user store
.AddEntityFrameworkStores<ScreenTimeDbContext>()
.AddSignInManager()                              // AuthController injects SignInManager
.AddDefaultTokenProviders() // Required for password reset, email confirmation tokens
.AddErrorDescriber<SwedishIdentityErrorDescriber>();

builder.Services.AddCors(options =>
{
    // The Vite dev server. Add the deployed origin here when there is one.
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        // Applies pending migrations and seeds, so a fresh clone only needs
        // `dotnet run` to get a working database.
        var db = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        await db.Database.MigrateAsync();
        await SeedData.SeedAsync(db);
    }

    // The Admin/Staff/Customer roles are an application invariant, not demo data:
    // registration assigns Customer and assumes it already exists. Seeded in every
    // environment, and before any user so the test user can be put in a role.
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await IdentitySeeder.SeedRolesAsync(roleManager);

    if (app.Environment.IsDevelopment())
    {
        // Seed a development test user so login can be tested
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await UserSeeder.SeedTestUserAsync(userManager);
    }
}



if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

// IMPORTANT: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose Program class for integration tests (WebApplicationFactory uses this)
public partial class Program { }
