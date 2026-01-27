namespace MeterReading.Service.Interface;

public interface IReadMeterVodafoneService
{
    decimal ReadMeter(string meterNumber, string ConnectionNumber, decimal lastReadingValue);
}