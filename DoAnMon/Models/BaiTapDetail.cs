using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DoAnMon.Models
{
    public class BaiTapDetail
    {
        [Key]
        public string ClassId { get; set; }
        [Key]
        public string BaiTapId { get; set; }
        public string? Url { get; set; }
        [ValidateNever]
        public BaiTap BaiTap { get; set; }
        [ValidateNever]
        public ClassRoom ClassRoom { get; set; }

    }
}
