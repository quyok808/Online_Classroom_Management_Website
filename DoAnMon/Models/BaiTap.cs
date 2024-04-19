using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class BaiTap
    {
		public string Id { get; set; }
		public string? Content { get; set; }
		public string? Title { get; set; }
		public string? attractUrl { get; set; }
		public string FileFormat { get; set; }
		public string? ClassRoomId { get; set; }
		[ValidateNever]
		public ICollection<ClassRoom> ClassRooms { get; set; }

		[ValidateNever]
		public ICollection<BaiTapDetail> BaiTapDetails { get; set; }
	}
}
