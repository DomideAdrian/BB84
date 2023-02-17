using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;
using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic.Implementation;

public class DiagonalTransformer : IGateTransformer
{
    public PulseDirection GetPulse(bool value) => value ? PulseDirection.DiagonalUp : PulseDirection.DiagonalDown;

    public bool GetValue(PulseDirection pulse) => pulse switch
    {
        PulseDirection.DiagonalUp => true,
        PulseDirection.DiagonalDown => false,
        _ => GetRandomBit()
    };

    private bool GetRandomBit() => RandomObjectGenerator.GetBit();
}
