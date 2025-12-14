using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Employee_Records.Models.EmployeeModel;

namespace Employee_Records.Models
{
    public enum PayslipStatus
    {
        Draft,
        Approved,
        Cancelled
    }

    public class Payslip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        // Pay Period
        [Required]
        public DateTime PayPeriodStart { get; set; }

        [Required]
        public DateTime PayPeriodEnd { get; set; }

        // Earnings
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HolidayPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimePay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }

        // Philippine Deductions
        [Column(TypeName = "decimal(18,2)")]
        public decimal SSS { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PhilHealth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PagIBIG { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WithholdingTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        // Totals
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        // Metadata
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public PayslipStatus Status { get; set; } = PayslipStatus.Draft;

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        // Work Details (for reference)
        public int DaysWorked { get; set; }

        public int RegularHolidaysWorked { get; set; }

        public int SpecialHolidaysWorked { get; set; }

        public decimal OvertimeHours { get; set; }

        // Notes/Remarks
        public string? Remarks { get; set; }

        // Navigation to request (optional)
        public int? PayslipRequestId { get; set; }

        [ForeignKey("PayslipRequestId")]
        public PayslipRequest? PayslipRequest { get; set; }

        // Computed properties for display
        [NotMapped]
        public decimal TotalEarnings => BasicPay + HolidayPay + OvertimePay + Allowances;

        [NotMapped]
        public string PayPeriodDisplay => $"{PayPeriodStart:MMM dd} - {PayPeriodEnd:MMM dd, yyyy}";

        [NotMapped]
        public string StatusBadgeColor => Status switch
        {
            PayslipStatus.Approved => "bg-success",
            PayslipStatus.Draft => "bg-warning text-dark",
            PayslipStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };
    }
}

