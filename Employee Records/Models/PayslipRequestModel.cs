using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Models
{
    public enum PayslipRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class PayslipRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        // Requested Pay Period
        [Required]
        public int PayPeriodMonth { get; set; }

        [Required]
        public int PayPeriodYear { get; set; }

        // 1 = First half (1-15), 2 = Second half (16-end)
        public int PayPeriodHalf { get; set; } = 2;

        // Request Details
        public DateTime RequestDate { get; set; } = DateTime.Now;

        public PayslipRequestStatus Status { get; set; } = PayslipRequestStatus.Pending;

        // Approval Details
        public string? ProcessedBy { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public string? RejectionReason { get; set; }

        // Link to generated payslip (after approval)
        public int? GeneratedPayslipId { get; set; }

        [ForeignKey("GeneratedPayslipId")]
        public Payslip? GeneratedPayslip { get; set; }

        // Computed properties
        [NotMapped]
        public string PayPeriodDisplay
        {
            get
            {
                var monthName = new DateTime(PayPeriodYear, PayPeriodMonth, 1).ToString("MMMM");
                var halfText = PayPeriodHalf == 1 ? "1st-15th" : "16th-End";
                return $"{monthName} {PayPeriodYear} ({halfText})";
            }
        }

        [NotMapped]
        public DateTime PayPeriodStart
        {
            get
            {
                return PayPeriodHalf == 1
                    ? new DateTime(PayPeriodYear, PayPeriodMonth, 1)
                    : new DateTime(PayPeriodYear, PayPeriodMonth, 16);
            }
        }

        [NotMapped]
        public DateTime PayPeriodEnd
        {
            get
            {
                return PayPeriodHalf == 1
                    ? new DateTime(PayPeriodYear, PayPeriodMonth, 15)
                    : new DateTime(PayPeriodYear, PayPeriodMonth, DateTime.DaysInMonth(PayPeriodYear, PayPeriodMonth));
            }
        }

        [NotMapped]
        public string StatusBadgeColor => Status switch
        {
            PayslipRequestStatus.Approved => "bg-success",
            PayslipRequestStatus.Pending => "bg-warning text-dark",
            PayslipRequestStatus.Rejected => "bg-danger",
            _ => "bg-secondary"
        };
    }
}

