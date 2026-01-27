using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MeterReading.Function;

public class StageMetersVodafone(ILogger<StageMetersVodafone> logger, IStagingMeterService stagingMeterService)
{
    [Function("StageVodafoneMeters")]
    public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("StageVodafoneMeters started at {Time}", DateTime.UtcNow);
        await stagingMeterService.StageVodafoneMetersForReading();
    }
}