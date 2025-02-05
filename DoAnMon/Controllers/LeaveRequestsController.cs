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
using DoAnMon.SendMail;
using System.Drawing;
using System.Numerics;
using Microsoft.IdentityModel.Tokens;
using DoAnMon.Cloudinary;

namespace DoAnMon.Controllers
{
    public class LeaveRequestsController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
		private readonly UserManager<CustomUser> _userManager;
		private readonly Mail _mailService;
		private readonly CloudinaryService _cloudinaryService;

		public LeaveRequestsController(IWebHostEnvironment environment, ApplicationDbContext context, UserManager<CustomUser> userManager, Mail mailService, CloudinaryService cloudinaryService)
        {
            _context = context;
			_userManager = userManager;
			_mailService = mailService;
			_environment = environment;
			_cloudinaryService = cloudinaryService;
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
										lr.Image,
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
                                    lr.Image,
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
		public async Task<IActionResult> Create(LeaveRequest leaveRequest, IFormFile? ImageUpload)
		{
			// Check what data is being received
			if (leaveRequest == null)
			{
				return Json(new { success = false, message = "LeaveRequest is null." });
			}

			if (ModelState.IsValid)
			{
				DateTime now = DateTime.Now;
				leaveRequest.ThoiGianYeuCau = now;
                if (ImageUpload != null && ImageUpload.Length > 0)
                {
					//// Đảm bảo thư mục tồn tại
					//var uploadsFolder = Path.Combine(_environment.WebRootPath, "LeaveRequest");
					//if (!Directory.Exists(uploadsFolder))
					//{
					//	Directory.CreateDirectory(uploadsFolder);
					//}

					//// Tạo tên file duy nhất
					//var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageUpload.FileName;
					//var filePath = Path.Combine(uploadsFolder, uniqueFileName);

					var fileName = await _cloudinaryService.UploadImageAsync(ImageUpload, leaveRequest.ClassRoomId);
					// Lưu file
					leaveRequest.Image = fileName;
					//using (var fileStream = new FileStream(filePath, FileMode.Create))
					//{
					//	await ImageUpload.CopyToAsync(fileStream);
					//}
				}
				else
				{
					// Nếu không có file, lưu thông tin mà không kèm ảnh
					leaveRequest.Image = null; // Hoặc giá trị mặc định nếu cần
				}
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

			//deleteFile("LeaveRequest", leaveRequest.Image);

            // Xóa hình ảnh trên Cloudinary
            if (!string.IsNullOrEmpty(leaveRequest.Image))
            {
                var publicId = ExtractPublicId(leaveRequest.Image); // Tách publicId từ URL nếu cần
                var deletionResult = await _cloudinaryService.DeleteImageAsync(publicId, leaveRequest.ClassRoomId);
                if (!deletionResult)
                {
                    return StatusCode(500, new { success = false, errors = "Không thể xóa hình ảnh từ Cloudinary" });
                }
            }
            _context.leaveRequest.Remove(leaveRequest);
			await _context.SaveChangesAsync();
			return Ok();
        }

        // Helper: Tách publicId từ URL (nếu cần)
        private string ExtractPublicId(string imageUrl)
        {
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');
            return segments[^1].Split('.')[0]; // Tách publicId từ URL
        }

        //DELETE FILE
        public void deleteFile(string folder, string fileName)
		{
			if (fileName.IsNullOrEmpty())
			{
				return;
			}
			var uploadsFolder = Path.Combine(_environment.WebRootPath, folder);

			var filePath = Path.Combine(uploadsFolder, fileName);
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
		}

		[HttpPut]
		public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
		{
			if (request == null || request.leaveRequestId <= 0)
			{
				return BadRequest(new { success = false, errors = "ID không hợp lệ!" });
			}

			var leaveRequest = await _context.leaveRequest.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == request.leaveRequestId);
			if (leaveRequest == null)
			{
				return NotFound(new { success = false, errors = "Không tìm thấy đơn này" });
			}

			leaveRequest.Status = request.status;
			await _context.SaveChangesAsync();
			
			try
			{
				string email = leaveRequest.User.Email;
				string subject = $"Đã duyệt đơn xin nghỉ phép";
				string body = $@"
						<!DOCTYPE html>
						<html lang='en'>
						<head>
							<meta charset='UTF-8'>
							<meta name='viewport' content='width=device-width, initial-scale=1.0'>
							<style>
								body {{
									font-family: 'JetBrains Mono', serif;
									margin: 0;
									padding: 0;
									background-color: #f4f4f4;
								}}
								.email-container {{
									max-width: 600px;
									margin: 20px auto;
									background-color: #ffffff;
									border: 1px solid #dddddd;
									border-radius: 8px;
									overflow: hidden;
								}}
								.header {{
									background-color: #007bff;
									color: #ffffff;
									text-align: center;
									padding: 20px;
									font-family: 'Rowdies', serif;
								}}
								.content {{
									padding: 20px;
									color: #333333;
								}}
								.content p {{
									font-size: 16px;
									line-height: 1.5;
								}}
								.footer {{
									background-color: #f4f4f4;
									color: #888888;
									text-align: center;
									padding: 10px;
									font-size: 12px;
								}}
								.deadline {{
									color: #FF8A8A;
									font-weight: bold;
								}}
							</style>
						</head>
						<body>
							<div class='email-container'>
								<div class='header'>
									<h1>ĐƠN XIN NGHỈ PHÉP ĐÃ ĐƯỢC DUYỆT</h1>
								</div>
								<div class='content'>
									<p>Chào <span class='deadline'>{leaveRequest.User.Name},</span></p>
									<p>Đơn xin nghỉ phép bạn gửi ngày <span class='deadline'>{leaveRequest.ThoiGianYeuCau}</span> yêu cầu nghỉ từ ngày <span class='deadline'>{leaveRequest.StartDate.ToString("dd/MM/yyyy")}</span> đến ngày <span class='deadline'>{leaveRequest.EndDate.ToString("dd/MM/yyyy")}</span> với lý do <span class='deadline'>{leaveRequest.Reasion}</span> đã được duyệt thành công.</p>
									<p>Kết quả là bạn <span class='deadline'>{(leaveRequest.Status == 1 ? "được phép nghỉ" : "không được phép nghỉ")}</span> từ ngày <span>{leaveRequest.StartDate.ToString("dd/MM/yyyy")}</span> đến ngày <span>{leaveRequest.EndDate.ToString("dd/MM/yyyy")}</span></p>
								</div>
								<div class='footer'>
									<p>Email này được gửi tự động từ hệ thống quản lý lớp học trực tuyến OnlyA.</p>
								</div>
							</div>
						</body>
						</html>";
				await _mailService.SendEmailAsync(email, subject, body);
			}
			catch (Exception ex)
			{
				// Ghi log lỗi hoặc thông báo lỗi
				Console.WriteLine($"Error sending email: {ex.Message}");
			}

			return Ok(new { success = true, message = "Cập nhật trạng thái thành công!" });
		}
	}
	public class UpdateStatusRequest
	{
		public int leaveRequestId { get; set; }
		public int status { get; set; }
	}

}
