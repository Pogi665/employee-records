using Employee_Records.Data;
using Employee_Records.Models;
using Microsoft.EntityFrameworkCore;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Services
{
    public class DataSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random;

        public DataSeederService(ApplicationDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        /// <summary>
        /// Seeds attendance and schedule data for an employee for the current week
        /// </summary>
        public async Task SeedEmployeeDataAsync(int employeeId)
        {
            // Get the start of the current week (Monday)
            var today = DateTime.Today;
            var daysSinceMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
            var weekStart = today.AddDays(-daysSinceMonday);

            // Seed schedules for the week (Monday to Friday)
            await SeedWeeklyScheduleAsync(employeeId, weekStart);

            // Seed attendance for days that have passed
            await SeedWeeklyAttendanceAsync(employeeId, weekStart, today);
        }

        /// <summary>
        /// Seeds weekly schedule with randomized shifts
        /// </summary>
        private async Task SeedWeeklyScheduleAsync(int employeeId, DateTime weekStart)
        {
            var shiftTypes = Enum.GetValues<ShiftType>();

            for (int i = 0; i < 5; i++) // Monday to Friday
            {
                var scheduleDate = weekStart.AddDays(i);

                // Check if schedule already exists
                var existingSchedule = await _context.EmployeeSchedules
                    .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Date == scheduleDate);

                if (existingSchedule == null)
                {
                    // Randomize shift type
                    var randomShift = shiftTypes[_random.Next(shiftTypes.Length)];
                    var (shiftStart, shiftEnd) = EmployeeSchedule.GetShiftTimes(randomShift);

                    var schedule = new EmployeeSchedule
                    {
                        EmployeeId = employeeId,
                        Date = scheduleDate,
                        ShiftType = randomShift,
                        ShiftStart = shiftStart,
                        ShiftEnd = shiftEnd
                    };

                    _context.EmployeeSchedules.Add(schedule);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds attendance records for past days in the week
        /// </summary>
        private async Task SeedWeeklyAttendanceAsync(int employeeId, DateTime weekStart, DateTime today)
        {
            for (int i = 0; i < 5; i++) // Monday to Friday
            {
                var attendanceDate = weekStart.AddDays(i);

                // Only seed attendance for past days (not future)
                if (attendanceDate > today) continue;

                // Check if attendance already exists
                var existingAttendance = await _context.AttendanceRecords
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == attendanceDate);

                if (existingAttendance == null)
                {
                    // Get the schedule for this day
                    var schedule = await _context.EmployeeSchedules
                        .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.Date == attendanceDate);

                    var attendance = GenerateRandomAttendance(employeeId, attendanceDate, schedule);
                    _context.AttendanceRecords.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generates a random attendance record
        /// </summary>
        private AttendanceRecord GenerateRandomAttendance(int employeeId, DateTime date, EmployeeSchedule? schedule)
        {
            // 80% chance present, 10% absent, 10% late
            var roll = _random.Next(100);
            AttendanceStatus status;
            TimeSpan? timeIn = null;
            TimeSpan? timeOut = null;
            int breakMinutes = 60; // Default 1 hour break

            var scheduledStart = schedule?.ShiftStart ?? new TimeSpan(9, 0, 0);
            var scheduledEnd = schedule?.ShiftEnd ?? new TimeSpan(17, 0, 0);

            if (roll < 10) // 10% absent
            {
                status = AttendanceStatus.Absent;
                // No time in/out for absent
            }
            else if (roll < 20) // 10% late
            {
                status = AttendanceStatus.Late;
                // Late by 10-45 minutes
                var lateMinutes = _random.Next(10, 46);
                timeIn = scheduledStart.Add(TimeSpan.FromMinutes(lateMinutes));
                
                // May leave on time or slightly late
                var extraMinutes = _random.Next(-5, 16);
                timeOut = scheduledEnd.Add(TimeSpan.FromMinutes(extraMinutes));
                
                // Randomize break (45-75 minutes)
                breakMinutes = _random.Next(45, 76);
            }
            else // 80% present (on time)
            {
                status = AttendanceStatus.Present;
                
                // Arrive 0-10 minutes early
                var earlyMinutes = _random.Next(0, 11);
                timeIn = scheduledStart.Subtract(TimeSpan.FromMinutes(earlyMinutes));
                
                // Leave on time or slightly over
                var extraMinutes = _random.Next(0, 16);
                timeOut = scheduledEnd.Add(TimeSpan.FromMinutes(extraMinutes));
                
                // Randomize break (45-75 minutes)
                breakMinutes = _random.Next(45, 76);
            }

            return new AttendanceRecord
            {
                EmployeeId = employeeId,
                Date = date,
                TimeIn = timeIn,
                TimeOut = timeOut,
                BreakMinutes = breakMinutes,
                Status = status
            };
        }

        /// <summary>
        /// Ensures data exists for an employee, seeding if necessary
        /// </summary>
        public async Task EnsureDataExistsAsync(int employeeId)
        {
            var today = DateTime.Today;
            var daysSinceMonday = ((int)today.DayOfWeek - 1 + 7) % 7;
            var weekStart = today.AddDays(-daysSinceMonday);
            var weekEnd = weekStart.AddDays(4); // Friday

            // Check if we have any schedules for this week
            var hasSchedules = await _context.EmployeeSchedules
                .AnyAsync(s => s.EmployeeId == employeeId && s.Date >= weekStart && s.Date <= weekEnd);

            if (!hasSchedules)
            {
                await SeedEmployeeDataAsync(employeeId);
            }
        }
    }
}

