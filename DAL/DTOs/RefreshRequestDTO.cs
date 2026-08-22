using System.ComponentModel.DataAnnotations;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class RefreshRequestDTO
    {
        [Required]
        public string refreshToken { get; set; } = string.Empty;
    }
}
