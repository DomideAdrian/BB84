using BB84Protocol.Models;
using System.Text;
using System.Text.Json;

namespace BB84Protocol.Helpers;

public static class ExtensionMethods
{
    public static string GetContent(this IEnumerable<PulseDirection> pulses)
    {
        StringBuilder result = new();
        foreach (var pulse in pulses)
        {
            result.Append((char)pulse);
        }
        return result.ToString();
    }

    public static IEnumerable<PulseDirection> GetPulses(this string pulses)
    {
        List<PulseDirection> result = new();
        foreach (var pulse in pulses)
        {
            result.Add((PulseDirection)pulse);
        }
        return result;
    }

    public static string GetJsonString(this IEnumerable<MutualConnexionData> data)
    {
        return JsonSerializer.Serialize(data);
    }
}
