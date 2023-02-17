using BB84Protocol.BusinessLogic;
using BB84Protocol.BusinessLogic.Interfaces;
using BB84Protocol.Helpers;
using BB84Protocol.Models;
using CLI_FileMessages.ClassicComunication;
using CLI_FileMessages.Infrastructure;
using System.Text;
using System.Text.Json;
namespace CLI_FileMessages;

internal class CLI_Manager
{
    private BB84Manager? _protocolManager;
    private IConnexion? _connexion;

    private ClassicComunicationService? _classicComm;

    private int? _selectedOption;
    private bool _shouldRun = true;

    private byte[] _key;

    public CLI_Manager()
    {
        _protocolManager = null;
        _connexion = null;
        _classicComm = null;
    }

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
                PrintError("Something happened while processing your query. ");
            }
        }
    }

    public void PrintKey()
    {
        foreach (var keyByte in _key)
        {
            Console.Write(keyByte);
            Console.Write("-");
        }
    }

    private void PrintError(string message)
    {
        Console.WriteLine(message + "Please try again.");
        Thread.Sleep(2000);
    }

    private bool ParseOption()
    {
        return _selectedOption switch
        {
            1 => InitConnexion(),
            2 => GenerateKey(),
            3 => GenerateKeyUser1(),
            _ => false,
        };
    }

    private bool GenerateKeyUser1()
    {
        var mutualData = _classicComm.Read();
        _key = _protocolManager.GenerateKey(JsonSerializer.Deserialize<List<MutualConnexionData>>(mutualData));
        PrintKey();
        return true;
    }

    private bool GenerateKey()
    {
        if (_protocolManager == null || _connexion == null || _classicComm == null)
        {
            Console.WriteLine("No connexion created yet.");
            return false;
        }

        if (File.Exists(_connexion.ConnectionString))
        {
            _protocolManager.InitProcessRead();
            var mutualData = _classicComm.Read();
            _classicComm.Write(_protocolManager.GetConnexionData().GetJsonString());
            _key = _protocolManager.GenerateKey(JsonSerializer.Deserialize<List<MutualConnexionData>>(mutualData));

            PrintKey();
        }
        else
        {
            Console.Write("Enter base key message: ");
            var data = Console.ReadLine();
            _protocolManager.InitProcessWrite(Encoding.UTF8.GetBytes(data));
            _classicComm.Write(_protocolManager.GetConnexionData().GetJsonString());
        }
        return true;
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

    private void PrintMenuOptions()
    {
        Console.Write(
            "Hello to BB84 FileTesting Suite!\n" +
            "Options:\n" +
            "1 - Initialize connexion\n" +
            "2 - Generate key U2 or init process U1\n" +
            "3 - Generate key U1"
            );
    }

    private bool InitConnexion()
    {
        Console.Write("Enter quantum file name:");
        var quantumFileName = Console.ReadLine();
        Console.Write("Enter classic file name:");
        var classicFileName = Console.ReadLine();
        if (quantumFileName == null || classicFileName == null)
            return false;

        _classicComm = new ClassicComunicationService(classicFileName);
        _connexion = new FileConnexion(quantumFileName);
        _protocolManager = new BB84Manager(_connexion);

        return true;
    }
}
