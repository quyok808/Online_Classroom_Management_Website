using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using DoAnMon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DoAnMon.SignalR
{
	public class ChatHub : Hub
	{
		private readonly UserManager<CustomUser> _userManager;
		private readonly ApplicationDbContext _context;

		public ChatHub(UserManager<CustomUser> userManager, ApplicationDbContext context)
		{
			_userManager = userManager;
			_context = context;
		}

		public async Task SendMessage(string user, string message, string time, string ClassID)
		{

			await Clients.All.SendAsync("ReceiveMessage", user, message);

			await SaveMessageToDatabase(ClassID, message, time);

		}

		private async Task SaveMessageToDatabase(string ClassID, string message, string time)
		{
			var user = Context.User;
			var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var newMessage = new Message();
			newMessage.UserId = currentUserId;
			newMessage.Noidung = message;
			newMessage.Time = time;
			newMessage.ClassRoomId = ClassID;

			_context.Messages.Add(newMessage);
			await _context.SaveChangesAsync();
		}
	}
}
