using Employee_Records.Data;
using Employee_Records.Models;
using Employee_Records.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataSeederService _dataSeeder;
        private readonly PayslipCalculationService _payslipService;

        public EmployeeDashboardController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            DataSeederService dataSeeder,
            PayslipCalculationService payslipService)
        {
            _context = context;
            _userManager = userManager;
            _dataSeeder = dataSeeder;
            _payslipService = payslipService;
        }

        // View own profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Find employee record by matching email (including Department)
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email.ToLower());

            var viewModel = new EmployeeDashboardViewModel
            {
                User = user,
                Employee = employee,
                HasEmployeeRecord = employee != null
            };

            // If employee record exists, load attendance and schedule data
            if (employee != null)
            {
                // Ensure mock data exists for this employee
                await _dataSeeder.EnsureDataExistsAsync(employee.Id);

                // Get week boundaries
                var today = DateTime.Today;
                var daysSinceMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
                var weekStart = today.AddDays(-daysSinceMonday);
                var weekEnd = weekStart.AddDays(6); // Sunday

                // Load weekly attendance records
                viewModel.WeeklyAttendance = await _context.AttendanceRecords
                    .Where(a => a.EmployeeId == employee.Id && a.Date >= weekStart && a.Date <= weekEnd)
                    .OrderBy(a => a.Date)
                    .ToListAsync();

                // Load weekly schedule
                viewModel.WeeklySchedule = await _context.EmployeeSchedules
                    .Where(s => s.EmployeeId == employee.Id && s.Date >= weekStart && s.Date <= weekEnd)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                // Calculate attendance statistics
                viewModel.PresentDays = viewModel.WeeklyAttendance.Count(a => a.Status == AttendanceStatus.Present);
                viewModel.AbsentDays = viewModel.WeeklyAttendance.Count(a => a.Status == AttendanceStatus.Absent);
                viewModel.LateDays = viewModel.WeeklyAttendance.Count(a => a.Status == AttendanceStatus.Late);

                // Calculate average worked hours and break time for chart
                var recordsWithTime = viewModel.WeeklyAttendance.Where(a => a.TimeIn.HasValue && a.TimeOut.HasValue).ToList();
                if (recordsWithTime.Any())
                {
                    viewModel.AverageWorkedHours = Math.Round(recordsWithTime.Average(a => a.WorkedHours), 1);
                    viewModel.AverageBreakHours = Math.Round(recordsWithTime.Average(a => a.BreakHours), 1);
                }

                viewModel.WeekStartDate = weekStart;
                viewModel.WeekEndDate = weekEnd;
            }

            return View(viewModel);
        }

        // View all payslips for the employee
        public async Task<IActionResult> Payslips()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email!.ToLower());

            if (employee == null)
            {
                TempData["Error"] = "No employee record found for your account.";
                return RedirectToAction("Index");
            }

            var payslips = await _context.Payslips
                .Where(p => p.EmployeeId == employee.Id && p.Status == PayslipStatus.Approved)
                .OrderByDescending(p => p.PayPeriodEnd)
                .ToListAsync();

            var pendingRequests = await _context.PayslipRequests
                .Where(r => r.EmployeeId == employee.Id && r.Status == PayslipRequestStatus.Pending)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            var viewModel = new EmployeePayslipsViewModel
            {
                Employee = employee,
                ApprovedPayslips = payslips,
                PendingRequests = pendingRequests
            };

            return View(viewModel);
        }

        // View a specific payslip
        public async Task<IActionResult> ViewPayslip(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email!.ToLower());

            if (employee == null)
            {
                TempData["Error"] = "No employee record found for your account.";
                return RedirectToAction("Index");
            }

            var payslip = await _context.Payslips
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(p => p.Id == id && p.EmployeeId == employee.Id && p.Status == PayslipStatus.Approved);

            if (payslip == null)
            {
                TempData["Error"] = "Payslip not found or not yet approved.";
                return RedirectToAction("Payslips");
            }

            return View(payslip);
        }

        // Request a new payslip
        [HttpGet]
        public async Task<IActionResult> RequestPayslip()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email!.ToLower());

            if (employee == null)
            {
                TempData["Error"] = "No employee record found for your account.";
                return RedirectToAction("Index");
            }

            // Pre-populate with current pay period
            var now = DateTime.Now;
            var viewModel = new RequestPayslipViewModel
            {
                EmployeeId = employee.Id,
                PayPeriodMonth = now.Month,
                PayPeriodYear = now.Year,
                PayPeriodHalf = now.Day <= 15 ? 1 : 2
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPayslip(RequestPayslipViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.ToLower() == user.Email!.ToLower());

            if (employee == null)
            {
                TempData["Error"] = "No employee record found for your account.";
                return RedirectToAction("Index");
            }

            // Check if a request already exists for this period
            var existingRequest = await _context.PayslipRequests
                .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id 
                    && r.PayPeriodMonth == model.PayPeriodMonth 
                    && r.PayPeriodYear == model.PayPeriodYear
                    && r.PayPeriodHalf == model.PayPeriodHalf
                    && r.Status != PayslipRequestStatus.Rejected);

            if (existingRequest != null)
            {
                TempData["Error"] = "A payslip request for this pay period already exists.";
                return RedirectToAction("Payslips");
            }

            // Check if an approved payslip already exists for this period
            var periodStart = model.PayPeriodHalf == 1 
                ? new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 1)
                : new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 16);
            var periodEnd = model.PayPeriodHalf == 1
                ? new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 15)
                : new DateTime(model.PayPeriodYear, model.PayPeriodMonth, DateTime.DaysInMonth(model.PayPeriodYear, model.PayPeriodMonth));

            var existingPayslip = await _context.Payslips
                .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id 
                    && p.PayPeriodStart == periodStart 
                    && p.PayPeriodEnd == periodEnd
                    && p.Status == PayslipStatus.Approved);

            if (existingPayslip != null)
            {
                TempData["Info"] = "An approved payslip already exists for this period.";
                return RedirectToAction("ViewPayslip", new { id = existingPayslip.Id });
            }

            var request = new PayslipRequest
            {
                EmployeeId = employee.Id,
                PayPeriodMonth = model.PayPeriodMonth,
                PayPeriodYear = model.PayPeriodYear,
                PayPeriodHalf = model.PayPeriodHalf,
                RequestDate = DateTime.Now,
                Status = PayslipRequestStatus.Pending
            };

            _context.PayslipRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your payslip request has been submitted. Please wait for admin approval.";
            return RedirectToAction("Payslips");
        }
    }

    // ViewModels for Employee Payslip functionality
    public class EmployeePayslipsViewModel
    {
        public Employee Employee { get; set; } = null!;
        public List<Payslip> ApprovedPayslips { get; set; } = new();
        public List<PayslipRequest> PendingRequests { get; set; } = new();
    }

    public class RequestPayslipViewModel
    {
        public int EmployeeId { get; set; }
        public int PayPeriodMonth { get; set; }
        public int PayPeriodYear { get; set; }
        public int PayPeriodHalf { get; set; }
    }

    public class EmployeeDashboardViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public Employee? Employee { get; set; }
        public bool HasEmployeeRecord { get; set; }

        // Attendance data
        public List<AttendanceRecord> WeeklyAttendance { get; set; } = new();
        public List<EmployeeSchedule> WeeklySchedule { get; set; } = new();

        // Statistics
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public double AverageWorkedHours { get; set; }
        public double AverageBreakHours { get; set; }

        // Week info
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }

        // Helper for chart - scheduled hours per day (8 hours)
        public double ScheduledHoursPerDay => 8.0;

        // Compute unaccounted time (overtime or undertime)
        public double UnaccountedHours => Math.Round(ScheduledHoursPerDay - AverageWorkedHours - AverageBreakHours, 1);
    }
}

