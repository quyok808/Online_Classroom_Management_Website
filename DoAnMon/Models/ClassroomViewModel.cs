using DoAnMon.IdentityCudtomUser;

namespace DoAnMon.Models
{
	public class ClassroomViewModel
	{
		public class ClassRoomViewModel
		{
			public ClassRoom ClassRoom { get; set; }
			public CustomUser Owner { get; set; }

			public List<BaiGiang>? Unit { get; set; }

			public List<BaiTap>? Homework { get; set; }
			public List<Post>? Post { get; set; }
			public bool isOwner { get; set; }

			public List<Message>? Message { get; set; }

			public List<ClassDate>? ClassDates { get; set; }

			public Rubric? Rubric { get; set; }

			public bool CustomRubric { get; set; }

		}

	}
}
