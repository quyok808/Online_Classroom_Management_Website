using DoAnMon.IdentityCudtomUser;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DoAnMon.Models
{
    public class ClassroomDetail
    {
        [Key]
        public string UserId { get; set; }
        [Key]
        public string ClassId { get; set; }
        public string? RoleId { get; set; }
        [ValidateNever]
        public ClassRoom ClassRoom { get; set; }
        [ValidateNever]
        public CustomUser User { get; set; }
        [ValidateNever]
        public CustomRole Role { get; set; }
    }
}
