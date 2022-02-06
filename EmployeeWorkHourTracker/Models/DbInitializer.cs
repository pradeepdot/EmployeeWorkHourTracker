using EmployeeWorkHourTracker.Models.Entity;

namespace EmployeeWorkHourTracker.Models
{
    public static class DbInitializer
    {
        public static void Initialize(EmployeeDBContext context)
        {
            context.Database.EnsureCreated();

            // Look for any employee.
            if (context.Employees.Any())
            {
                return;   // DB has been seeded
            }

            var employees = new Employee[]
            {
                new Employee{ FirstName="John", LastName="Doe", Passode="4515"},
                new Employee{ FirstName="Patrick", LastName="Jackson", Passode="8954"},
            };
            foreach (Employee employee in employees)
            {
                context.Employees.Add(employee);
            }
            context.SaveChanges();
        }

        public static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<EmployeeDBContext>();
                    Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

    }
}
