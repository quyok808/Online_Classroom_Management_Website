using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DoAnMon.Models
{
    public class ClassRoom
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }
        public string? Description { get; set; }
        public string? RoomOnline { get; set; }
        [ValidateNever]
        public ICollection<ClassroomDetail> ClassroomDetails { get; set; }
        [ValidateNever]
        public CustomUser? User { get; set; }
        [ValidateNever]
		public ICollection<BaiGiang> BaiGiangs { get; set; }
	}

}
