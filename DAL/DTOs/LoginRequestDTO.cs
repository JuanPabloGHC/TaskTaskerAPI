using System.ComponentModel.DataAnnotations;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class LoginRequestDTO
    {
        [Required]
        public string name { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;
    }
}
