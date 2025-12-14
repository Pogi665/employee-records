using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Employee_Records.Models;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<EmployeeSchedule> EmployeeSchedules { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<PayslipRequest> PayslipRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure AttendanceRecord
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure EmployeeSchedule
            builder.Entity<EmployeeSchedule>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Payslip
            builder.Entity<Payslip>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PayslipRequest
            builder.Entity<PayslipRequest>()
                .HasOne(r => r.Employee)
                .WithMany()
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PayslipRequest to Payslip relationship
            builder.Entity<PayslipRequest>()
                .HasOne(r => r.GeneratedPayslip)
                .WithMany()
                .HasForeignKey(r => r.GeneratedPayslipId)
                .OnDelete(DeleteBehavior.SetNull);

            // Add indexes for better query performance
            builder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.EmployeeId, a.Date });

            builder.Entity<EmployeeSchedule>()
                .HasIndex(s => new { s.EmployeeId, s.Date });

            builder.Entity<Payslip>()
                .HasIndex(p => new { p.EmployeeId, p.PayPeriodStart, p.PayPeriodEnd });

            builder.Entity<PayslipRequest>()
                .HasIndex(r => new { r.EmployeeId, r.Status });
        }
    }
}
