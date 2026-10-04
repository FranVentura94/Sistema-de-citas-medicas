using Notificaciones.Core.Exceptions;
using Notificaciones.Core.Models;
using Notificaciones.Core.Services;

namespace Notificaciones.Tests;

public class NotificacionServiceTests
{
    private sealed class FakePacientesService : IPacientesService
    {
        private readonly PacienteDto _paciente;

        public FakePacientesService(PacienteDto paciente)
        {
            _paciente = paciente;
        }

        public Task<PacienteDto> GetPorDocumentoAsync(string numeroDocumento) => Task.FromResult(_paciente);
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<(string Destinatario, string Asunto, string Cuerpo)> Enviados { get; } = [];

        public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            Enviados.Add((destinatario, asunto, cuerpo));
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task EnviarBienvenidaAsync_ConEmail_EnviaElCorreoAlPaciente()
    {
        var sender = new FakeEmailSender();
        var paciente = new PacienteDto
        {
            Nombres = "Ana",
            Apellidos = "Martinez",
            NumeroDocumento = "123",
            Email = "ana@correo.com"
        };
        var service = new NotificacionService(new FakePacientesService(paciente), sender);

        var resultado = await service.EnviarBienvenidaAsync("123");

        Assert.Equal("ana@correo.com", resultado.Destinatario);
        var enviado = Assert.Single(sender.Enviados);
        Assert.Equal("ana@correo.com", enviado.Destinatario);
        Assert.Contains("Ana Martinez", enviado.Cuerpo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EnviarBienvenidaAsync_SinEmail_LanzaPacienteSinEmailYNoEnviaNada(string? email)
    {
        var sender = new FakeEmailSender();
        var paciente = new PacienteDto { Nombres = "Carlos", Apellidos = "Rivas", Email = email };
        var service = new NotificacionService(new FakePacientesService(paciente), sender);

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.EnviarBienvenidaAsync("123"));

        Assert.Equal(Errores.PACIENTE_SIN_EMAIL, exception.Codigo);
        Assert.Empty(sender.Enviados);
    }
}