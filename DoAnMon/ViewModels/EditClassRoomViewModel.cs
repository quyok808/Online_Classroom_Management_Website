using System.ComponentModel.DataAnnotations;

namespace DoAnMon.ViewModels
{
    public class EditClassRoomViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Tên lớp học")]
        [Required(ErrorMessage = "Tên lớp học là bắt buộc.")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Ngày học")]
        public string[] DaysOfWeek { get; set; }

        [Display(Name = "Giờ bắt đầu")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Giờ kết thúc")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
		public int? ShowRubric { get; set; }
	}

}
