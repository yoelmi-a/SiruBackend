using SIRU.Infrastructure.Persistence;
using SIRU.Infrastructure.Identity;
using SIRU.Infrastructure.Shared;
using SIRU.Core.Application;
using SIRU.Presentation.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddIdentityLayer();
builder.Services.AddSharedLayer();
builder.Services.AddApiLayer();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIRU API V1");
        c.RoutePrefix = string.Empty; // Muestra Swagger en la raíz
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
