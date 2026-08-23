using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskTaskerAPI.DAL.Context;
using TaskTaskerAPI.DAL.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskTaskerAPI.DAL
{
    /// <summary>
    /// Idempotent seed of the base catalog. Runs at startup and only inserts what
    /// is missing (matched by name), so it is safe to run on every boot.
    /// Images are generated as self-contained base64 SVG placeholders — meant to
    /// be replaced later from the web admin panel.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(TaskTaskerContext context)
        {
            await SeedRoles(context);
            await SeedStatuses(context);
            await SeedTasks(context);
            await context.SaveChangesAsync();

            await SeedAchievements(context);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(TaskTaskerContext context)
        {
            (string name, string color)[] defaults =
            {
                ("Owner", "#7C3AED"),
                ("Admin", "#2563EB"),
                ("Member", "#0F766E"),
            };

            List<string> existing = await context.Roles.Select(r => r.name).ToListAsync();

            foreach ((string name, string color) in defaults)
                if (!existing.Contains(name))
                    await context.Roles.AddAsync(new Role { name = name, image = Placeholder(name, color) });
        }

        private static async Task SeedStatuses(TaskTaskerContext context)
        {
            (string name, string color)[] defaults =
            {
                ("Pending", "#9CA3AF"),
                ("En revisión", "#F59E0B"),
                ("Done", "#10B981"),
            };

            List<string> existing = await context.Statuses.Select(s => s.name).ToListAsync();

            foreach ((string name, string color) in defaults)
                if (!existing.Contains(name))
                    await context.Statuses.AddAsync(new Status { name = name, color = color });
        }

        private static async Task SeedTasks(TaskTaskerContext context)
        {
            (string name, string color)[] defaults =
            {
                ("Barrer", "#F59E0B"),
                ("Trapear", "#3B82F6"),
                ("Lavar trastes", "#10B981"),
                ("Sacar basura", "#6B7280"),
                ("Tender cama", "#8B5CF6"),
                ("Lavar ropa", "#06B6D4"),
            };

            List<string> existing = await context.Tasks.Select(t => t.name).ToListAsync();

            foreach ((string name, string color) in defaults)
                if (!existing.Contains(name))
                    await context.Tasks.AddAsync(new Entities.Task { name = name, image = Placeholder(name, color) });
        }

        private static async Task SeedAchievements(TaskTaskerContext context)
        {
            (string name, int days, string taskName, string color)[] defaults =
            {
                ("Limpiador", 5, "Barrer", "#F59E0B"),
                ("Trapeador experto", 5, "Trapear", "#3B82F6"),
                ("Rey de la cocina", 10, "Lavar trastes", "#10B981"),
            };

            Dictionary<string, int> taskIds = await context.Tasks.ToDictionaryAsync(t => t.name, t => t.id);
            List<string> existing = await context.Achievements.Select(a => a.name).ToListAsync();

            foreach ((string name, int days, string taskName, string color) in defaults)
            {
                if (existing.Contains(name)) continue;
                if (!taskIds.TryGetValue(taskName, out int taskId)) continue;

                await context.Achievements.AddAsync(new Achievement
                {
                    name = name,
                    days = days,
                    image = Placeholder(name, color),
                    task_id = taskId
                });
            }
        }

        /// <summary>Builds a base64 data-URI SVG (colored card with the label) as a placeholder image.</summary>
        private static string Placeholder(string label, string background)
        {
            string safe = SecurityElement.Escape(label) ?? label;

            string svg =
                "<svg xmlns='http://www.w3.org/2000/svg' width='120' height='120'>"
                + $"<rect width='120' height='120' rx='16' fill='{background}'/>"
                + "<text x='60' y='64' font-family='Arial, Helvetica, sans-serif' font-size='15' "
                + $"fill='#ffffff' text-anchor='middle'>{safe}</text></svg>";

            return "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        }
    }
}
