using BB84Protocol.BusinessLogic;
using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;
using BB84Protocol.Models;
using CLI_SignalR_Messages.ClassicComunication;
using CLI_SignalR_Messages.Infrastructure;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CLI_SignalR_Messages;

internal class CLI_Manager
{
    private BB84Manager? _protocolManager;
    private IConnexion? _connexion;

    private ClassicComunicationService? _classicCommService;
    private ClassicComunicationClient? _classicCommClient;

    private int? _selectedOption;
    private bool _shouldRun = true;
    private string _myHeader;
    private Byte[]? _randomSequence = null;
    private double _bitErrorRate = 0.20;
    private bool[] _currentKeyBits;
    private byte[] _key;
    private bool _autoErr = false;

    public void Run()
    {
        while (_shouldRun)
        {
            PrintMenuOptions();
            if (!SelectOption())
            {
                PrintError("No option selected. ");
                continue;
            }

            if (!ParseOption())
            {
                _shouldRun = false;
            }
        }

    }

    private void PrintMenuOptions()
    {
        Console.Write(
            "\n\nHello to BB84 SignaRTesting Suite!\n" +
            "Options:\n" +
            "1 - Initialize hub ( Alice )\n" +
            "2 - Initialize protocol ( Bob )\n" +
            "3 - Start pulse transmision ( Alice )\n" +
            "4 - Start pulse mesuring (Bob)\n" +
            "5 - Generate key\n"
            );
    }

    private bool SelectOption()
    {
        Console.Write("\nEnter option:");
        var option = Console.ReadLine();
        if (string.IsNullOrEmpty(option))
            return false;
        if (!int.TryParse(option, out int selectedOption))
        {
            return false;
        }
        _selectedOption = selectedOption;
        return true;
    }

    private bool ParseOption()
    {
        return _selectedOption switch
        {
            1 => InitHub(),
            2 => InitProtocol(),
            3 => StartPulseTransmission(),
            4 => StartPulseMeasuring(),
            5 => GenerateKey(),
            _ => false,
        };
    }

    private bool InitHub()
	{
        _myHeader = "Alice";

        Thread thread = new Thread(new ThreadStart(Lisen));
        thread.Start();

        Console.WriteLine("Thread started!");

        InitProtocol();

        return true;
	}

    private bool InitProtocol()
	{
        if (_myHeader is not "Alice"){
            _myHeader = "Bob";
        }

        _classicCommClient = new();
        _classicCommClient.startClient();

        _connexion = new FileConnexion("qfile.txt");
        _protocolManager = new BB84Manager(_connexion);

        return true;
	}
    private void PrintError(string message)
    {
        Console.WriteLine(message + "Please try again.");
        Thread.Sleep(2000);
    }

    private void Lisen()
	{
        _classicCommService = new ClassicComunicationService();
        _classicCommService.startService(args: new[] { "" });
    }

    private bool StartPulseTransmission()
	{
        if (_protocolManager == null || _connexion == null)
        {
            Console.WriteLine("Error. Connection problems.");
            return false;
        }

        ReadUserData();

        _protocolManager.InitProcessWrite(_randomSequence);
        _classicCommClient.SendMessage(_myHeader, _protocolManager.GetConnexionData().GetJsonString());
        if(_autoErr) 
            InsertErrors();
        
        return true;
	}

    private bool StartPulseMeasuring()
	{
        if (_protocolManager == null || _connexion == null)
        {
            Console.WriteLine("Error. Connection problems.");
            return false;
        }

        Console.WriteLine("Measuring ...");
        _protocolManager.InitProcessRead();
        return true;
	}

    private bool GenerateKey()
	{
        var mutualData = _classicCommClient.ReadMessage(_myHeader);
        if (mutualData == null)
        {
            Console.WriteLine("Don't received polarization basis");
            return true;
        }
        Console.WriteLine("Generating key...");
        _classicCommClient.SendMessage(_myHeader + ":init", _protocolManager.GetConnexionData().GetJsonString());
        _key = _protocolManager.GenerateKey(JsonSerializer.Deserialize<List<MutualConnexionData>>(mutualData));

        Console.WriteLine("Initial key:");
        PrintKey();
        Console.WriteLine("\n");

        ShuffleKey();
        if (!EstimateErrors())
            return true;

        _key = ParseBits(_currentKeyBits.ToList());

        ErrorCorrection();

        _key = ParseBits(_currentKeyBits.ToList());
        Console.WriteLine("Final key:");
        PrintKey();
        
        return false;
    }

    public void PrintKey()
    {
        foreach (var keyByte in _key)
        {
            Console.Write(keyByte);
            Console.Write(" ");
        }
    }

    private void ReadUserData()
    {
        Console.Write("Enter base key size (in bytes): ");
        int size = Convert.ToInt32(Console.ReadLine());
        Random random = new Random();
        _randomSequence = new Byte[size];
        random.NextBytes(_randomSequence);

        Console.WriteLine("\n");

        Console.WriteLine("Enter yes or no if you want to insert errors automatically: ");
        string option = Console.ReadLine().ToString();
        _autoErr = option.Contains("yes") ? true : false;

    }

    private void ShuffleKey()
    {
        if(_myHeader == "Bob")
        {
            int[] numbers = Enumerable.Range(0, (_key.Count() * 8)).ToArray();

            Random rnd = new Random();
            int[] permutation = numbers.OrderBy(x => rnd.Next()).ToArray();
            ShuffleBits(permutation);
            _classicCommClient.SendMessage(_myHeader, "shuffle:" + String.Join(",", permutation ));
        }
        else
        {
            int[] permutation = _classicCommClient.ReadMessage(_myHeader).Remove(0,8).Split(",").Select(n => Convert.ToInt32(n)).ToArray();
            ShuffleBits(permutation);
        }

    }

    private void ShuffleBits(int[] permutation)
    {
        if (_currentKeyBits is null) {
            _currentKeyBits = new bool[permutation.Length];
            _currentKeyBits = _key.SelectMany(GetBitsStartingFromLSB).ToArray();
        }
        bool[] newKey = new bool[permutation.Length];
        for (int index = 0; index < permutation.Length; index++)
        {
            newKey[permutation[index]] = _currentKeyBits[index];
        }
        _currentKeyBits = newKey;
    }

    private IEnumerable<bool> GetBitsStartingFromLSB(byte b)
    {
        for (int i = 0; i < 8; i++)
        {
            yield return (b % 2 == 0) ? false : true;
            b = (byte)(b >> 1);
        }
    }

    private byte[] ParseBits(List<bool> bits)
    {
        List<byte> result = new();
        int bitIndex = 0;
        if (bits.Count % 8 != 0)
        {
            bits.AddRange(new bool[8 - (bits.Count % 8)]);
        }
        while (bitIndex < bits.Count)
        {
            int byteValue = 0;
            for (int i = 0; i < 8; i++)
            {
                if (bits[bitIndex + i])
                    byteValue |= (1 << i);
            }
            bitIndex += 8;
            result.Add((byte)byteValue);
        }
        return result.ToArray();
    }

    private bool EstimateErrors()
    {

        if (_myHeader == "Bob")
        {
            _classicCommClient.SendMessage(_myHeader, String.Join(",", GetTestedBits()));
            string response = null;
            while ( response == null )
            {
                response = _classicCommClient.ReadMessage(_myHeader);
                if (response != null)
                {
                    if (response.Contains("GateType"))
                    {
                        response = null;
                    }
                }
            }
            if (response.Contains("abort"))
            {
                Console.WriteLine("\n\nToo many errors. Aborted.");
                return false;
            }
            return true;
        }
        else
        {
            int[] bits = _classicCommClient.ReadMessage(_myHeader).Split(",").Select(n => Convert.ToInt32(n)).ToArray();
            double qber = EstimateProportion(bits);
            if (qber > _bitErrorRate)
            {
                Console.WriteLine("\n\nToo many errors. Aborted.");
                _classicCommClient.SendMessage(_myHeader, "abort");
                return false;
            }
            _classicCommClient.SendMessage(_myHeader, "ok");
            return true;
        }
    }

    private int[] GetTestedBits()
    {
        List<bool> currentBits = _currentKeyBits.ToList();
        int[] testedBits = new int[(_currentKeyBits.Count() / 4) + 1];
        for (int index = 0; index < currentBits.Count(); index += 4)
        {
            testedBits[index / 4] = Convert.ToInt32(currentBits[index]);
        }
        for (int index = 0; index < currentBits.Count(); index += 3)
        {
            currentBits.RemoveAt(index);
        }
        _currentKeyBits = currentBits.ToArray();
        return testedBits;
    }

    private double EstimateProportion(int[] testedBits)
    {
        int[] myValues = GetTestedBits();
        int problems = 0;
        
        for (int index = 0; index < myValues.Length; index++)
        {
            if (myValues[index] != testedBits[index])
                problems++;
        }
        return Convert.ToDouble(problems) / Convert.ToDouble(testedBits.Length);
    }

    private void ErrorCorrection()
    {
        SyncKey();

        if(_myHeader == "Bob")
        {
            int iteration = 1;
            int[]? permutation = null;
            while (iteration < 5)
            {
                if (iteration != 1)
                {
                    int[] numbers = Enumerable.Range(0, (_key.Count() * 8)).ToArray();
                    Random rnd = new Random();
                    permutation = numbers.OrderBy(x => rnd.Next()).ToArray();
                    ShuffleBits(permutation);
                    _classicCommClient.SendMessage(_myHeader, "shuffle:" + String.Join(",", permutation));
                }

                if (permutation == null)
                {
                    permutation = Enumerable.Range(0, (_key.Count() * 8)).ToArray();
                }

                for (int index = 0; index < permutation.Count(); index += (iteration * 3))
                {
                    int elements = 0;
                    if (permutation.Count() - index < iteration * 3)
                    {
                        elements = permutation.Count() - index;
                    }
                    else
                        elements = iteration * 3;

                    int[] currentBlk = new int[elements];
                    Array.Copy(permutation, index, currentBlk, 0, elements);
                    _classicCommClient.SendMessage(_myHeader, "parity:" + String.Join(",", currentBlk));
                    bool myParity = VerifyParity(currentBlk);
                    string message = null;
                    while (message is null)
                    {
                        message = _classicCommClient.ReadMessage(_myHeader);
                        if (message is not null)
                        {
                            if (message.Contains("parity"))
                            {
                                message = message.Remove(0, 7);
                            }
                            else
                                message = null;
                        }
                    }

                    bool recParity;
                    if (message.Contains("1"))
                        recParity = true;
                    else
                        recParity = false;
                    
                    if (recParity != myParity)
                    {
                        StartBinaryCorrection(currentBlk);
                    }
                    //Console.WriteLine("No error in first " + index.ToString() + " bits \n");
                }
                //Console.WriteLine("Suntem la " + iteration.ToString() );
                iteration++;

            }

            _classicCommClient.SendMessage(_myHeader, "fin" );

        }
        else
        {
            bool _stillCorrecting = true;
            string operation = _classicCommClient.ReadMessage(_myHeader);
            while ( _stillCorrecting)
            {
                if (operation != null)
                {
                    if (operation.Contains("parity"))
                    {
                        bool parity = VerifyParity(operation.Remove(0, 7).Split(",").Select(n => Convert.ToInt32(n)).ToArray());
                        //Console.WriteLine("for " + String.Join(",", operation.Remove(0, 7).Split(",").Select(n => Convert.ToInt32(n)).ToArray()) + " send " + parity.ToString());
                        _classicCommClient.SendMessage(_myHeader, "parity:" + (parity ? "1" : "0"));
                    }
                    else if (operation.Contains("shuffle"))
                    {
                        int[] permutation = operation.Remove(0, 8).Split(",").Select(n => Convert.ToInt32(n)).ToArray();
                        ShuffleBits(permutation);
                    }
                    else if (operation.Contains("fin"))
                    {
                        _stillCorrecting = false;
                    }
                }
                operation = _classicCommClient.ReadMessage(_myHeader);
            }
        }
    }

    private void StartBinaryCorrection(int[] permutation)
    {
        if (permutation.Count() > 1)
        {
            _classicCommClient.SendMessage(_myHeader, "parity:" + String.Join(",", permutation.Take(permutation.Count() / 2).ToArray()));
            bool myParity = VerifyParity(permutation.Take(permutation.Count() / 2).ToArray());
            string message = null;
            while (message is null)
            {
                message = _classicCommClient.ReadMessage(_myHeader);
            }
            bool recParity = message.Contains("1") ? true : false;
            if (recParity != myParity)
            {
                if((permutation.Count() / 2) == 1)
                    _currentKeyBits[permutation[0]] = _currentKeyBits[permutation[0]] ? false : true;
                else
                    StartBinaryCorrection(permutation.Take(permutation.Count() / 2).ToArray());
            }
            else
            {
                if (permutation.Count() - (permutation.Count() / 2) == 1)
                    _currentKeyBits[permutation[1]] = _currentKeyBits[permutation[1]] ? false : true;
                else
                    StartBinaryCorrection(permutation.Skip(permutation.Count() / 2).ToArray());
            }

        }
    }

    private void SyncKey()
    {
        bool[] sync = new bool[_key.Count() * 8]; 
        for (int i = 0; i < _currentKeyBits.Length ; i++)
        {
            sync[i] = _currentKeyBits[i];
        }
        _currentKeyBits = sync;
    }

    private bool VerifyParity( int[] permutation )
    {
        int parity = 0;
        for (int index = 0; index < permutation.Length; index++)
        {
            if (_currentKeyBits[permutation[index]])
                parity++;
        }
        if (parity % 2 == 0)
            return false;
        else
            return true;
    }

    private void InsertErrors()
    {
        string pulsesEncoding = File.ReadAllText("qfile.txt");
        int rate = _randomSequence.Length * 8 / 10;
        string error = new string('-', rate);
        pulsesEncoding = error + pulsesEncoding.Substring(rate);

        pulsesEncoding = pulsesEncoding.Substring(0, _randomSequence.Length * 8 / 2) + pulsesEncoding.Substring(_randomSequence.Length * 8 / 2).Replace("|", "-");

        File.WriteAllText("qfile.txt", pulsesEncoding);

    }
}