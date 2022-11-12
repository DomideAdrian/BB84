using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;
using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic.Implementation;

public class RectilinearTransformer : IGateTransformer
{
    public PulseDirection GetPulse(bool value) => value ? PulseDirection.Horizontal : PulseDirection.Vertical;

    public bool GetValue(PulseDirection pulse) => pulse switch
    {
        PulseDirection.Horizontal => true,
        PulseDirection.Vertical => false,
        _ => GetRandomBit()
    };

    private bool GetRandomBit() => RandomObjectGenerator.GetBit();
}
