namespace MsAgenda.Core.Entities;

// Ids y códigos fijos del catálogo de estados (se siembran en la base de datos)
public static class EstadosCita
{
    public const int PROGRAMADA = 1;
    public const int CONFIRMADA = 2;
    public const int EN_ESPERA = 3;
    public const int EN_ATENCION = 4;
    public const int ATENDIDA = 5;
    public const int CANCELADA = 6;
    public const int NO_ASISTIO = 7;

    // Estados que ya no ocupan el espacio en la agenda
    public static readonly int[] Liberan = { CANCELADA, NO_ASISTIO };

    // A qué estados se puede pasar desde cada estado
    public static readonly Dictionary<int, int[]> Transiciones = new()
    {
        [PROGRAMADA] = new[] { CONFIRMADA, EN_ESPERA, EN_ATENCION, CANCELADA, NO_ASISTIO },
        [CONFIRMADA] = new[] { EN_ESPERA, EN_ATENCION, CANCELADA, NO_ASISTIO },
        [EN_ESPERA] = new[] { EN_ATENCION, CANCELADA, NO_ASISTIO },
        [EN_ATENCION] = new[] { ATENDIDA },
        [ATENDIDA] = Array.Empty<int>(),
        [CANCELADA] = Array.Empty<int>(),
        [NO_ASISTIO] = Array.Empty<int>()
    };
}