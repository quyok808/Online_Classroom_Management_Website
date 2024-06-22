using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
	public class DiemDanh
	{
		public int Id { get; set; }
		public string time { get; set; }
		public string? UserId { get; set; }
		public string? ClassRoomId { get; set; }

		[ValidateNever]
		public CustomUser? User { get; set; }

		[ValidateNever]
		public ClassRoom? ClassRoom { get; set; }
	}
}
