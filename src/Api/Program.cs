using BuildingBlocks.Persistence;
using Core;
using Core.Features.Pacientes.Interfaces;
using Identity.Data;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repositories;
using Serilog;
using Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// NUEVO: Serilog, para ver los logs de la librería REST
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// 1. Configurar las bases de datos (Identidad y Clínica)
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("IdentityConnection")));

// 2. Registrar MediatR (Para los Handler de CQRS)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Core.Features.Pacientes.Queries.GetPacientesQuery).Assembly));

// NUEVO: opciones de Core + librería REST y servicios de Infrastructure
builder.Services.AddCore(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Registrar los Repositorios
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ClinicaDbContext>());
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>(); // Registra el repositorio específico de Pacientes

// 4. Agregar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

// 5. Middlewares de ejecución
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();