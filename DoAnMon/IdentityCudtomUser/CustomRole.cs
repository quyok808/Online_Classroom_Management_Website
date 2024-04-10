using DoAnMon.Models;
using Microsoft.AspNetCore.Identity;
namespace DoAnMon.IdentityCudtomUser
{
    public class CustomRole : IdentityRole
    {

        public ICollection<ClassroomDetail> ClassroomDetails { get; set; }
    }
}
