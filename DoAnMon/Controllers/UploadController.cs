using DoAnMon.Data;
using DoAnMon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace DoAnMon.Controllers
{
	[Route("upload")]
	[ApiController]
	public class UploadController : ControllerBase
	{
		private readonly string _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","Uploads", "Temp");
		private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
		private readonly string _messageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "messagefile");
		private readonly ApplicationDbContext _context;

		public UploadController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpPost]
		public async Task<IActionResult> UploadChunk()
		{
			var folderType = Request.Form["folderType"]; // Lấy loại thư mục từ yêu cầu
			var fileName = Request.Query["fileName"];
			var ClassId = Request.Query["ClassId"];
			var resumableChunkNumber = int.Parse(Request.Form["resumableChunkNumber"]);
			var resumableTotalChunks = int.Parse(Request.Form["resumableTotalChunks"]);
			var resumableIdentifier = Request.Form["resumableIdentifier"];
			var resumableFilename = Request.Form["resumableFilename"];

			// Xác định thư mục lưu file dựa trên folderType
			string folderPath;
			if (folderType == "BAITAP")
			{
				folderPath = Path.Combine(_uploadPath, "BAITAP");
			}
			else if (folderType == "BAIGIANG")
            {
                var file = Request.Form.Files[0];
                var fileExtension = Path.GetExtension(file.FileName);
                folderPath = Path.Combine(_uploadPath, "BAIGIANG");
                resumableFilename = $"{ClassId}_{fileName}{fileExtension}";
				if (resumableChunkNumber == resumableTotalChunks)
					await SavedatabaseBAIGIANG(ClassId, resumableFilename, fileName); // Sử dụng await
            }
            else if (folderType == "BAINOP")
			{
				folderPath = Path.Combine(_uploadPath, "BAINOP");
			}
			else
			{
				return BadRequest("Invalid folder type");
			}

			// Tạo đường dẫn lưu file theo thư mục và tên riêng
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}


			var chunkFilePath = Path.Combine(_tempPath, resumableIdentifier, resumableChunkNumber.ToString());
			if (!Directory.Exists(Path.Combine(_tempPath, resumableIdentifier)))
			{
				Directory.CreateDirectory(Path.Combine(_tempPath, resumableIdentifier));
			}

			using (var stream = new FileStream(chunkFilePath, FileMode.Create))
			{
				await Request.Form.Files[0].CopyToAsync(stream);
			}

			// Nếu đây là chunk cuối cùng, ghép file và đặt tên dựa trên thư mục
			if (resumableChunkNumber == resumableTotalChunks)
			{
				// Tạo tên file tùy chỉnh theo thư mục
				//string customFileName = $"{folderType}_{resumableFilename}";
				var finalFilePath = Path.Combine(folderPath, resumableFilename);

				using (var finalFileStream = new FileStream(finalFilePath, FileMode.Create))
				{
					for (var i = 1; i <= resumableTotalChunks; i++)
					{
						var chunkPath = Path.Combine(_tempPath, resumableIdentifier, i.ToString());
						using (var chunkFileStream = new FileStream(chunkPath, FileMode.Open))
						{
							await chunkFileStream.CopyToAsync(finalFileStream);
						}

						// Xóa chunk sau khi ghép
						System.IO.File.Delete(chunkPath);
					}
				}

				// Xóa thư mục tạm
				Directory.Delete(Path.Combine(_tempPath, resumableIdentifier));

				return Ok("Upload complete");
			}

			return Ok();
		}

        private async Task SavedatabaseBAIGIANG(StringValues classId, StringValues resumableFilename, StringValues name)
        {
            BaiGiang newLecture = new BaiGiang();
            newLecture.UrlBaiGiang = resumableFilename;
            newLecture.Name = name;
            newLecture.ClassId = classId;
            _context.BaiGiang.Add(newLecture);
            await _context.SaveChangesAsync();
        }


        // Kiểm tra chunk đã tồn tại chưa
        [HttpGet]
		public IActionResult DoesChunkExist()
		{
			var resumableChunkNumber = Request.Query["resumableChunkNumber"];
			var resumableIdentifier = Request.Query["resumableIdentifier"];

			var chunkPath = Path.Combine(_tempPath, resumableIdentifier, resumableChunkNumber);

			if (System.IO.File.Exists(chunkPath))
			{
				return StatusCode(200); // Chunk đã tồn tại
			}
			else
			{
				return StatusCode(204); // Chunk chưa tồn tại
			}
		}
		[HttpPost]
		[Route("upload-file-message")]
		public async Task<IActionResult> UploadFileForMessage(IFormFile file, int messageId)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			// Đường dẫn lưu file riêng cho tin nhắn
			if (!Directory.Exists(_messageUploadPath))
			{
				Directory.CreateDirectory(_messageUploadPath);
			}

			// Tạo tên file duy nhất để tránh trùng lặp
			var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
			var filePath = Path.Combine(_messageUploadPath, uniqueFileName);

			// Lưu file vào thư mục FileMessage
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			// Lưu thông tin file vào bảng FileAttachment
			var fileAttachment = new FileAttachment
			{
				FileName = file.FileName,
				FilePath = "/messagefile/" + uniqueFileName,  // Đường dẫn tương đối cho web
				FileType = file.ContentType,
				MessageId = messageId  // Liên kết với tin nhắn
			};

			_context.FileAttachments.Add(fileAttachment);
			await _context.SaveChangesAsync();

			return Ok(new { filePath = fileAttachment.FilePath });
		}


	}

}
