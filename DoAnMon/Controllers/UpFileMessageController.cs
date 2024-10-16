using Microsoft.AspNetCore.Mvc;

namespace DoAnMon.Controllers
{
	[Route("api/upfilemessage")]
	[ApiController]
	public class UpFileMessageController : ControllerBase
	{
		private readonly IWebHostEnvironment _environment;

		public UpFileMessageController(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("Chưa có file được tải lên.");

			var uploads = Path.Combine(_environment.WebRootPath, "messagefile");
			if (!Directory.Exists(uploads))
			{
				Directory.CreateDirectory(uploads);  
			}

			var fileName = Path.GetFileName(file.FileName);
			var filePath = Path.Combine(uploads, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			var fileUrl = $"/messagefile/{fileName}";
			return Ok(fileUrl);
		}
	}
}
