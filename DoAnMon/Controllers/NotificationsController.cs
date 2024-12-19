using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnMon.Data;
using DoAnMon.Models;
using DoAnMon.IdentityCudtomUser;
using DoAnMon.SignalR;
using DoAnMon.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DoAnMon.Controllers
{
    public class NotificationsController : Controller
    {
		private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;
		public NotificationsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

        // GET: FriendRequests
        public async Task<IActionResult> Index()
        {
            CustomUser? currentUser = await GetCurrentUser();
            var currentUserId = currentUser.Id;
            DateTime endDate = DateTime.Now;
            List<FriendRequest> listfriend = await _context.FriendRequests.Where(user => user.TargetId.Equals(currentUserId)).OrderByDescending(p => p.createAt).ToListAsync();
            foreach (var item in listfriend)
            {
                if (item.createAt.HasValue)
				{
                    TimeSpan difference = endDate - item.createAt.Value;
                    if (difference.Days >= 3)
                    {
                        listfriend.Remove(item);
                        _context.Remove(item);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return View(listfriend);
        }

        [HttpPost]
		public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestModel model)
		{
			var targetUserId = model.TargetUserId;
			var classId = model.ClassId;
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var currentUser = await _userManager.GetUserAsync(User);
			var userName = currentUser.Name;
			if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(targetUserId))
			{
				return BadRequest(new { message = "Không tìm thấy người dùng" });
			}

			// Kiểm tra nếu targetUserId tồn tại
			var targetUser = await _userManager.FindByIdAsync(targetUserId);
			if (targetUser == null)
			{
				return BadRequest(new { message = "Người dùng không tồn tại." });
			}
			DateTime now = DateTime.Now;
			// Lưu yêu cầu kết bạn vào database
			var request = new FriendRequest
			{
				RequesterId = currentUserId,
				TargetId = targetUserId,
				IsAccepted = false,
				ClassID = classId,
				createAt = now,
			};

			_context.FriendRequests.Add(request);
			await _context.SaveChangesAsync();

			// Gửi thông báo qua SignalR
			var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
			await hubContext.Clients.User(targetUserId).SendAsync("ReceiveFriendRequestNotification", userName);

			return Ok(new { message = "Gửi lời mời vào nhóm thành công." });
		}

		[HttpPost]
		public async Task<IActionResult> AcceptRequest([FromBody] AcceptRequestModel model)
		{
			var requestId = model.requestId;
			var status = model.status;
			var classId = model.classId;

            var request = await _context.FriendRequests.FindAsync(requestId);
			if (request != null)
			{
				if (status == 1)
				{
                    request.IsAccepted = true;
					SaveGroup(classId, request.RequesterId, request.TargetId);
                }
				else if (status == -1 || status == 0)
				{
					_context.Remove(request);
				}
				await _context.SaveChangesAsync();
				return Ok(new { success = true, message = "Đã đồng ý lập nhóm !!!" });
            }
			return NotFound(new { success = false, message = "Request not found" });
        }

		private void SaveGroup(string classId, string requestUserId, string targerUserId)
		{
			var ClassroomDetails = _context.classroomDetail.Where(p => p.ClassRoomId.Equals(classId)).ToList();
			var userClassDetails = ClassroomDetails.Where(p => p.UserId.Equals(requestUserId) || p.UserId.Equals(targerUserId));
            Guid g = Guid.NewGuid();
            string groupId = g.ToString();

            foreach (var item in userClassDetails)
            {
                if (item.GroupId != null)
				{
					groupId = item.GroupId;
                    break;
                }
            }
            foreach (var item in userClassDetails)
            {
                item.GroupId ??= groupId;
				_context.Update(item);
            }
			_context.SaveChanges();
        }

		private async Task<CustomUser> GetCurrentUser()
		{
			var currentUser = await _userManager.GetUserAsync(User);
			return currentUser;
		}

		// POST: FriendRequests/Delete/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var friendRequest = await _context.FriendRequests.FindAsync(id);
			if (friendRequest != null)
			{
				_context.FriendRequests.Remove(friendRequest);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool FriendRequestExists(int id)
		{
			return _context.FriendRequests.Any(e => e.Id == id);
		}
	}

}
