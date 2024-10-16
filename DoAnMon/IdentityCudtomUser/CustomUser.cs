using DoAnMon.Models;
using Microsoft.AspNetCore.Identity;

namespace DoAnMon.IdentityCudtomUser
{
    public class CustomUser : IdentityUser
    {
        public string? Mssv { get; set; }
        public string? Name { get; set; }
        public string? NgaySinh { get; set; }
        public string? UrlAvt { get; set; }

        public ICollection<ClassroomDetail> ClassroomDetails { get; set; }
        public ICollection<LeaveRequest> leaveRequests { get; set; }
    }
}
