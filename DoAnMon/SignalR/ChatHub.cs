using Microsoft.AspNetCore.SignalR;

namespace DoAnMon.SignalR
{
	public class ChatHub : Hub
	{
		public async Task SendMessage(string user, string message)
		{
			// Gửi tin nhắn đến tất cả các kết nối
			await Clients.All.SendAsync("ReceiveMessage", user, message);
		}
	}
}
