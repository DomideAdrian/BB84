using Microsoft.AspNetCore.SignalR.Client;

namespace CLI_SignalR_Messages.ClassicComunication;
internal class ClassicComunicationClient
{
	private HubConnection? connection = null;
	private List<string>? receivedMessage = null;

	public ClassicComunicationClient()
	{
		connection = new HubConnectionBuilder()
			.WithUrl("https://localhost:5001/chathub")
			.Build();

		receivedMessage = new List<string>();
	}

	public void startClient()
	{
		connection.StartAsync().Wait();
		connection.On("ReceiveMessage", (string userName, string message) =>
		{
			receivedMessage.Add(userName);
			receivedMessage.Add(message);

		});
	}

	public void SendMessage(string username, string message)
	{
		connection.InvokeCoreAsync("SendMessage", args: new[] { username, message });
	}

	public string ReadMessage(string username)
	{
		
		for (int index = 0; index < receivedMessage.Count() - 1; index += 2)
		{
			if (receivedMessage[index].Contains(username))
			{
				receivedMessage.RemoveAt(index);
				receivedMessage.RemoveAt(index);
				index -= 2;
			}
		} 

		string? temp = null;

		for (int index = 0; index < receivedMessage.Count() - 1; index +=2)
		{
			if (!receivedMessage[index].Contains(username))
			{
				temp = receivedMessage[index+1];
				receivedMessage.RemoveAt(index);
				receivedMessage.RemoveAt(index);

			}
			return temp;
		}

		return null;
	}
}
