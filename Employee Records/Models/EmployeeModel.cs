using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Employee_Records.Models
{
    public class EmployeeModel
    {
        public class Employee
        {
            public int Id { get; set; }

            // Auto-generated Employee ID: EMP-2025-0001
            public string? EmployeeCode { get; set; }

            [Required]
            public string Name { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Position { get; set; }

            [Range(0, 999999)]
            public decimal Salary { get; set; }

            // Department relationship
            public int? DepartmentId { get; set; }

            [ForeignKey("DepartmentId")]
            public Department? Department { get; set; }

            // Location fields for Google Maps integration
            public string? Address { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
        }
    }
}