using LibraryManagement.API.Extensions;
using LibraryManagement.API.Middleware;
using LibraryManagement.Auth.Extensions;
using LibraryManagement.DataAccess.Extensions;
using LibraryManagement.Services.Extensions;
using Microsoft.OpenApi;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) => logger
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    //for production
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://your-production-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the access token returned by /api/auth/login"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddDataAccessServices();
builder.Services.AddAuthServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

await app.SeedAdminAsync();

if (!app.Environment.IsDevelopment()
    && string.Equals(app.Configuration["Payment:Provider"], "Mock", StringComparison.OrdinalIgnoreCase))
{
    app.Logger.LogWarning("Payment:Provider is Mock outside Development: seats can be booked WITHOUT real payment.");
}

// Request logging wraps the exception handler so it records the final status code (e.g. 409, not 500)
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1");
    });
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("ProductionPolicy");
}

app.UseHttpsRedirection();

app.UseMediaFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
