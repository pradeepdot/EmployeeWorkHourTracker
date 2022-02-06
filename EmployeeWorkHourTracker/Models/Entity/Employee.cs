using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeWorkHourTracker.Models.Entity
{
    public class Employee
    {
        public Employee()
        {
            WorkTrackerLogs = new HashSet<WorkTrackerLog>();
        }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EmployeeID { get; set; }
        public string Passode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<WorkTrackerLog> WorkTrackerLogs { get; set; }
    }
}
