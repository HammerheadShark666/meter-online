using System.Text.Json;

namespace MeterReading.Helper;
internal class JsonHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}