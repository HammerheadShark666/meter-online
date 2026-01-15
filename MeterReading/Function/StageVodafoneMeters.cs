using MeterReading.Service.Interface;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MeterReading.Function;

public class StageVodafoneMeters(ILoggerFactory loggerFactory, IStagingMeterService stagingMeterService) //IConfiguration config, 
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StageVodafoneMeters>();

    [Function("StageVodafoneMeters")]
    public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {Time}", DateTime.Now);


        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {Time}", myTimer.ScheduleStatus.Next);
        }

        await stagingMeterService.StageVodafoneMetersForReading();
    }
}
