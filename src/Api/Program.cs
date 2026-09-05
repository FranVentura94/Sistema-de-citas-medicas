using Core.Common.Interfaces;
using Core.Features.Pacientes.Interfaces;
using Core.Features.Pacientes.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configurar conexión a SQL Server con ClinicaDbContext
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar repositorio de pacientes (patrón específico, CQRS)
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();

// Registrar repositorio genérico de persistencia (CRUD con objetos genéricos)
// Resuelve IRepository<TEntity, TKey> para cualquier entidad (Medico, Citas, Atenciones, etc.)
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

// Registrar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetPacientesQueryHandler).Assembly));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Habilitar Swagger para visualizar y probar endpoints
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();