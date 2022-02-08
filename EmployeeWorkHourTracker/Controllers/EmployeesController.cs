#nullable disable
using EmployeeWorkHourTracker.Models.Entity;
using EmployeeWorkHourTracker.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeWorkHourTracker.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeDBContext _context;

        public EmployeesController(EmployeeDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Dashboard");
        }


        public async Task<IActionResult> Dashboard()
        {
            return View(await _context.Employees.Include(x => x.WorkTrackerLogs.Where(z => z.Date.Date == DateTime.Today.Date))
                .ToListAsync());
        }

        [HttpGet("Passcode")]
        public async Task<IActionResult> Passcode(int? ID, string postAction = "LogTime")
        {
            ViewData["PostToAction"] = postAction;
            if (ID.HasValue)
                return View(await _context.Employees
                    .Where(x => x.EmployeeID == ID)
                    .Select(x => new EmployeePasscodeViewModel
                    {
                        EmployeeID = x.EmployeeID,
                        PassCode = x.PassCode,
                        PostToActionName = postAction
                    }).FirstOrDefaultAsync());

            return View(new EmployeePasscodeViewModel { PostToActionName = postAction });
        }

        [HttpPost("Passcode")]
        public async Task<IActionResult> Passcode(EmployeePasscodeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!await _context.Employees
                  .AnyAsync(x => x.PassCode == model.PassCode))
            {
                ModelState.AddModelError("Employee", "Employee with given passcode does not exists.");
                return View("Passcode", model);
            }

            return RedirectToActionPermanent(model.PostToActionName, new
            {
                PassCode = model.PassCode
            });
        }

        [HttpGet("LogTime")]
        public async Task<IActionResult> LogTime(string PassCode)
        {
            var workTrackerLog = await _context.WorkTrackerLogs.Where(x => x.Employee.PassCode == PassCode && x.Date.Date == DateTime.Today.Date).FirstOrDefaultAsync();
            if (workTrackerLog == null)
            {
                return View(new TimeLogViewModel
                {
                    PassCode = PassCode,
                    StartLog = true,
                    StopLog = false,
                    DateTime = DateTime.UtcNow
                });
            }

            if (workTrackerLog != null)
            {
                if (workTrackerLog.EndDateTime.HasValue && workTrackerLog.EndDateTime.Value != DateTime.MinValue)
                {
                    ModelState.AddModelError("AlreadyTimeLog", "You have already logged start & end time for today.");
                    ModelState.AddModelError("Time", $"Start Time: {TimeZoneInfo.ConvertTimeFromUtc(workTrackerLog.StartDateTime, TimeZoneInfo.Local).ToString("hh:mm tt")}, End Time: {TimeZoneInfo.ConvertTimeFromUtc(workTrackerLog.EndDateTime.Value, TimeZoneInfo.Local).ToString("hh:mm tt")}");
                    
                    return View(new TimeLogViewModel
                    {
                        PassCode = PassCode,
                        StartLog = false,
                        StopLog = false,
                        DateTime = DateTime.UtcNow
                    });
                }

                return View(new TimeLogViewModel
                {
                    PassCode = PassCode,
                    StartLog = false,
                    StopLog = true,
                    DateTime = DateTime.UtcNow
                });
            }

            return View(new EmployeePasscodeViewModel());
        }

        [HttpPost("LogTime")]
        public async Task<IActionResult> LogTime(TimeLogViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for work log entry for today already exist
                var workTrackerLog = await _context.WorkTrackerLogs
                    .Where(x => x.Employee.PassCode == model.PassCode
                    && x.Date.Date == DateTime.Today.Date)
                    .FirstOrDefaultAsync();

                if (workTrackerLog == null)
                {
                    int employeeID = await _context.Employees.Where(x => x.PassCode == model.PassCode).Select(x => x.EmployeeID).FirstOrDefaultAsync();
                    await _context.WorkTrackerLogs.AddAsync(new WorkTrackerLog
                    {
                        Date = model.DateTime,
                        EmployeeID = employeeID,
                        StartDateTime = model.DateTime
                    });
                }

                if (workTrackerLog != null)
                {
                    workTrackerLog.EndDateTime = model.DateTime;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Dashboard");
            }

            return View("LogTime", model);
        }

        [HttpGet("MyHours")]
        public async Task<IActionResult> MyHours(string PassCode)
        {
            return View(await _context.Employees.Include(x => x.WorkTrackerLogs)
            .Where(x => x.PassCode == PassCode)
            .FirstOrDefaultAsync());
        }
    }
}
