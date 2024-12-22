namespace DoAnMon.Cloudinary
{
	using CloudinaryDotNet.Actions;
	using CloudinaryDotNet;
	public class CloudinaryService
	{
		private readonly Cloudinary _cloudinary;

		public CloudinaryService(IConfiguration config)
		{
			var cloudName = config["CloudinarySettings:CloudName"];
			var apiKey = config["CloudinarySettings:ApiKey"];
			var apiSecret = config["CloudinarySettings:ApiSecret"];

			_cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
		}

		public async Task<string> UploadImageAsync(IFormFile file, string folderName)
		{
			if (file.Length > 0)
			{
				await using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(file.FileName, stream),
					Folder = folderName
				};
				var uploadResult = await _cloudinary.UploadAsync(uploadParams);
				return uploadResult.SecureUrl.AbsoluteUri; // Trả về URL ảnh
			}
			return null;
		}

		public async Task<string> UploadFileAsync(IFormFile file, string folderName)
		{
			if (file.Length > 0)
			{
				await using var stream = file.OpenReadStream();
				var uploadParams = new RawUploadParams
				{
					File = new FileDescription(file.FileName, stream),
					Folder = folderName,
				};
				var uploadResult = await _cloudinary.UploadAsync(uploadParams);
				return uploadResult.SecureUrl.AbsoluteUri; // Trả về URL ảnh
			}
			return null;
		}
	}
}
