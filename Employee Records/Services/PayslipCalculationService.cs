using Employee_Records.Models;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Services
{
    public class PayslipCalculationService
    {
        private readonly Random _random = new Random();

        // 2024 Philippine Regular Holidays
        private readonly List<(DateTime Date, string Name, bool IsRegular)> _philippineHolidays2025 = new()
        {
            // Regular Holidays (200% pay)
            (new DateTime(2025, 1, 1), "New Year's Day", true),
            (new DateTime(2025, 4, 9), "Araw ng Kagitingan", true),
            (new DateTime(2025, 4, 17), "Maundy Thursday", true),
            (new DateTime(2025, 4, 18), "Good Friday", true),
            (new DateTime(2025, 4, 19), "Black Saturday", true),
            (new DateTime(2025, 5, 1), "Labor Day", true),
            (new DateTime(2025, 6, 12), "Independence Day", true),
            (new DateTime(2025, 8, 25), "National Heroes Day", true),
            (new DateTime(2025, 11, 30), "Bonifacio Day", true),
            (new DateTime(2025, 12, 25), "Christmas Day", true),
            (new DateTime(2025, 12, 30), "Rizal Day", true),

            // Special Non-Working Holidays (130% pay)
            (new DateTime(2025, 1, 29), "Chinese New Year", false),
            (new DateTime(2025, 2, 25), "EDSA Revolution Anniversary", false),
            (new DateTime(2025, 8, 21), "Ninoy Aquino Day", false),
            (new DateTime(2025, 11, 1), "All Saints' Day", false),
            (new DateTime(2025, 11, 2), "All Souls' Day", false),
            (new DateTime(2025, 12, 8), "Feast of the Immaculate Conception", false),
            (new DateTime(2025, 12, 24), "Christmas Eve", false),
            (new DateTime(2025, 12, 31), "New Year's Eve", false),
        };

        // SSS Contribution Table 2024 (Employee Share)
        private readonly List<(decimal MinSalary, decimal MaxSalary, decimal Contribution)> _sssTable = new()
        {
            (0, 4249.99m, 180m),
            (4250, 4749.99m, 202.50m),
            (4750, 5249.99m, 225m),
            (5250, 5749.99m, 247.50m),
            (5750, 6249.99m, 270m),
            (6250, 6749.99m, 292.50m),
            (6750, 7249.99m, 315m),
            (7250, 7749.99m, 337.50m),
            (7750, 8249.99m, 360m),
            (8250, 8749.99m, 382.50m),
            (8750, 9249.99m, 405m),
            (9250, 9749.99m, 427.50m),
            (9750, 10249.99m, 450m),
            (10250, 10749.99m, 472.50m),
            (10750, 11249.99m, 495m),
            (11250, 11749.99m, 517.50m),
            (11750, 12249.99m, 540m),
            (12250, 12749.99m, 562.50m),
            (12750, 13249.99m, 585m),
            (13250, 13749.99m, 607.50m),
            (13750, 14249.99m, 630m),
            (14250, 14749.99m, 652.50m),
            (14750, 15249.99m, 675m),
            (15250, 15749.99m, 697.50m),
            (15750, 16249.99m, 720m),
            (16250, 16749.99m, 742.50m),
            (16750, 17249.99m, 765m),
            (17250, 17749.99m, 787.50m),
            (17750, 18249.99m, 810m),
            (18250, 18749.99m, 832.50m),
            (18750, 19249.99m, 855m),
            (19250, 19749.99m, 877.50m),
            (19750, 20249.99m, 900m),
            (20250, 20749.99m, 922.50m),
            (20750, 21249.99m, 945m),
            (21250, 21749.99m, 967.50m),
            (21750, 22249.99m, 990m),
            (22250, 22749.99m, 1012.50m),
            (22750, 23249.99m, 1035m),
            (23250, 23749.99m, 1057.50m),
            (23750, 24249.99m, 1080m),
            (24250, 24749.99m, 1102.50m),
            (24750, 25249.99m, 1125m),
            (25250, 25749.99m, 1147.50m),
            (25750, 26249.99m, 1170m),
            (26250, 26749.99m, 1192.50m),
            (26750, 27249.99m, 1215m),
            (27250, 27749.99m, 1237.50m),
            (27750, 28249.99m, 1260m),
            (28250, 28749.99m, 1282.50m),
            (28750, 29249.99m, 1305m),
            (29250, 29749.99m, 1327.50m),
            (29750, decimal.MaxValue, 1350m),
        };

        /// <summary>
        /// Calculate SSS contribution based on monthly salary
        /// </summary>
        public decimal CalculateSSS(decimal monthlySalary)
        {
            foreach (var bracket in _sssTable)
            {
                if (monthlySalary >= bracket.MinSalary && monthlySalary <= bracket.MaxSalary)
                {
                    return bracket.Contribution;
                }
            }
            return 1350m; // Max contribution
        }

        /// <summary>
        /// Calculate PhilHealth contribution (Employee share = 2.5% of salary)
        /// 2024 rate: 5% total, split between employer and employee
        /// </summary>
        public decimal CalculatePhilHealth(decimal monthlySalary)
        {
            // PhilHealth rate is 5% of monthly salary, employee pays half (2.5%)
            // Minimum monthly premium floor: 500 (employee share: 250)
            // Maximum monthly premium ceiling: 5,000 (employee share: 2,500)
            decimal employeeShare = monthlySalary * 0.025m;
            
            // Apply floor and ceiling
            if (employeeShare < 250m) employeeShare = 250m;
            if (employeeShare > 2500m) employeeShare = 2500m;

            return Math.Round(employeeShare, 2);
        }

        /// <summary>
        /// Calculate Pag-IBIG contribution (Employee share)
        /// 2% of salary, maximum of 100 per month for employee
        /// </summary>
        public decimal CalculatePagIBIG(decimal monthlySalary)
        {
            // Pag-IBIG: 2% of salary, capped at 100 for employee share
            decimal contribution = monthlySalary * 0.02m;
            return Math.Min(contribution, 100m);
        }

        /// <summary>
        /// Calculate Withholding Tax based on BIR Tax Table (2024)
        /// </summary>
        public decimal CalculateWithholdingTax(decimal taxableIncome)
        {
            // Monthly Tax Table 2024
            // First 20,833 is tax exempt
            if (taxableIncome <= 20833m) return 0m;

            decimal tax = 0m;

            if (taxableIncome <= 33332m)
            {
                tax = (taxableIncome - 20833m) * 0.15m;
            }
            else if (taxableIncome <= 66666m)
            {
                tax = 1875m + (taxableIncome - 33333m) * 0.20m;
            }
            else if (taxableIncome <= 166666m)
            {
                tax = 8541.80m + (taxableIncome - 66667m) * 0.25m;
            }
            else if (taxableIncome <= 666666m)
            {
                tax = 33541.80m + (taxableIncome - 166667m) * 0.30m;
            }
            else
            {
                tax = 183541.80m + (taxableIncome - 666667m) * 0.35m;
            }

            return Math.Round(tax, 2);
        }

        /// <summary>
        /// Get holidays within a pay period
        /// </summary>
        public (int RegularHolidays, int SpecialHolidays) GetHolidaysInPeriod(DateTime start, DateTime end)
        {
            int regular = 0;
            int special = 0;

            foreach (var holiday in _philippineHolidays2025)
            {
                if (holiday.Date >= start && holiday.Date <= end)
                {
                    if (holiday.IsRegular)
                        regular++;
                    else
                        special++;
                }
            }

            return (regular, special);
        }

        /// <summary>
        /// Generate a complete payslip with random realistic values
        /// </summary>
        public Payslip GeneratePayslip(Employee employee, DateTime periodStart, DateTime periodEnd, int? requestId = null)
        {
            // Calculate working days in period (exclude weekends)
            int totalDays = 0;
            int workingDays = 0;
            for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
            {
                totalDays++;
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }

            // Get holidays in period
            var (regularHolidays, specialHolidays) = GetHolidaysInPeriod(periodStart, periodEnd);

            // Random days worked (90-100% attendance)
            int daysWorked = Math.Max(1, workingDays - _random.Next(0, Math.Max(1, workingDays / 10)));

            // Random overtime (0-20 hours)
            decimal overtimeHours = _random.Next(0, 21);

            // Random holidays worked (0 to actual holidays)
            int regularHolidaysWorked = _random.Next(0, regularHolidays + 1);
            int specialHolidaysWorked = _random.Next(0, specialHolidays + 1);

            // Calculate daily rate (monthly salary / 22 working days average)
            decimal dailyRate = employee.Salary / 22m;
            decimal hourlyRate = dailyRate / 8m;

            // Calculate earnings
            decimal basicPay = Math.Round(dailyRate * daysWorked, 2);
            
            // Holiday pay: Regular = 200% (extra 100%), Special = 130% (extra 30%)
            decimal holidayPay = Math.Round(
                (regularHolidaysWorked * dailyRate * 1.0m) + // Extra 100% for regular holidays
                (specialHolidaysWorked * dailyRate * 0.3m),  // Extra 30% for special holidays
                2);

            // Overtime pay: 125% of hourly rate
            decimal overtimePay = Math.Round(overtimeHours * hourlyRate * 1.25m, 2);

            // Random allowances (transportation, meal, etc.)
            decimal allowances = _random.Next(0, 6) * 100m; // 0-500 in 100 increments

            // Gross pay
            decimal grossPay = basicPay + holidayPay + overtimePay + allowances;

            // Calculate deductions (based on semi-monthly, so divide monthly by 2)
            decimal semiMonthlyBase = employee.Salary / 2m;
            decimal sss = CalculateSSS(employee.Salary) / 2m; // Semi-monthly
            decimal philHealth = CalculatePhilHealth(employee.Salary) / 2m;
            decimal pagIbig = CalculatePagIBIG(employee.Salary) / 2m;

            // Taxable income = Gross - SSS - PhilHealth - PagIBIG
            decimal taxableIncome = grossPay - sss - philHealth - pagIbig;
            decimal withholdingTax = CalculateWithholdingTax(taxableIncome * 2) / 2m; // Estimate based on semi-monthly

            // Random other deductions (loans, absences, etc.)
            decimal otherDeductions = _random.Next(0, 4) * 50m; // 0-150

            decimal totalDeductions = sss + philHealth + pagIbig + withholdingTax + otherDeductions;
            decimal netPay = grossPay - totalDeductions;

            return new Payslip
            {
                EmployeeId = employee.Id,
                PayPeriodStart = periodStart,
                PayPeriodEnd = periodEnd,
                BasicPay = basicPay,
                HolidayPay = holidayPay,
                OvertimePay = overtimePay,
                Allowances = allowances,
                SSS = Math.Round(sss, 2),
                PhilHealth = Math.Round(philHealth, 2),
                PagIBIG = Math.Round(pagIbig, 2),
                WithholdingTax = Math.Round(withholdingTax, 2),
                OtherDeductions = otherDeductions,
                GrossPay = grossPay,
                TotalDeductions = Math.Round(totalDeductions, 2),
                NetPay = Math.Round(netPay, 2),
                GeneratedDate = DateTime.Now,
                Status = PayslipStatus.Draft,
                DaysWorked = daysWorked,
                RegularHolidaysWorked = regularHolidaysWorked,
                SpecialHolidaysWorked = specialHolidaysWorked,
                OvertimeHours = overtimeHours,
                PayslipRequestId = requestId
            };
        }

        /// <summary>
        /// Get list of Philippine holidays for display
        /// </summary>
        public List<(DateTime Date, string Name, bool IsRegular)> GetPhilippineHolidays()
        {
            return _philippineHolidays2025;
        }
    }
}

