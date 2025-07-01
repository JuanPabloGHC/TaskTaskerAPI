namespace TaskTaskerAPI.DAL.DTOs
{
    public class RoleDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; }

        public string image { get; set; }

        #endregion

        #region CONSTRUCTORS

        public RoleDTO() { }

        public RoleDTO(int id, string name, string image)
        {
            this.id = id;
            this.name = name;
            this.image = image;
        }

        #endregion

    }
}
