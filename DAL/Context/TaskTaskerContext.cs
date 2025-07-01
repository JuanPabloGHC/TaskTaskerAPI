using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskTaskerAPI.DAL.Entities;

namespace TaskTaskerAPI.DAL.Context
{
    public class TaskTaskerContext: DbContext
    {
        #region DATA MEMBERS

        protected readonly IConfiguration Configuration;

        #endregion

        #region DB SETS DEFINITION

        public DbSet<Person> Persons { get; set; }

        public DbSet<Home> Homes { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<Entities.Task> Tasks { get; set; }

        public DbSet<Achievement> Achievements { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<Attainment> Attainments { get; set; }

        #endregion

        #region CONSTRUCTOR

        public TaskTaskerContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region EVENT HANDLING

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConnectionString("Database"));
        }

        #endregion

    }
}
