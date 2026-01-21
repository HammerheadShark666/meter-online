using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MeterReading.Function;

public class StageVodafoneMeters(ILoggerFactory loggerFactory, IStagingMeterService stagingMeterService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StageVodafoneMeters>();

    [Function("StageVodafoneMeters")]
    public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("StageVodafoneMeters started at {Time}", DateTime.UtcNow);

        try
        {
            await stagingMeterService.StageVodafoneMetersForReading();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StageVodafoneMeters failed");
            throw;
        }
    }
}