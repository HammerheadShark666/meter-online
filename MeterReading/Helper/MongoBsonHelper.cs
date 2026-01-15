using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace MeterReading.Helper;
internal class MongoBsonHelper
{
    public static BsonRegularExpression ExactMatch(string value)
    {
        return new BsonRegularExpression($"^{Regex.Escape(value)}$", "i");
    }
}