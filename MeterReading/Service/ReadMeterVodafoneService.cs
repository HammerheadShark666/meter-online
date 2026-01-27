using MeterReading.Helper;
using MeterReading.Service.Interface;

namespace MeterReading.Service;
public sealed class ReadMeterVodafoneService : IReadMeterVodafoneService
{
    public double ReadMeter(string meterNumber, string ConnectionNumber, double lastReadingValue)
    {
        Random random = new();
        double meterReadingValue = ReadMeterValue.MinValue + (random.NextDouble() * (ReadMeterValue.MaxValue - ReadMeterValue.MinValue));

        return lastReadingValue + meterReadingValue;
    }
}