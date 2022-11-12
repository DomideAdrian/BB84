using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Models;

namespace BB84Protocol.BusinessLogic;

public class BB84Manager
{
    private IConnexion _connexion;

    /// <summary>
    /// This array will contain parsed data starting with LSB.
    /// </summary>
    private List<PersonalConnexionData> _personalConnexionData;

    public BB84Manager(IConnexion connexion)
    {
        _connexion = connexion;

        _personalConnexionData = new();
    }

    public IEnumerable<MutualConnexionData> GetConnexionData()
    {
        return _personalConnexionData;
    }

    public void InitProcessWrite(byte[] data)
    {
        foreach (var dataByte in data)
        {
            _personalConnexionData.AddRange(ParseByte(dataByte));
        }

        _connexion.Write(PolarizeData());
    }

    public void InitProcessRead()
    {
        var pulses = _connexion.Read();
        foreach (var pulse in pulses)
        {
            _personalConnexionData.Add(new PersonalConnexionData(pulse));
        }
    }

    public byte[] GenerateKey(List<MutualConnexionData> corespondentData)
    {
        if (!corespondentData.Any())
            throw new InvalidOperationException("Corespondent data can not be empty.");
        if (corespondentData.Count != _personalConnexionData.Count)
            throw new InvalidOperationException("Mutual connexion data error: different lengths.");

        List<bool> keyBaseValueBits = new();
        for(int i = 0; i < _personalConnexionData.Count; i++)
        {
            if (_personalConnexionData[i].GateType == corespondentData[i].GateType)
            {
                keyBaseValueBits.Add(_personalConnexionData[i].BitValue);
            }
        }

        if (keyBaseValueBits.Count == 0)
            throw new InvalidOperationException("There were no bits to generate the key...");

        return ParseBits(keyBaseValueBits);
    }

    private byte[] ParseBits(List<bool> bits)
    {
        List<byte> result = new();
        int bitIndex = 0;
        if(bits.Count % 8 != 0)
        {
            bits.AddRange(new bool[8 - (bits.Count % 8)]);
        }
        while (bitIndex < bits.Count)
        {
            int byteValue = 0;
            for(int i = 0; i < 8; i++)
            {
                if (bits[bitIndex + i])
                    byteValue |= (1 << i);
            }
            bitIndex += 8;
            result.Add((byte)byteValue);
        }
        return result.ToArray();
    }

    private List<PersonalConnexionData> ParseByte(byte info)
    {
        List<PersonalConnexionData> result = new();
        for(int i = 0; i < 8; i++)
        {
            var bitValue = (info & (1 << i)) != 0;
            result.Add(new PersonalConnexionData(bitValue));
        }
        return result;
    }

    private List<PulseDirection> PolarizeData()
    {
        List<PulseDirection> result = new();

        foreach (var bit in _personalConnexionData)
        {
            result.Add(bit.GeneratePulseDirection());
        }

        return result;
    }
}
