using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DoAnMon.Models
{
	public class BaiNop
	{
		[Key]
		public Guid? IdBaiNop { get; set; }
		public string BaiTapId { get; set; }
		public string ClassId { get; set; }
		public DateTime SubmittedAt { get; set; }
		public string UserId { get; set; }
		public string? Urlbainop { get; set; }
		public decimal? Diem {  get; set; }
		public int? daChamDiem { get; set; }
		[ValidateNever]
		public ICollection<ClassRoom> ClassRooms { get; set; }
		[ValidateNever]
		public BaiTap BaiTap { get; set; }
	}
}
