using Microsoft.EntityFrameworkCore;

namespace EmployeeWorkHourTracker.Models.Entity
{
    public class EmployeeDBContext : DbContext
    {
        public EmployeeDBContext(DbContextOptions<EmployeeDBContext> dbContextOptions) : base(dbContextOptions)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkTrackerLog> WorkTrackerLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<WorkTrackerLog>()
                .HasIndex(w => new { w.EmployeeID, w.Date })
                .IsUnique();
        }
    }
}
