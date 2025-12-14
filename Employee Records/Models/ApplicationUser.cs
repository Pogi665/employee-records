using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Employee_Records.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(5)]
        public string? MiddleInitial { get; set; }

        [StringLength(10)]
        public string? Suffix { get; set; }
    }
}

