using MeterReading.Helper;
using MeterReading.Service.Interface;

namespace MeterReading.Service;
public sealed class ReadMeterVodafoneService : IReadMeterVodafoneService
{
    private static readonly Random _random = new();

    public decimal ReadMeter(string meterNumber, string ConnectionNumber, decimal lastReadingValue)
    {
        //In real world this code would connect to the meter and get the reading.
        //Here we will simulate this by generating a random number between MinValue and MaxValue.

        decimal min = ReadMeterValue.MinValue;
        decimal max = ReadMeterValue.MaxValue;

        decimal meterIncrement =
            min + (decimal)_random.NextDouble() * (max - min);

        return Math.Round(lastReadingValue + meterIncrement, 2, MidpointRounding.AwayFromZero);
    }
}