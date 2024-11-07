using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
	public class Post
	{
		public string Id { get; set; }
		public string? Content { get; set; }
		public string? Title { get; set; }
		public DateTime CreateTime { get; set; }
		public string? ClassRoomId { get; set; }
		[ValidateNever]
		public ICollection<ClassRoom> ClassRooms { get; set; }
		public string? UserId { get; set; }
	}
}
