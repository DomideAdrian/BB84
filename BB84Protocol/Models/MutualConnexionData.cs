using System.Text.Json.Serialization;

namespace BB84Protocol.Models;

public class MutualConnexionData
{
    public GateType GateType { get; protected set; }
    public DateTime TransferTime { get; protected set; }

    public MutualConnexionData(GateType gateType)
    {
        GateType = gateType;
    }

    [JsonConstructor]
    public MutualConnexionData(GateType gateType, DateTime transferTime) 
        : this(gateType)
    {
        TransferTime = transferTime;
    }

    public MutualConnexionData(PersonalConnexionData obj) 
        : this(obj.GateType, obj.TransferTime) { }
}
