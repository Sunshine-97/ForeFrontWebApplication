using System.Threading.RateLimiting;
using System.Text;
using ForeFrontWebApplication.Data;
using ForeFrontWebApplication.Repositories.Order;
using ForeFrontWebApplication.Repositories.Warehouse;
using ForeFrontWebApplication.Services;
using ForeFrontWebApplication.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ?? JWT settings ?????????????????????????????????????????????????????????????
// Supply Jwt:SigningKey via environment variable or secrets manager in production.
// Never commit a real signing key to source control.
var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        $"Required configuration section '{JwtSettings.SectionName}' is missing.");

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// ?? Authentication ????????????????????????????????????????????????????????????
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            // Remove the default 5-minute clock-skew tolerance for tighter expiry enforcement
            ClockSkew = TimeSpan.Zero,
        };
    });

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing.")));

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository,    EfOrderRepository>();
builder.Services.AddScoped<IWarehouseRepository, EfWarehouseRepository>();
builder.Services.AddScoped<IOrderService,        OrderService>();
builder.Services.AddScoped<IWarehouseService,    WarehouseService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IAuthService,  AuthService>();

// ?? Authorization ?????????????????????????????????????????????????????????????
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Customer", policy => policy.RequireRole("Customer"))
    .AddPolicy("Warehouse", policy => policy.RequireRole("Warehouse"))
    .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

// ?? Rate Limiting ?????????????????????????????????????????????????????????????
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("ReadById", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 60;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("Mutate", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 20;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });
});

// ?? MVC / API ?????????????????????????????????????????????????????????????????
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. The 'Bearer ' prefix is added automatically.",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            []
        },
    });
});

var app = builder.Build();

// ?? HTTP pipeline ?????????????????????????????????????????????????????????????
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRateLimiter();

// Authentication must run before Authorization so the JWT is validated
// and HttpContext.User is populated before any [Authorize] policy is evaluated.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

