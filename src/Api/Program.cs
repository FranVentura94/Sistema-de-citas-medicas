using Core.Features.Pacientes.Interfaces;
using Domain.Interfaces;
using Identity.Data;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

// 3. Registrar los Repositorios
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>(); // Registra el repositorio específico de Pacientes

// 4. Agregar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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