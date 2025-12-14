using Employee_Records.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Employee_Records.Models.EmployeeModel;

namespace MvcMySqlApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to populate departments dropdown
        private async Task PopulateDepartmentsDropdown(int? selectedDepartmentId = null)
        {
            var departments = await _context.Departments.OrderBy(d => d.Name).ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartmentId);
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();
            return View(employees);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // CREATE GET
        public async Task<IActionResult> Create()
        {
            await PopulateDepartmentsDropdown();
            return View();
        }

        // CREATE POST
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsDropdown(employee.DepartmentId);
                return View(employee);
            }

            // Generate Employee ID: EMP-2025-0001
            var year = DateTime.Now.Year;
            var count = await _context.Employees.CountAsync();
            var sequence = (count + 1).ToString("D4");
            employee.EmployeeCode = $"EMP-{year}-{sequence}";

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            
            await PopulateDepartmentsDropdown(employee.DepartmentId);
            return View(employee);
        }

        // EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsDropdown(employee.DepartmentId);
                return View(employee);
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
