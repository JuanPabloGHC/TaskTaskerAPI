using System.ComponentModel.DataAnnotations;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class AdminCredentialsDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string username { get; set; } = string.Empty;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string password { get; set; } = string.Empty;
    }
}
