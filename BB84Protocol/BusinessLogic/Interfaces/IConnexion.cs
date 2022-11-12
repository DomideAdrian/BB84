using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic.Interfaces;

public interface IConnexion
{
    string ConnectionString { get; }

    void Write(IEnumerable<PulseDirection> pulses);

    IEnumerable<PulseDirection> Read();
}
