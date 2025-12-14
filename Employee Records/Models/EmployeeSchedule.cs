using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Models
{
    public enum ShiftType
    {
        Morning,  // 6:00 AM - 2:00 PM
        Day,      // 9:00 AM - 5:00 PM
        Night     // 2:00 PM - 10:00 PM
    }

    public class EmployeeSchedule
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public ShiftType ShiftType { get; set; }

        [Required]
        public TimeSpan ShiftStart { get; set; }

        [Required]
        public TimeSpan ShiftEnd { get; set; }

        // Helper method to get shift display string
        [NotMapped]
        public string ShiftDisplay
        {
            get
            {
                var startTime = DateTime.Today.Add(ShiftStart).ToString("h:mm tt");
                var endTime = DateTime.Today.Add(ShiftEnd).ToString("h:mm tt");
                return $"{startTime} - {endTime}";
            }
        }

        // Helper method to get shift badge color
        [NotMapped]
        public string ShiftBadgeColor
        {
            get
            {
                return ShiftType switch
                {
                    ShiftType.Morning => "bg-info",
                    ShiftType.Day => "bg-success",
                    ShiftType.Night => "bg-secondary",
                    _ => "bg-primary"
                };
            }
        }

        // Static helper to get shift times based on type
        public static (TimeSpan Start, TimeSpan End) GetShiftTimes(ShiftType shiftType)
        {
            return shiftType switch
            {
                ShiftType.Morning => (new TimeSpan(6, 0, 0), new TimeSpan(14, 0, 0)),   // 6 AM - 2 PM
                ShiftType.Day => (new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),       // 9 AM - 5 PM
                ShiftType.Night => (new TimeSpan(14, 0, 0), new TimeSpan(22, 0, 0)),    // 2 PM - 10 PM
                _ => (new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
            };
        }
    }
}

