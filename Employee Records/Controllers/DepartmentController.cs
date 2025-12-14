using Employee_Records.Data;
using Employee_Records.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employee_Records.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentListViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = _context.Employees.Count(e => e.DepartmentId == d.Id)
                })
                .ToListAsync();

            return View(departments);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var employees = await _context.Employees
                .Where(e => e.DepartmentId == id)
                .ToListAsync();

            ViewBag.Employees = employees;
            return View(department);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid) return View(department);

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Department '{department.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();
            return View(department);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department)
        {
            if (!ModelState.IsValid) return View(department);

            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Department '{department.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var employeeCount = await _context.Employees.CountAsync(e => e.DepartmentId == id);
            ViewBag.EmployeeCount = employeeCount;

            return View(department);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            // Check if any employees are assigned to this department
            var employeeCount = await _context.Employees.CountAsync(e => e.DepartmentId == id);
            if (employeeCount > 0)
            {
                TempData["Error"] = $"Cannot delete '{department.Name}' because {employeeCount} employee(s) are assigned to it. Reassign them first.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Department '{department.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class DepartmentListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EmployeeCount { get; set; }
    }
}
