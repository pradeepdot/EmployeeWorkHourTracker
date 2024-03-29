﻿#nullable disable
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
                        PassCode = x.PassCode
                    }).FirstOrDefaultAsync());

            return View(new EmployeePasscodeViewModel());
        }

        [HttpPost("LogTime")]
        public async Task<IActionResult> LogTime(TimeLogViewModel model)
        {
            ViewData["PostToAction"] = "LogTime";

            if (ModelState.IsValid)
            {
                // Check if give passcode match with any employee  https://screencast-o-matic.com/watch/c3n1ewVDGHU
                if (await _context.Employees
                  .AnyAsync(x => x.PassCode == model.PassCode))
                {
                    // Check for work log entry for today already exist
                    var workTrackerLog = await _context.WorkTrackerLogs.Where(x => x.Employee.PassCode == model.PassCode && x.Date.Date == DateTime.Today.Date).FirstOrDefaultAsync();
                    if (workTrackerLog == null)
                    {
                        if (!model.StartLog)
                        {
                            return View(new TimeLogViewModel
                            {
                                EmployeeID = model.EmployeeID,
                                PassCode = model.PassCode,
                                StartLog = true,
                                StopLog = false,
                                DateTime = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            int employeeID = await _context.Employees.Where(x => x.PassCode == model.PassCode).Select(x => x.EmployeeID).FirstOrDefaultAsync();

                            await _context.WorkTrackerLogs.AddAsync(new WorkTrackerLog
                            {
                                Date = model.DateTime,
                                EmployeeID = employeeID,
                                StartDateTime = model.DateTime
                            });
                        }
                    }

                    if (workTrackerLog != null)
                    {
                        if (workTrackerLog.EndDateTime.HasValue && workTrackerLog.EndDateTime.Value != DateTime.MinValue)
                        {
                            ModelState.AddModelError("AlreadyTimeLog", "You have already logged start & end time for today.");
                            ModelState.AddModelError("Time", $"Start Time: {TimeZoneInfo.ConvertTimeFromUtc(workTrackerLog.StartDateTime, TimeZoneInfo.Local).ToString("hh:mm tt")}, End Time: {TimeZoneInfo.ConvertTimeFromUtc(workTrackerLog.EndDateTime.Value, TimeZoneInfo.Local).ToString("hh:mm tt")}");
                            model.DateTime = DateTime.UtcNow;
                            return View(model);
                        }

                        if (!model.StopLog)
                        {
                            return View(new TimeLogViewModel
                            {
                                EmployeeID = model.EmployeeID,
                                PassCode = model.PassCode,
                                StartLog = false,
                                StopLog = true,
                                DateTime = DateTime.UtcNow
                            });
                        }
                        else
                            workTrackerLog.EndDateTime = model.DateTime;
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("Employee", "Employee with given passcode does not exists.");
                    return View("Passcode", model);
                }
            }
            return View("Passcode", model);
        }

        [HttpPost("MyHours")]
        public async Task<IActionResult> MyHours(EmployeePasscodeViewModel model)
        {
            ViewData["PostToAction"] = "MyHours";

            if (ModelState.IsValid)
            {
                return View(await _context.Employees.Include(x => x.WorkTrackerLogs)
                .Where(x => x.PassCode == model.PassCode)
                .FirstOrDefaultAsync());
            }
            return View("Passcode", model);
        }
    }
}
