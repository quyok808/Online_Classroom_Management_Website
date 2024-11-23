using DoAnMon.Data;
using DoAnMon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace DoAnMon.Controllers
{
	[Route("upload")]
	[ApiController]
	public class UploadController : ControllerBase
	{
		private readonly string _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","Uploads", "Temp");
		private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
		private readonly ApplicationDbContext _context;

		public UploadController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpPost("UploadChunk")]
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
        [HttpGet("DoesChunkExist")]
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

		[HttpPost("UploadFileResumable")]
		public IActionResult UploadFileResumable()
		{
			var resumableIdentifier = Request.Form["resumableIdentifier"];
			var resumableFilename = Request.Form["resumableFilename"];
			var resumableChunkNumber = int.Parse(Request.Form["resumableChunkNumber"]);

			var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/BAINOP/temp", resumableIdentifier);
			Directory.CreateDirectory(uploadPath);

			var chunkFilePath = Path.Combine(uploadPath, $"{resumableChunkNumber}");
			using (var fileStream = new FileStream(chunkFilePath, FileMode.Create))
			{
				Request.Body.CopyTo(fileStream);
			}

			var totalChunks = int.Parse(Request.Form["resumableTotalChunks"]);
			if (Directory.GetFiles(uploadPath).Length == totalChunks)
			{
				var finalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads/BAINOP", resumableFilename);

				using (var finalFile = new FileStream(finalPath, FileMode.Create))
				{
					for (var i = 1; i <= totalChunks; i++)
					{
						var chunkPath = Path.Combine(uploadPath, $"{i}");
						var chunkData = System.IO.File.ReadAllBytes(chunkPath);
						finalFile.Write(chunkData, 0, chunkData.Length);
						System.IO.File.Delete(chunkPath);
					}
				}

				Directory.Delete(uploadPath);

				return new JsonResult(new { success = true, filePath = $"/Uploads/BAINOP/{resumableFilename}" });
			}

			return Ok();
		}

	}

}
