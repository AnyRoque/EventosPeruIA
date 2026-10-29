using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.API.Workers
{
    // US-15: proceso automático que cada 15 minutos envía el recordatorio
    // a quienes tienen entradas para un evento que empieza en las próximas 24 horas.
    public class RecordatorioWorker : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecordatorioWorker> _logger;

        public RecordatorioWorker(IServiceScopeFactory scopeFactory, ILogger<RecordatorioWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Se crea un scope porque el DbContext no puede usarse directamente en un servicio en segundo plano
                    using var scope = _scopeFactory.CreateScope();
                    var notificaciones = scope.ServiceProvider.GetRequiredService<INotificacionService>();
                    var enviados = await notificaciones.EnviarRecordatorios();
                    if (enviados > 0) _logger.LogInformation("Recordatorios enviados: {Cantidad}", enviados);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar recordatorios");
                }

                await Task.Delay(Intervalo, stoppingToken);
            }
        }
    }
}
