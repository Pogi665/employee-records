# Email System Fix Documentation

## Overview

This document explains the problems identified in the Employee Records application's email system and the steps taken to resolve them.

---

## Problem Description

The email system in the application was not functioning. Specifically:

1. **Password Recovery Emails Not Sent**: When users attempted to recover their password via the "Forgot Password" feature, no email was received.
2. **Email Confirmation Not Working**: Email confirmation for new registrations was also non-functional.

Users would click "Forgot Password", enter their email, see a confirmation message, but never receive any email.

---

## Root Causes Identified

### Cause 1: Email Service Not Registered in Dependency Injection

The `SmtpEmailSender` class (which implements `IEmailSender`) was defined in `SmtpSettings.cs`, but it was **never registered** in the application's dependency injection container in `Program.cs`.

**Impact**: ASP.NET Core Identity pages that inject `IEmailSender` would receive a null or default no-op implementation, meaning no emails were ever sent.

### Cause 2: Gmail Security Requirements

Gmail no longer accepts regular account passwords for third-party applications (as of May 2022). Instead, Gmail requires:
- 2-Step Verification (2FA) to be enabled on the Google account
- An **App Password** to be generated and used instead of the regular password

The `appsettings.json` contained a regular password, which Gmail would reject.

### Cause 3: Email Confirmation Check Blocking Password Reset

In `ForgotPassword.cshtml.cs`, the code checked:

```csharp
if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
{
    return RedirectToPage("./ForgotPasswordConfirmation");
}
```

Since `RequireConfirmedAccount` was set to `false`, users could register without confirming their email. However, the password reset code still checked if the email was confirmed. For users who never confirmed their email (which was all of them), password reset would **silently fail** - redirecting to a success page without sending any email.

---

## Solution Implemented

### Step 1: Register Email Service in Program.cs

Added the following using statements:

```csharp
using Employee_Records.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
```

Added service registration:

```csharp
// Email Service Configuration
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
```

**What this does**: 
- Binds the `Smtp` section from `appsettings.json` to the `SmtpSettings` class
- Registers `SmtpEmailSender` as the implementation for `IEmailSender`
- Now when Identity pages request `IEmailSender`, they receive the working SMTP sender

### Step 2: Fix Password Reset for Unconfirmed Users

Modified `ForgotPassword.cshtml.cs` to remove the email confirmation check:

**Before:**
```csharp
if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
```

**After:**
```csharp
if (user == null)
```

**What this does**: Allows password reset emails to be sent to users even if they haven't confirmed their email address.

### Step 3: Gmail App Password Instructions

Updated `appsettings.json` to include instructions and a placeholder for the App Password:

```json
"Smtp": {
    "_Instructions": "Gmail requires an App Password. Go to https://myaccount.google.com/apppasswords to generate one (requires 2FA enabled).",
    "Password": "REPLACE_WITH_YOUR_16_CHARACTER_APP_PASSWORD"
}
```

---

## Action Required: Generate Gmail App Password

To complete the email setup, you must:

1. **Enable 2-Step Verification** on your Google Account:
   - Go to https://myaccount.google.com/security
   - Under "Signing in to Google", click "2-Step Verification"
   - Follow the steps to enable it

2. **Generate an App Password**:
   - Go to https://myaccount.google.com/apppasswords
   - Select app: "Mail"
   - Select device: "Windows Computer" (or other)
   - Click "Generate"
   - Copy the 16-character password shown (e.g., `abcd efgh ijkl mnop`)

3. **Update appsettings.json**:
   - Replace `REPLACE_WITH_YOUR_16_CHARACTER_APP_PASSWORD` with your App Password
   - Remove any spaces from the password

---

## Expected Behavior After Fix

### Password Recovery Flow
1. User clicks "Forgot your password?" on the login page
2. User enters their registered email address
3. System sends a password reset email to the user
4. User receives email with a reset link
5. User clicks the link and sets a new password
6. User can now log in with the new password

### Email Confirmation Flow (if enabled)
1. User registers with an email address
2. System sends a confirmation email
3. User clicks the confirmation link
4. Email is marked as confirmed in the database

---

## Files Modified

| File | Changes Made |
|------|--------------|
| `Program.cs` | Added using statements and email service registration |
| `ForgotPassword.cshtml.cs` | Removed email confirmation check from password reset |
| `appsettings.json` | Added App Password instructions and placeholder |

---

## Testing Instructions

1. **Build and run the application**

2. **Test Password Recovery**:
   - Go to the Login page
   - Click "Forgot your password?"
   - Enter a registered user's email
   - Check the email inbox (and spam folder)
   - Click the reset link in the email
   - Set a new password
   - Log in with the new password

3. **Check for Errors**:
   - If emails still don't arrive, check the `mailkit-protocol.log` file in the project directory for SMTP errors
   - Common issues:
     - App Password not entered correctly
     - Gmail account security settings blocking access
     - Network/firewall blocking port 587

---

## Troubleshooting

### No email received
- Verify the App Password is correct (no spaces, all 16 characters)
- Check spam/junk folder
- Ensure 2FA is enabled on the Gmail account
- Check `mailkit-protocol.log` for error details

### Authentication failed error in logs
- The App Password may be incorrect
- The Gmail account may have been locked due to suspicious activity
- Try generating a new App Password

### Connection refused/timeout
- Port 587 may be blocked by firewall
- Try using port 465 with `UseStartTls: false` (SSL instead of STARTTLS)

---

*Document created as part of the Email System Fix implementation.*

