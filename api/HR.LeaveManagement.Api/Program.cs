using HR.LeaveManagement.Application;
using HR.LeaveManagement.Persistence;
using HR.LeaveManagement.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HR.LeaveManagement.Api.Middleware;
using Serilog;
using HR.LeaveManagement.Identity;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HR.LeaveManagement.Api.Configurations;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;



var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddControllers();

// Configure API Versioning
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure Swagger
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

// Configure Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var corsSection = builder.Configuration.GetSection("Cors");
var policyName = corsSection["PolicyName"] ?? "Default";
var origins = corsSection.GetSection("Origins").Get<string[]>() ?? Array.Empty<string>();

// Configure CORS policy
builder.Services.AddCors(options => {
    options.AddPolicy(policyName, policy => {
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors(policyName);

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// Enable middleware to serve generated Swagger as a JSON endpoint.
// This is moved outside of 'if (app.Environment.IsDevelopment())' 
// to ensure it's accessible during interviews/demos in any environment.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(options =>
{
    // Resolve the IApiVersionDescriptionProvider from the Service Provider
    // to dynamically generate endpoints for all registered API versions.
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        // Build the URL for the swagger.json file for each version
        // Example: /swagger/v1/swagger.json
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json", 
            $"HR Management API - {description.GroupName.ToUpperInvariant()}"
        );
    }

    // Optional: Set Swagger UI as the application's root page
    // By setting RoutePrefix to string.Empty, you can access Swagger at http://localhost:5282/
    // options.RoutePrefix = string.Empty; 

    // Enable 'Try it out' by default for easier demonstration
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
