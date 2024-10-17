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
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoAnMon.Controllers
{
    public class LeaveRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;

		public LeaveRequestsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
			_userManager = userManager;
        }

		public IActionResult GetAllLeaveRequest()
		{
			// Lấy tất cả leave requests theo classId
			var leaveRequests = _context.leaveRequest.ToList();

			return Json(leaveRequests);
		}

		public async Task<IActionResult> GetAllLeaveRequestByClassID(string ClassID)
		{
			try
			{
				
				// Asynchronously retrieve all leave requests by ClassID and UserID
				var leaveRequests = await _context.leaveRequest
					.Where(p => p.ClassRoomId.Equals(ClassID))
					.ToListAsync();

				if (leaveRequests == null || !leaveRequests.Any())
				{
					return NotFound("No leave requests found for the specified class ID.");
				}

				return Json(leaveRequests);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}


		public IActionResult GetAllLeaveRequestByUserId(string UserID, string ClassID, bool isOwner)
		{
			// Lấy tất cả leave requests theo classId
			var leaveRequests = _context.leaveRequest
								.Include(lr => lr.User) // Assuming 'User' is the navigation property for the user
								.Where(p => p.ClassRoomId.Equals(ClassID) && p.UserID.Equals(UserID))
								.Select(lr => new
									{
										lr.Id,
										lr.Reasion,
										lr.StartDate,
										lr.EndDate,
										lr.Status,
										lr.UserID,
										Name = lr.User.Name // Assuming the 'User' entity has the 'Name' property
									})

								.ToList();
			if (isOwner)
			{
				leaveRequests = _context.leaveRequest
								.Include(lr => lr.User) // Assuming 'User' is the navigation property for the user
								.Where(p => p.ClassRoomId.Equals(ClassID))
								.Select(lr => new
								{
									lr.Id,
									lr.Reasion,
									lr.StartDate,
									lr.EndDate,
									lr.Status,
									lr.UserID,
									Name = lr.User.Name ,// Assuming the 'User' entity has the 'Name' property
								})

								.ToList();
			}
			return Json(leaveRequests);
		}


		// POST: LeaveRequests/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(LeaveRequest leaveRequest)
		{
			// Check what data is being received
			if (leaveRequest == null)
			{
				return Json(new { success = false, message = "LeaveRequest is null." });
			}

			if (ModelState.IsValid)
			{
				_context.Add(leaveRequest);
				await _context.SaveChangesAsync();
				return Json(new { success = true, message = "Leave request created successfully." });
			}

			var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
			return BadRequest(new { success = false, errors });
		}

        // GET: LeaveRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
				return BadRequest(new { success = false, errors = "Không thành công !!!" });
            }

			var leaveRequest = await _context.leaveRequest.FindAsync(id);
			if (leaveRequest == null)
            {
				return BadRequest(new { success = false, errors = "Lỗi không tìm thấy đơn này" });
			}
			_context.leaveRequest.Remove(leaveRequest);
			await _context.SaveChangesAsync();
			return Ok();
        }

		[HttpPut]
		public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
		{
			if (request == null || request.leaveRequestId <= 0)
			{
				return BadRequest(new { success = false, errors = "ID không hợp lệ!" });
			}

			var leaveRequest = await _context.leaveRequest.FindAsync(request.leaveRequestId);
			if (leaveRequest == null)
			{
				return NotFound(new { success = false, errors = "Không tìm thấy đơn này" });
			}

			leaveRequest.Status = request.status;
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
		}
	}
	public class UpdateStatusRequest
	{
		public int leaveRequestId { get; set; }
		public int status { get; set; }
	}

}
