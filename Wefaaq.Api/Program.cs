using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Wefaaq.Api.Auth;
using Wefaaq.Api.Extensions;
using Wefaaq.Api.Middleware;
using Wefaaq.Bll.Interfaces;
using Wefaaq.Bll.Mappings;
using Wefaaq.Bll.Services;
using Wefaaq.Bll.Validators;
using Wefaaq.Dal;
using Wefaaq.Dal.Repositories;
using Wefaaq.Dal.RepositoriesInterfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly)
    .AddControllersAsServices()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Configure DbContext with SQL Server
// Get connection string from environment variable (Railway) or appsettings.json (local)
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<WefaaqContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClientDeletionService, ClientDeletionService>();
builder.Services.AddScoped<IClientBranchService, ClientBranchService>();
builder.Services.AddScoped<IExternalWorkerService, ExternalWorkerService>();
builder.Services.AddScoped<IUserPaymentService, UserPaymentService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ClientCreateDtoValidator>();

// Configure Firebase Authentication and JWT Bearer
builder.Services.AddFirebaseAuthentication(builder.Configuration);

// Add claims transformation for role injection
builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();

// Add authorization policies
builder.Services.AddAuthorizationPolicies();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Wefaaq API",
        Version = "v1",
        Description = "API for managing clients and organizations"
    });

    // Add JWT Bearer security definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your Firebase ID token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Wefaaq API v1");
    options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseCors("AllowAll");

// Global exception handler - catches unhandled exceptions and returns proper JSON responses
app.UseGlobalExceptionHandler();

// Disable HTTPS redirection in production (Railway handles SSL at proxy level)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseAutoWrapperConfig(apiVersion: "1.0");

app.MapControllers();

// Apply migrations and seed data on startup (optional - remove in production)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WefaaqContext>();
        try
        {
            db.Database.Migrate();

            // Seed data
            DataSeeder.SeedData(db);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}

// Railway port configuration (only in production)
if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.Run();
