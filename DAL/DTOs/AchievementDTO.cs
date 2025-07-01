namespace TaskTaskerAPI.DAL.DTOs
{
    public class AchievementDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string image { get; set; }

        #endregion

        #region CONSTRUCTORS

        public AchievementDTO() { }

        public AchievementDTO(int id, string name, string description, string image)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.image = image;
        }

        #endregion

    }
}
