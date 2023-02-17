using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;
using BB84Protocol.Models;

namespace CLI_SignalR_Messages.Infrastructure;

public class FileConnexion : IConnexion
{
    private string _fileName;

    public FileConnexion(string fileName)
    {
        _fileName = fileName;
    }

    public string ConnectionString => _fileName;

    public IEnumerable<PulseDirection> Read()
    {
        var pulsesEncoding = File.ReadAllText(_fileName);
        return pulsesEncoding.GetPulses();
    }

    public void Write(IEnumerable<PulseDirection> pulses)
    {
        File.WriteAllText(_fileName, pulses.GetContent());
    }
}
