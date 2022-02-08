using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkHourTracker.Models.ViewModel
{
    public class EmployeeTrackLogViewModel
    {
    }

    public class EmployeePasscodeViewModel
    {
        [Required(ErrorMessage = "Employee Passcode is required")]
        [MinLength(4, ErrorMessage = "Employee Passcode length must be 4 or more character long")]
        public string PassCode { get; set; }
        public int EmployeeID { get; set; }
        public string PostToActionName { get; set; }
    }

    public class TimeLogViewModel
    {
        [Required(ErrorMessage = "Employee Passcode is required")]
        [MinLength(4, ErrorMessage = "Employee Passcode length must be 4 or more character long")]
        public string PassCode { get; set; }
        public bool StartLog { get; set; }
        public bool StopLog { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class MyHoursReqViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
