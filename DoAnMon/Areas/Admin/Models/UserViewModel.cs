namespace DoAnMon.Areas.Admin.Models
{
	public class UserViewModel
	{
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public List<string> Roles { get; set; }
		public List<string> AllRoles { get; set; }
		public string CurrentRole { get; set; }
	}
}
