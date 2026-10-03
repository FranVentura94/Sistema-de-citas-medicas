using Notificaciones.Core.Models;

namespace Notificaciones.Core.Services;

public interface IPacientesService
{
    Task<PacienteDto> GetPorDocumentoAsync(string numeroDocumento);
}