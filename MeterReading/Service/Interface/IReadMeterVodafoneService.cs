namespace MeterReading.Service.Interface;

public interface IReadMeterVodafoneService
{
    double ReadMeter(string meterNumber, string ConnectionNumber, double lastReadingValue);
}