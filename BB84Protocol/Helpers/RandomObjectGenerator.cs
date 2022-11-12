using BB84Protocol.Models;

namespace BB84Protocol.Helpers;

public static class RandomObjectGenerator
{
    private static Random _random = new Random();

    public static GateType GetGate() => (_random.Next() % 2) switch
    {
        0 => GateType.Rectilinear,
        1 => GateType.Diagonal,
        _ => throw new InvalidOperationException("Random value error...")
    };

    public static bool GetBit() => (_random.Next() % 2) != 0;
}
