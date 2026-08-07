using System.Text;
using System.Threading.RateLimiting;
using Inventory.Api.Data;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PhotoStorageService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Disables automatic remapping of short claims ("sub" -> ClaimTypes.NameIdentifier
        // etc.), which collided with the custom "uid" claim used in the controllers.
        options.MapInboundClaims = false;

        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

// Rate limiting: 15 requests/minute on the login screen (by IP, since there's no session yet)
// and on sensitive actions (by logged-in user) — deleting records, editing items/users, and
// the Dashboard report (the most expensive one to calculate).
const int RequestsPerMinuteLimit = 15;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("Login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = RequestsPerMinuteLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));

    options.AddPolicy("SensitiveActions", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.FindFirst("uid")?.Value
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = RequestsPerMinuteLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedTestUserAsync(db, builder.Configuration);
    await DataSeeder.SeedAdminUserAsync(db, builder.Configuration);

    var seedJsonPath = Path.Combine(app.Environment.ContentRootPath, "..", "..", "locals", "claude", "cadastrar.json");
    await DataSeeder.SeedTestItemsAsync(db, seedJsonPath);
}
else
{
    // HSTS tells the browser to never try http on this domain again for a while — only
    // makes sense outside Development, where the certificate is real (not the dev one).
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Sample item photos live in /img at the repository root (outside wwwroot), also served
// over http so the frontend can display them.
var imgPath = Path.Combine(app.Environment.ContentRootPath, "..", "..", "img");
if (Directory.Exists(imgPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imgPath),
        RequestPath = "/img",
    });
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
