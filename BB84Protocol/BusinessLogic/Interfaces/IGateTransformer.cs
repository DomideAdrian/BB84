using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic.Interfaces;

public interface IGateTransformer
{
    PulseDirection GetPulse(bool value);

    bool GetValue(PulseDirection pulse);
}
