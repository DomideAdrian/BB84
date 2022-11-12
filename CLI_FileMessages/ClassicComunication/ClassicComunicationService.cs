namespace CLI_FileMessages.ClassicComunication;

internal class ClassicComunicationService
{
    private string _fileName;

    public ClassicComunicationService(string fileName)
    {
        _fileName = fileName;
    }

    public string FileName => _fileName;

    public void Write(string message)
    {
        File.WriteAllText(_fileName, message);
    }

    public string Read()
    {
        return File.ReadAllText(_fileName);
    }
}
