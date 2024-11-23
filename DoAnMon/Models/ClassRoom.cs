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
        public string? backgroundUrl { get; set; }
        public int STT { get; set; }
        public int? RubricId { get; set; } // Nếu dùng rubric default thì giá trị là -1

		// Các trường mới cho ngày học
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string DaysOfWeek { get; set; } // Đây sẽ là chuỗi chứa các ngày trong tuần (vd: "Monday,Wednesday")
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }

        public Rubric? Rubric { get; set; }
		[ValidateNever]
        public ICollection<ClassroomDetail> ClassroomDetails { get; set; }
        [ValidateNever]
        public CustomUser? User { get; set; }
        [ValidateNever]
		public ICollection<BaiGiang> BaiGiangs { get; set; }
        [ValidateNever]
        public ICollection<LeaveRequest> leaveRequests {  get; set; }

		public int GetDisplayNumber()
		{
			return STT + 1;
		}
	}

}
