// Dominus.WebAPI / Program.cs
using Dominus.Application.Services;
using Dominus.Infrastructure.Data;
using Dominus.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models; // <- required for OpenApi types
using System;
using System.Security.Claims;
using System.Text;
using System.Net;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
ServicePointManager.Expect100Continue = false;






var builder = WebApplication.CreateBuilder(args);

// DB (WebAPI references Infrastructure for AppDbContext)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// AutoMapper, Repos, Services (extension methods from Infrastructure)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddRepositories();
builder.Services.AddServices();

// JWT secret
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured. Please set 'Jwt:Secret' in appsettings.json");
var key = Encoding.ASCII.GetBytes(jwtSecret);

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            Console.WriteLine("Token validated. Claims:");
            foreach (var c in ctx.Principal.Claims)
                Console.WriteLine($"{c.Type} => {c.Value}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine("AuthenticationFailed: " + ctx.Exception?.Message);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5180",
                "https://localhost:7121"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // For preflight requests
    });
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin", "Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("user", "admin", "User", "Admin"));
    options.AddPolicy("Customer", policy => policy.RequireRole("user", "User"));
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// After builder.Services.AddControllers()
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // 2GB limit
    options.MemoryBufferThreshold = int.MaxValue;
});

// Also configure Kestrel server limits
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
    // Increase request timeout to handle large file uploads (10 minutes)
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

// Configure request timeout for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddEndpointsApiExplorer();

// Swagger � defined in Web project only
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dominus API", Version = "v1" });

    // Add servers to OpenAPI spec
    c.AddServer(new OpenApiServer { Url = "https://localhost:7121", Description = "HTTPS Server" });
    c.AddServer(new OpenApiServer { Url = "http://localhost:5180", Description = "HTTP Server" });

    // JWT bearer definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Use Reference-only scheme in the requirement (recommended pattern)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});
builder.Services.AddScoped<CloudinaryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dominus API v1");
        c.RoutePrefix = "swagger";
        // Enable request duration display
        c.ConfigObject.DisplayRequestDuration = true;
        c.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None;
        // Enable CORS for Swagger UI (allows file uploads)
        c.ConfigObject.DeepLinking = true;
        c.ConfigObject.Filter = "";
        c.ConfigObject.ShowExtensions = true;
        c.ConfigObject.ShowCommonExtensions = true;
    });
    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl = "/swagger/v1/swagger.json";
        options.DocumentTitle = "Dominus API Documentation";
    });
}

app.MapGet("/", () => Results.Redirect("/swagger"));

// CORS must be before UseHttpsRedirection, UseAuthentication and UseAuthorization
// This ensures CORS headers are set for all requests including preflight
app.UseCors("AllowLocalDev");

// Handle preflight requests explicitly
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync(string.Empty);
        return;
    }
    await next();
});

app.UseHttpsRedirection();

// Global exception handler middleware for development
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"An error occurred: {ex.Message}");
        }
    });
}

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
