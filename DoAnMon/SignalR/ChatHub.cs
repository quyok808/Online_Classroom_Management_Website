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

		public async Task SendMessage(string user, string message, string time, string ClassID, List<FileAttachment> attachments)
		{
			await Clients.All.SendAsync("ReceiveMessage", user, message, attachments);
			await SaveMessageToDatabase(ClassID, message, time, attachments);  // Không cần gọi song song
		}


		private async Task SaveMessageToDatabase(string ClassID, string message, string time, List<FileAttachment> attachments)
		{
			var user = Context.User;
			var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			// Tạo đối tượng Message mới và khởi tạo danh sách FileAttachments
			var newMessage = new Message
			{
				UserId = currentUserId,
				Noidung = message,
				Time = time,
				ClassRoomId = ClassID,
				FileAttachments = new List<FileAttachment>()  // Khởi tạo danh sách
			};

			// Lưu tin nhắn vào cơ sở dữ liệu trước để lấy ID
			_context.Messages.Add(newMessage);
			await _context.SaveChangesAsync();  // Lưu để ID của newMessage được tạo

			// Thêm từng file đính kèm vào danh sách FileAttachments
			foreach (var attachment in attachments)
			{
				attachment.MessageId = newMessage.Id;  // Gắn ID message.
				newMessage.FileAttachments.Add(attachment);
			}

			// Cập nhật cơ sở dữ liệu.
			await _context.SaveChangesAsync();
		}

	}
}
