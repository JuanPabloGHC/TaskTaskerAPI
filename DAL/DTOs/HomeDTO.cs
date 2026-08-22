using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class HomeDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; } = string.Empty;

        public string image { get; set; } = string.Empty;

        #endregion

        #region CONSTRUCTORS

        public HomeDTO() { }

        public HomeDTO(int id, string name, string image)
        {
            this.id = id;
            this.name = name;
            this.image = image;
        }

        public HomeDTO(Home home)
        {
            this.id = home.id;
            this.name = home.name;
            this.image = home.image;
        }

        #endregion

    }
}
