using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class AdminDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string username { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public AdminDTO() { }

        public AdminDTO(AdminUser admin)
        {
            this.id = admin.id;
            this.username = admin.username;
        }

        #endregion

    }
}
