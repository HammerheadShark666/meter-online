using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace MeterReading.Service.Interface;
public interface IUpdateMeterReadingService
{
    Task UpdateMeterReading(ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions);
}