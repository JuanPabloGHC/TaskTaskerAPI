using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("RefreshToken")]
    public class RefreshToken
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        [Required]
        public int person_id { get; set; }

        [Required]
        public string token { get; set; } = string.Empty;

        [Required]
        public DateTime expires_at { get; set; }

        [Required]
        public bool is_revoked { get; set; }

        [Required]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        #endregion

        #region CONSTRUCTORS

        public RefreshToken() { }

        #endregion

    }
}
