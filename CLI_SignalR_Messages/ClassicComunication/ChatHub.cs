using Microsoft.AspNetCore.SignalR;

namespace CLI_SignalR_Messages.ClassicComunication;
internal class ChatHub : Hub
{
    public async Task SendMessage(string userName, string message)
	{
		await Clients.All.SendAsync("ReceiveMessage", userName, message);
	}
}

