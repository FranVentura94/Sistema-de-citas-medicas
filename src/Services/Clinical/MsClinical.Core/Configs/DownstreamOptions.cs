namespace MsClinical.Core.Configs;

public class DownstreamOptions
{
    // URL base del microservicio Agenda (citas)
    public string AgendaBaseUrl { get; set; } = string.Empty;

    // URL base del microservicio Directory (pacientes)
    public string DirectoryBaseUrl { get; set; } = string.Empty;
}