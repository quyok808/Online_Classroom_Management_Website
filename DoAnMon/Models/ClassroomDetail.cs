using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnMon.Models
{
	public class ClassroomDetail
	{
		[Key, Column(Order = 0)]
		public string UserId { get; set; }

		[Key, Column(Order = 1)]
		public string ClassRoomId { get; set; }

		public string RoleId { get; set; } // Không cần navigation property đến CustomRole
		[ValidateNever]
		public ClassRoom ClassRoom { get; set; }
		[ValidateNever]
		public CustomUser User { get; set; }
		public string? GroupId { get; set; }

	}
}
