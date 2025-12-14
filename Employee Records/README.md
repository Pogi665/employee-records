# 📋 Employee Records Management System

A comprehensive ASP.NET Core 8.0 web application for managing employee records, payslips, attendance tracking, and departmental organization.

---

## 🚀 Quick Start

### Prerequisites

Before running this application, ensure you have the following installed:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** - Runtime for the application
2. **[XAMPP](https://www.apachefriends.org/)** - For MySQL database (or any MySQL 8.0+ server)
3. **[Visual Studio 2022](https://visualstudio.microsoft.com/)** or **[VS Code](https://code.visualstudio.com/)** - Code editor

### Database Setup

1. **Start MySQL via XAMPP**:
   - Open XAMPP Control Panel as Administrator
   - Click "Start" next to MySQL
   - Wait for MySQL to turn green

2. **Create the database**:
   - The application uses `employee_db` by default
   - Database will be auto-created on first migration

3. **Apply migrations**:
   ```bash
   cd "Employee Records"
   dotnet ef database update
   ```

### Running the Application

```bash
cd "Employee Records"
dotnet run
```

The application will be available at: `https://localhost:5001` or `http://localhost:5000`

---

## 🔐 User Roles & Authentication

The application supports two user roles:

### 👤 Employee Role
- View personal dashboard with attendance and schedule
- Request payslips for specific pay periods
- View approved payslips
- Access own profile information

### 👔 Admin Role
- Full employee CRUD (Create, Read, Update, Delete)
- Department management
- Generate and approve payslips
- View all payslip requests
- Access comprehensive statistics dashboard

### How to Register

| Role | Registration Page | Requirements |
|------|------------------|--------------|
| Employee | `/Identity/Account/Register` | Standard registration |
| Admin | `/Identity/Account/RegisterAdmin` | Requires secret code |

> ⚠️ **Admin Registration Code**: `ADMIN2025SECRET`  
> (Configurable in `appsettings.json` under `AdminRegistrationCode`)

---

## ✨ Features

### Employee Management
- Add, edit, and delete employee records
- Auto-generated employee codes (e.g., `EMP-2025-0001`)
- Assign employees to departments
- Track employee locations with address and coordinates

### Department Management
- Create and manage organizational departments
- View employee counts per department
- Department-based reporting

### Payslip System
- **Employee-initiated requests**: Employees can request payslips for specific pay periods
- **Admin approval workflow**: Admins review and approve/reject requests
- **Direct generation**: Admins can generate payslips directly for any employee
- **Bi-monthly periods**: Support for 1st-15th and 16th-end of month periods

### Attendance & Schedule Tracking
- Weekly attendance overview
- Status tracking: Present, Absent, Late
- Work hours calculation with break time
- Mock data generation for demonstration

### Dashboard Analytics
- Total employees and salary statistics
- Department distribution charts
- Recent employees overview
- Pending payslip request notifications

---

## 📧 Email Configuration

The application uses Gmail SMTP for sending emails (password recovery, confirmations).

### Setup Gmail App Password

1. **Enable 2-Step Verification** on your Google Account:
   - Go to https://myaccount.google.com/security
   - Enable 2-Step Verification

2. **Generate App Password**:
   - Go to https://myaccount.google.com/apppasswords
   - Select app: "Mail"
   - Click "Generate"
   - Copy the 16-character password

3. **Update `appsettings.json`**:
   ```json
   "Smtp": {
     "Host": "smtp.gmail.com",
     "Port": 587,
     "UseStartTls": true,
     "User": "your-email@gmail.com",
     "Password": "your-16-char-app-password",
     "From": "your-email@gmail.com",
     "FromName": "Employee Records"
   }
   ```

> 📝 Check `mailkit-protocol.log` for SMTP debugging information.  
> For detailed email troubleshooting, see [Explanation/EMAIL_SYSTEM_FIX.md](Explanation/EMAIL_SYSTEM_FIX.md).

---

## ⚙️ Configuration Reference

### `appsettings.json` Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=employee_db;user=root;password=;"
  },
  "AdminRegistrationCode": "ADMIN2025SECRET",
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseStartTls": true,
    "User": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Geoapify": {
    "ApiKey": "your-geoapify-api-key"
  }
}
```

### External APIs

| Service | Purpose | Get API Key |
|---------|---------|-------------|
| Geoapify | Address geocoding & location services | https://www.geoapify.com/ (Free: 3,000 requests/day) |

---

## 🗂️ Project Structure

```
Employee Records/
├── Areas/
│   └── Identity/           # ASP.NET Identity pages (Login, Register, etc.)
├── Controllers/
│   ├── AdminController.cs          # Admin dashboard & payslip management
│   ├── DepartmentController.cs     # Department CRUD operations
│   ├── Employee.cs                 # Employee CRUD operations
│   ├── EmployeeDashboardController.cs  # Employee self-service
│   └── HomeController.cs           # Public pages
├── Data/
│   └── ApplicationDbContext.cs     # EF Core database context
├── Migrations/                     # Database migrations
├── Models/
│   ├── ApplicationUser.cs          # Extended Identity user
│   ├── AttendanceRecord.cs         # Attendance tracking
│   ├── DepartmentModel.cs          # Department entity
│   ├── EmployeeModel.cs            # Employee entity
│   ├── EmployeeSchedule.cs         # Work schedule
│   ├── PayslipModel.cs             # Payslip entity
│   └── PayslipRequestModel.cs      # Payslip request entity
├── Services/
│   ├── DataSeederService.cs        # Mock data generation
│   └── PayslipCalculationService.cs # Payslip calculations
├── Views/                          # Razor views
├── wwwroot/                        # Static files (CSS, JS, images)
├── Program.cs                      # Application entry point
└── appsettings.json               # Configuration
```

---

## 🛠️ Development

### Adding New Migrations

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Common Commands

```bash
# Build the project
dotnet build

# Run in development mode
dotnet run

# Run with hot reload
dotnet watch run

# Reset database (drop and recreate)
dotnet ef database drop --force
dotnet ef database update
```

---

## 🔧 Troubleshooting

### Database Connection Failed
```
Could not connect to MySQL database.
```
**Solution**: Ensure MySQL is running in XAMPP Control Panel.

### Email Not Sending
- Check that App Password is correct (no spaces)
- Verify 2FA is enabled on Gmail account
- Check `mailkit-protocol.log` for errors
- Ensure port 587 is not blocked by firewall

### Identity Pages Not Working
- Ensure `app.MapRazorPages()` is called in `Program.cs`
- Verify authentication/authorization middleware order

### Roles Not Working
- The application auto-seeds "Admin" and "Employee" roles on startup
- Check logs for role seeding errors

---

## 📄 License

This project was developed for educational purposes.

---

## 🤝 Contributors

Developed with ASP.NET Core 8.0, Entity Framework Core, MySQL, and Bootstrap.

