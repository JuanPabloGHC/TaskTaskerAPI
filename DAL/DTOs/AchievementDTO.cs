using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.DTOs
{
    public class AchievementDTO
    {
        #region PROPERTIES

        public int id { get; set; }

        public string name { get; set; }

        public int days { get; set; }

        public string image { get; set; }

        public TaskDTO task { get; set; }

        #endregion

        #region CONSTRUCTORS

        public AchievementDTO() { }

        public AchievementDTO(int id, string name, int days, string image, TaskDTO task)
        {
            this.id = id;
            this.name = name;
            this.days = days;
            this.image = image;
            this.task = task;
        }

        public AchievementDTO(Achievement achievement)
        {
            this.id = achievement.id;
            this.name = achievement.name;
            this.days = achievement.days;
            this.image = achievement.image;
            this.task = new TaskDTO(achievement.task);
        }

        #endregion

    }
}
