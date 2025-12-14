using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late
    }

    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Nullable for absent days
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }

        // Break time in minutes (default 60 minutes for 1 hour break)
        public int BreakMinutes { get; set; } = 60;

        [Required]
        public AttendanceStatus Status { get; set; }

        // Computed property for worked hours (excluding break)
        [NotMapped]
        public double WorkedHours
        {
            get
            {
                if (TimeIn.HasValue && TimeOut.HasValue)
                {
                    var totalMinutes = (TimeOut.Value - TimeIn.Value).TotalMinutes - BreakMinutes;
                    return Math.Max(0, totalMinutes / 60.0);
                }
                return 0;
            }
        }

        // Computed property for total scheduled hours (8 hours standard)
        [NotMapped]
        public double ScheduledHours => 8.0;

        // Computed property for break hours
        [NotMapped]
        public double BreakHours => BreakMinutes / 60.0;
    }
}

