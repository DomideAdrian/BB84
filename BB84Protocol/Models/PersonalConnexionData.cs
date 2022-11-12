using BB84Protocol.BusinessLogic.Factories;
using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;

namespace BB84Protocol.Models;

public class PersonalConnexionData : MutualConnexionData
{
    public bool BitValue { get; private set; }

    private IGateTransformer _gateTransformer;
    
    public PersonalConnexionData(bool bitValue)
        : base(RandomObjectGenerator.GetGate())
    {
        BitValue = bitValue;
        _gateTransformer = GateTransformerFactory.GetGate(GateType);
    }

    public PersonalConnexionData(PulseDirection pulse)
        : base(RandomObjectGenerator.GetGate(), DateTime.Now)
    {
        _gateTransformer = GateTransformerFactory.GetGate(GateType);
        BitValue = _gateTransformer.GetValue(pulse);
    }

    public PulseDirection GeneratePulseDirection()
    {
        TransferTime = new DateTime();
        return _gateTransformer.GetPulse(BitValue);
    }
}
