using Employee_Records.Data;
using Employee_Records.Models;
using Employee_Records.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employee_Records.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayslipCalculationService _payslipService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context, 
            PayslipCalculationService payslipService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _payslipService = payslipService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            var pendingPayslipRequests = await _context.PayslipRequests
                .Where(r => r.Status == PayslipRequestStatus.Pending)
                .CountAsync();

            var dashboardStats = new AdminDashboardViewModel
            {
                TotalEmployees = employees.Count,
                TotalSalary = employees.Sum(e => e.Salary),
                AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
                HighestSalary = employees.Any() ? employees.Max(e => e.Salary) : 0,
                LowestSalary = employees.Any() ? employees.Min(e => e.Salary) : 0,
                EmployeesWithLocation = employees.Count(e => e.Latitude.HasValue && e.Longitude.HasValue),
                RecentEmployees = employees.OrderByDescending(e => e.Id).Take(5).ToList(),
                DepartmentCounts = employees
                    .GroupBy(e => e.Department?.Name ?? "Unassigned")
                    .Select(g => new DepartmentCount { DepartmentName = g.Key, Count = g.Count() })
                    .OrderByDescending(d => d.Count)
                    .ToList(),
                PendingPayslipRequests = pendingPayslipRequests
            };

            return View(dashboardStats);
        }

        // View all pending payslip requests
        public async Task<IActionResult> PayslipRequests()
        {
            var requests = await _context.PayslipRequests
                .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // Generate payslip for a request
        [HttpGet]
        public async Task<IActionResult> GeneratePayslip(int requestId)
        {
            var request = await _context.PayslipRequests
                .Include(r => r.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["Error"] = "Payslip request not found.";
                return RedirectToAction("PayslipRequests");
            }

            if (request.Employee == null)
            {
                TempData["Error"] = "Employee not found for this request.";
                return RedirectToAction("PayslipRequests");
            }

            // Generate a preview payslip
            var payslip = _payslipService.GeneratePayslip(
                request.Employee, 
                request.PayPeriodStart, 
                request.PayPeriodEnd, 
                request.Id);

            var viewModel = new GeneratePayslipViewModel
            {
                Request = request,
                PreviewPayslip = payslip
            };

            return View(viewModel);
        }

        // Approve and save the generated payslip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayslip(int requestId)
        {
            var request = await _context.PayslipRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["Error"] = "Payslip request not found.";
                return RedirectToAction("PayslipRequests");
            }

            if (request.Employee == null)
            {
                TempData["Error"] = "Employee not found for this request.";
                return RedirectToAction("PayslipRequests");
            }

            var user = await _userManager.GetUserAsync(User);

            // Generate the payslip
            var payslip = _payslipService.GeneratePayslip(
                request.Employee, 
                request.PayPeriodStart, 
                request.PayPeriodEnd, 
                request.Id);

            payslip.Status = PayslipStatus.Approved;
            payslip.ApprovedBy = user?.Email ?? "Admin";
            payslip.ApprovalDate = DateTime.Now;

            _context.Payslips.Add(payslip);
            await _context.SaveChangesAsync();

            // Update the request
            request.Status = PayslipRequestStatus.Approved;
            request.ProcessedBy = user?.Email ?? "Admin";
            request.ProcessedDate = DateTime.Now;
            request.GeneratedPayslipId = payslip.Id;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Payslip approved for {request.Employee.Name} - {request.PayPeriodDisplay}";
            return RedirectToAction("PayslipRequests");
        }

        // Reject a payslip request
        [HttpGet]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var request = await _context.PayslipRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["Error"] = "Payslip request not found.";
                return RedirectToAction("PayslipRequests");
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequestConfirm(int requestId, string rejectionReason)
        {
            var request = await _context.PayslipRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                TempData["Error"] = "Payslip request not found.";
                return RedirectToAction("PayslipRequests");
            }

            var user = await _userManager.GetUserAsync(User);

            request.Status = PayslipRequestStatus.Rejected;
            request.ProcessedBy = user?.Email ?? "Admin";
            request.ProcessedDate = DateTime.Now;
            request.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Payslip request rejected for {request.Employee?.Name}";
            return RedirectToAction("PayslipRequests");
        }

        // View all payslips
        public async Task<IActionResult> ManagePayslips()
        {
            var payslips = await _context.Payslips
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .OrderByDescending(p => p.GeneratedDate)
                .ToListAsync();

            return View(payslips);
        }

        // View a specific payslip details
        public async Task<IActionResult> ViewPayslip(int id)
        {
            var payslip = await _context.Payslips
                .Include(p => p.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payslip == null)
            {
                TempData["Error"] = "Payslip not found.";
                return RedirectToAction("ManagePayslips");
            }

            return View(payslip);
        }

        // Generate payslip directly for an employee (without request)
        [HttpGet]
        public async Task<IActionResult> GenerateDirectPayslip(int? employeeId)
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .OrderBy(e => e.Name)
                .ToListAsync();

            var now = DateTime.Now;
            var viewModel = new GenerateDirectPayslipViewModel
            {
                Employees = employees,
                SelectedEmployeeId = employeeId,
                PayPeriodMonth = now.Month,
                PayPeriodYear = now.Year,
                PayPeriodHalf = now.Day <= 15 ? 1 : 2
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateDirectPayslip(GenerateDirectPayslipViewModel model)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == model.SelectedEmployeeId);

            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("GenerateDirectPayslip");
            }

            var periodStart = model.PayPeriodHalf == 1
                ? new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 1)
                : new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 16);
            var periodEnd = model.PayPeriodHalf == 1
                ? new DateTime(model.PayPeriodYear, model.PayPeriodMonth, 15)
                : new DateTime(model.PayPeriodYear, model.PayPeriodMonth, DateTime.DaysInMonth(model.PayPeriodYear, model.PayPeriodMonth));

            // Check if payslip already exists
            var existingPayslip = await _context.Payslips
                .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id
                    && p.PayPeriodStart == periodStart
                    && p.PayPeriodEnd == periodEnd);

            if (existingPayslip != null)
            {
                TempData["Error"] = "A payslip already exists for this employee and pay period.";
                return RedirectToAction("ManagePayslips");
            }

            var user = await _userManager.GetUserAsync(User);

            var payslip = _payslipService.GeneratePayslip(employee, periodStart, periodEnd);
            payslip.Status = PayslipStatus.Approved;
            payslip.ApprovedBy = user?.Email ?? "Admin";
            payslip.ApprovalDate = DateTime.Now;

            _context.Payslips.Add(payslip);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Payslip generated and approved for {employee.Name}";
            return RedirectToAction("ViewPayslip", new { id = payslip.Id });
        }
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal HighestSalary { get; set; }
        public decimal LowestSalary { get; set; }
        public int EmployeesWithLocation { get; set; }
        public List<Employee_Records.Models.EmployeeModel.Employee> RecentEmployees { get; set; } = new();
        public List<DepartmentCount> DepartmentCounts { get; set; } = new();
        public int PendingPayslipRequests { get; set; }
    }

    public class DepartmentCount
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class GeneratePayslipViewModel
    {
        public PayslipRequest Request { get; set; } = null!;
        public Payslip PreviewPayslip { get; set; } = null!;
    }

    public class GenerateDirectPayslipViewModel
    {
        public List<Employee_Records.Models.EmployeeModel.Employee> Employees { get; set; } = new();
        public int? SelectedEmployeeId { get; set; }
        public int PayPeriodMonth { get; set; }
        public int PayPeriodYear { get; set; }
        public int PayPeriodHalf { get; set; }
    }
}
