using CommunityMS.Infrastructure.Persistence;
using CommunityMS.Domain.Interfaces;
using CommunityMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CommunityMS.Application; // Placeholder for MediatR assembly
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CommunityDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IForumRepository, ForumRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CommunityMS.Application.AssemblyReference).Assembly));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommunityDbContext>();
    db.Database.Migrate(); 
}

app.Run();
