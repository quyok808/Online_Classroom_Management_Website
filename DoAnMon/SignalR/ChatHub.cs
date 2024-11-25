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

		public async Task SendMessage(string user, string message, string time, string classID, string? fileUrl = null)
		{
			if (!string.IsNullOrEmpty(message) || !string.IsNullOrEmpty(fileUrl))
			{
				var content = string.IsNullOrEmpty(message) ? "Đã gửi một file." : message;
				await SaveMessageToDatabase(classID, content, time, fileUrl);
				await Clients.All.SendAsync("ReceiveMessage", user, content, fileUrl);
			}
		}

		private async Task SaveMessageToDatabase(string classID, string message, string time, string? filePath)
		{
			var user = Context.User;
			var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var newMessage = new Message
			{
				UserId = currentUserId,
				Noidung = message,
				Time = time,
				ClassRoomId = classID,
				FilePath = filePath
			};

			_context.Messages.Add(newMessage);
			await _context.SaveChangesAsync();
		}	
	}
}
