using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddControllers();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<WefaaqContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// Register services
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ClientCreateDtoValidator>();

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

// Use global exception handler
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Wefaaq API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup (optional - remove in production)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WefaaqContext>();
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

app.Run();
