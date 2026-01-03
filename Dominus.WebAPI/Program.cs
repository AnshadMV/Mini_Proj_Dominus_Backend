using Dominus.Application.Services;
using Dominus.Domain.Interfaces;
//using Dominus.Infrastructure.Cloudinary;
//using Dominus.Infrastructure.Cloudinary;
using Dominus.Infrastructure.Data;
using Dominus.Infrastructure.Extensions;
using Dominus.WebAPI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Security.Claims;
using System.Text;
using Serilog;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
ServicePointManager.Expect100Continue = false;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);




builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddRepositories();
builder.Services.AddServices();




var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured. Please set 'Jwt:Secret' in appsettings.json");
var key = Encoding.ASCII.GetBytes(jwtSecret);




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
        // ?? THIS IS THE KEY FIX ??
        OnMessageReceived = context =>
        {
            // Read JWT from HttpOnly cookie
            var token = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Unauthorized",
                message = "JWT token is missing or invalid"
            });

            return context.Response.WriteAsync(result);
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Forbidden",
                message = "You do not have permission to access this resource"
            });

            return context.Response.WriteAsync(result);
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
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});





builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin", "Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("user", "User"));
});





builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });




builder.Services.AddHttpContextAccessor();



builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});





builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});




builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});

builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dominus API", Version = "v1" });


    //If you want the option to choose between two servers (https://localhost:7121 and http://localhost:5180) but manage them effectively based on the environment (Development, Production, etc.), you can use IConfiguration to set which server URL to use dynamically. This way, you can keep both but not confuse users in production.
    //c.AddServer(new OpenApiServer { Url = "https://localhost:7121", Description = "HTTPS Server" });
    //c.AddServer(new OpenApiServer { Url = "http://localhost:5180", Description = "HTTP Server" });



    //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    //{
    //    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
    //    Name = "Authorization",
    //    In = ParameterLocation.Header,
    //    Type = SecuritySchemeType.ApiKey,
    //    Scheme = "Bearer"
    //});

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter ONLY your JWT token. Do NOT type 'Bearer'."
    });
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




//builder.Services.Configure<CloudinarySettings>(
//    builder.Configuration.GetSection("Cloudinary")
//);

//builder.Services.AddScoped<IImageStorageService, CloudinaryService>();


var app = builder.Build();




app.UseGlobalExceptionHandler();






using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        throw;
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dominus API v1");
        //c.RoutePrefix = "swagger";


        c.EnablePersistAuthorization();
        // Shows how long the API request took (in milliseconds) after you execute an endpoint.
        c.ConfigObject.DisplayRequestDuration = true;


        // Expand-collabsive aakkaaann
        //c.ConfigObject.DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None;


        //Allows direct URL linking to a specific endpoint or operation.
        c.ConfigObject.DeepLinking = true;


        // End point Filter option cheyyaan vendi
        //c.ConfigObject.Filter = "";  


        //Displays vendor extensions (x-*) in Swagger UI.
        //c.ConfigObject.ShowExtensions = true;

        //Shows standard OpenAPI extensions such as:
        //c.ConfigObject.ShowCommonExtensions = true;
    });
    app.UseReDoc(options =>
    {
        options.RoutePrefix = "redoc";
        options.SpecUrl = "/swagger/v1/swagger.json";
        options.DocumentTitle = "Dominus API Documentation";
    });
}

//app.MapGet("/", () => Results.Redirect("/swagger"));

// CORS must be before UseHttpsRedirection, UseAuthentication and UseAuthorization
// This ensures CORS headers are set for all requests including preflight
app.UseCors("AllowLocalDev");

// Handle preflight requests explicitly
//app.Use(async (context, next) =>
//{
//    if (context.Request.Method == "OPTIONS")
//    {
//        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
//        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
//        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");
//        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
//        context.Response.StatusCode = 200;
//        await context.Response.WriteAsync(string.Empty);
//        return;
//    }
//    await next();
//});


// Global exception handler middleware for development
//if (app.Environment.IsDevelopment())
//{
//    app.Use(async (context, next) =>
//    {
//        try
//        {
//            await next();
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Unhandled exception: {ex.Message}");
//            Console.WriteLine($"Stack trace: {ex.StackTrace}");
//            context.Response.StatusCode = 500;
//            await context.Response.WriteAsync($"An error occurred: {ex.Message}");
//        }
//    });
//}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();