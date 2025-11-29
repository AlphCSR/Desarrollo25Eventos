using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using UsersMS.Application;
using UsersMS.Domain.Interfaces;
using UsersMS.Infrastructure.Persistence;
using UsersMS.Infrastructure.Repositories;
using UsersMS.Application.Interfaces;
using UsersMS.Infrastructure.Services;
using UsersMS.Application.Behaviors;
using MediatR;
using MongoDB.Driver;

[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

// 1. Capas de la Arquitectura
builder.Services.AddApplication(); // MediatR

// 2. Base de Datos (Infraestructura)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration["MongoDb:ConnectionString"]));
builder.Services.AddScoped<IAuditService, MongoAuditService>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
builder.Services.AddHttpContextAccessor(); 
// 3. Inyeccion de Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 4. API Basic
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Registrar HttpClient para Keycloak
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

var app = builder.Build();

// 6. Migraciones Automáticas 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); 
app.UseAuthorization();
app.MapControllers();

app.Run();