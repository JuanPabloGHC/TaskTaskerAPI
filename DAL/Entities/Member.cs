using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskTaskerAPI.DAL.DTOs;

namespace TaskTaskerAPI.DAL.Entities
{
    [Table("Member")]
    public class Member
    {
        #region PROPERTIES

        [Key]
        public int id { get; set; }

        #endregion

        #region NAVIGATION

        [Required]
        public int person_id { get; set; }
        [ForeignKey("person_id")]
        public Person person { get; set; } = null!;

        [Required]
        public int home_id { get; set; }
        [ForeignKey("home_id")]
        public Home home { get; set; } = null!;

        [Required]
        public int role_id { get; set; }
        [ForeignKey("role_id")]
        public Role role { get; set; } = null!;

        #endregion

        #region CONSTRUCTORS

        public Member() { }

        public Member(MemberDTO memberDTO)
        {
            this.id = memberDTO.id;
            this.person_id = memberDTO.person.id;
            this.home_id = memberDTO.home.id;
            this.role_id = memberDTO.role.id;
        }

        #endregion

    }
}
