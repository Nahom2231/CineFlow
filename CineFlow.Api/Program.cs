using CineFlow.Infrastructure;
using CineFlow.Application;
using Application.Interfaces;
using Infrastructure.Payments;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi; 
using Infrastructure.Configuration;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using CineFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureService(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CineFlow API", Version = "v1" });
    
    var scheme = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", scheme);

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


builder.Services.Configure<ChapaOptions>(
    builder.Configuration.GetSection(ChapaOptions.SectionName));

builder.Services.Configure<EncryptionOptions>(
    builder.Configuration.GetSection(EncryptionOptions.SectionName));



builder.Services.AddHttpClient<IPaymentService, ChapaPaymentService>();


builder.Services.AddScoped<IEncryptionService, EncryptionService>();



builder.Services.AddControllers();
var app = builder.Build();


// Enable Swagger UI across environments for live API documentation & testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CineFlow API V1");
    c.RoutePrefix = "swagger";
});

// Auto-apply EF Core database migrations on startup if connection string is configured
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CineFlowDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogWarning(ex, "Automatic database migration skipped or failed during startup: {Message}", ex.Message);
    }
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.Run();


