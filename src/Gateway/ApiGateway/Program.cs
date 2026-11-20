var builder = WebApplication.CreateBuilder(args);

// ---------- YARP: lee las rutas y los microservicios desde appsettings.json ----------
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Cliente para revisar el estado de cada microservicio
builder.Services.AddHttpClient("estado", c => c.Timeout = TimeSpan.FromSeconds(3));

var app = builder.Build();

// ---------- GET /  -> información del gateway ----------
app.MapGet("/", () => Results.Ok(new
{
    sistema = "Sistema de Citas Médicas - API Gateway",
    version = "1.0",
    microservicios = new[] { "Identity", "Directory", "Agenda", "Clinical" },
    estado = "/estado"
}));

// ---------- GET /estado  -> revisa si cada microservicio responde ----------
app.MapGet("/estado", async (IConfiguration config, IHttpClientFactory factory) =>
{
    var http = factory.CreateClient("estado");
    var clusters = config.GetSection("ReverseProxy:Clusters").GetChildren();

    var tareas = clusters.Select(async cluster =>
    {
        var url = cluster.GetSection("Destinations").GetChildren().First()["Address"]!;
        try
        {
            // Cualquier respuesta HTTP (aunque sea 401 o 404) significa que el servicio está arriba
            await http.GetAsync(url);
            return new { servicio = cluster.Key, url, activo = true };
        }
        catch
        {
            return new { servicio = cluster.Key, url, activo = false };
        }
    });

    var resultados = await Task.WhenAll(tareas);
    return Results.Ok(new { todosActivos = resultados.All(r => r.activo), servicios = resultados });
});

// ---------- Todo lo que empiece con /api/... se reenvía al microservicio que corresponda ----------
app.MapReverseProxy();

app.Run();