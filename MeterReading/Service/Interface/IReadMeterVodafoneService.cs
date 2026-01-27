using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace MeterReading.Service.Interface;

public interface IReadMeterVodafoneService
{
    Task ReadMeter(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions);
}