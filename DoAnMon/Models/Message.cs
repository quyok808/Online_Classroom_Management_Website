using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
	public class Message
	{
		public int Id { get; set; }
		public string Noidung { get; set; }
		public string Time { get; set; }
		public string UserId { get; set; }
		public string ClassRoomId { get; set; }
        public string? FilePath { get; set; }


        [ValidateNever]
		public CustomUser? User { get; set; }

		[ValidateNever]
		public ClassRoom? ClassRoom { get; set; }

	}
}
