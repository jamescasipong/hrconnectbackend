﻿// Importing necessary namespaces for Entity Framework, Models, and Collections
using hrconnectbackend.Models;
using hrconnectbackend.Models.Sessions;
using Microsoft.EntityFrameworkCore;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.Requests;

namespace hrconnectbackend.Data
{
    // DataContext class representing the database context for the application
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        // Constructor to initialize the DbContext with options

        // DbSets representing tables in the database
        public DbSet<Employee> Employees { get; set; }  // Employee table
        public DbSet<Department> Departments { get; set; }  // Department table
        public DbSet<Leaves> Leaves { get; set; }  // Leave types
        public DbSet<AboutEmployee> AboutEmployees { get; set; }  // About Employee information
        public DbSet<LeaveApplication> LeaveApplications { get; set; }  // Leave applications
        public DbSet<OtApplication> OtApplications { get; set; }  // OT (Overtime) applications
        public DbSet<Attendance> Attendances { get; set; }  // Employee attendance
        public DbSet<Payroll> Payrolls { get; set; }  // Payroll records
        public DbSet<Shift> Shifts { get; set; }  // Employee shift schedules
        public DbSet<EducationBackground> EducationBackgrounds { get; set; }  // Employee education details
        public DbSet<Notifications> Notifications { get; set; }  // Notifications
        public DbSet<UserNotification> UserNotifications { get; set; }  // User-specific notifications
        public DbSet<UserAccount> UserAccounts { get; set; }  // User accounts table
        public DbSet<AttendanceCertification> AttendanceCertifications { get; set; }  // Certifications for attendance
        public DbSet<LeaveBalance> LeaveBalances { get; set; }  // Leave balance records for employees
        public DbSet<UserSettings> UserSettings { get; set; }  // User settings configuration
        public DbSet<EmailSigninSession> EmailSigninSessions { get; set; }  // Email sign-in sessions
        public DbSet<ResetPasswordSession> ResetPasswordSessions { get; set; }  // Reset password sessions
        public DbSet<Plan> Plans { get; set; }  // Subscription plan details
        public DbSet<Subscription> Subscriptions { get; set; }  // Subscriptions
        public DbSet<RefreshToken> RefreshTokens { get; set; }  // Refresh tokens for authentication
        public DbSet<Organization> Organizations { get; set; }  // Organization data
        public DbSet<EmployeeDepartment> EmployeeDepartments { get; set; }  // Employee-Department relationship data
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<UsageRecord> UsageRecords { get; set; }  // Usage records for subscriptions
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Payment> Payments { get; set; }  // Payment records for subscriptions


        // Configuring relationships between the entities using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Plan>()
                .HasMany(p => p.Features)
                .WithOne(f => f.Plan)
                .HasForeignKey(f => f.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Plan>()
                .HasMany(p => p.Subscriptions)
                .WithOne(s => s.Plan)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.Payments)
                .WithOne(p => p.Subscription)
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasMany(s => s.UsageRecords)
                .WithOne(u => u.Subscription)
                .HasForeignKey(u => u.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.NextBillingDate);

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => s.Status);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();



            modelBuilder.Entity<Organization>().HasMany(o => o.Employees).WithOne(e => e.Organization).HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.Payrolls).WithOne(p => p.Organization).HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.LeaveBalances).WithOne(l => l.Organization).HasForeignKey(l => l.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.Departments).WithOne(d => d.Organization).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.Shifts).WithOne(a => a.Organization).HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.Leaves).WithOne(u => u.Organization).HasForeignKey(u => u.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.EmployeeDepartments).WithOne(d => d.Organization).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.EmployeePositions).WithOne(p => p.Organization).HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.AttendanceCertifications).WithOne(a => a.Organization).HasForeignKey(a => a.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Organization>().HasMany(o => o.LeaveApplications).WithOne(d => d.Organization).HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Department>().HasIndex(a => a.DeptName).IsUnique();
            modelBuilder.Entity<Department>().HasIndex(a => a.DepartmentGuid).IsUnique();

            modelBuilder.Entity<Leaves>().HasOne(leaves => leaves.Employee).WithOne(ed => ed.Leaves).HasForeignKey<Leaves>(leaves => leaves.EmployeeId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.CreateAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP") // This can be changed depending on the logic you want
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.Expires)
                .HasDefaultValueSql("CURRENT_TIMESTAMP") // Ensure Expires is UTC
                .ValueGeneratedOnAdd();

            // Configuring the relationship between Employee and EmployeePosition
            modelBuilder.Entity<Employee>()
                .HasOne(a => a.Position)
                .WithMany(a => a.Employees)
                .HasForeignKey(a => a.PositionId);
            modelBuilder.Entity<Employee>().HasOne(a => a.UserAccount).WithOne(a => a.Employee).HasForeignKey<Employee>(a => a.UserId);
            // Configuring the relationship between Employee and EmployeeDepartment
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.EmployeeDepartment)  // Each employee has one EmployeeDepartment
                .WithMany(ed => ed.Employees)       // An EmployeeDepartment can have many employees
                .HasForeignKey(e => e.EmployeeDepartmentId);  // Foreign Key on EmployeeDepartmentId

            // Configuring the relationship between EmployeeDepartment and Department
            modelBuilder.Entity<EmployeeDepartment>()
                .HasOne(ed => ed.Department)
                .WithMany()  // A department can have many EmployeeDepartments
                .HasForeignKey(ed => ed.DepartmentId);

            // Configuring the relationship between EmployeeDepartment and Manager (Employee)
            modelBuilder.Entity<EmployeeDepartment>().HasMany(a => a.Employees).WithOne(a => a.EmployeeDepartment).HasForeignKey(a => a.EmployeeDepartmentId).OnDelete(DeleteBehavior.SetNull);
            // Configuring the relationship between OtApplication and Employee
            modelBuilder.Entity<OtApplication>()
                .HasOne(a => a.Employee)
                .WithMany(a => a.OtApplication)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configuring the relationship between UserPermission and UserAccount
            modelBuilder.Entity<UserPermission>()
                .HasOne(a => a.User)
                .WithOne(a => a.UserPermission)
                .HasForeignKey<UserPermission>(a => a.UserId);
            
            // Configure composite or unique indexes, relationships, etc.
            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.ContactEmail)
                .IsUnique();  // Ensure unique constraint for contact email

            // Configuring the relationship between UserAccount and Organization (with deletion behavior)
            modelBuilder.Entity<UserAccount>()
                .HasOne(a => a.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(a => a.OrganizationId)  // Foreign key is OrganizationId
                .OnDelete(DeleteBehavior.Cascade); // On delete, set OrganizationId to null

            // Configuring the relationship between RefreshToken and UserAccount (Cascade on delete)
            modelBuilder.Entity<RefreshToken>()
                .HasOne(a => a.UserAccount)
                .WithMany(a => a.RefreshTokens)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // RefreshToken is deleted if UserAccount is deleted

            // Configuring the relationship between Subscription and Organization (Cascade on delete)
            modelBuilder.Entity<Subscription>()
                .HasOne(e => e.Organization)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between LeaveBalance and Employee (Restrict delete)
            modelBuilder.Entity<LeaveBalance>()
                .HasOne(e => e.Employee)
                .WithMany(l => l.LeaveBalance)
                .HasForeignKey(k => k.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring the relationship between AttendanceCertification and Employee (Restrict delete)
            modelBuilder.Entity<AttendanceCertification>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<AttendanceCertification>()
                .HasOne(a => a.Employee)
                .WithMany(a => a.AttendanceCertifications)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring the relationship between UserSettings and UserAccount (Cascade on delete)
            modelBuilder.Entity<UserSettings>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(e => e.User)
                .WithOne(u => u.UserSettings)
                .HasForeignKey<UserSettings>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between UserNotification and Employee (Restrict delete)
            modelBuilder.Entity<UserNotification>()
                .HasKey(e => e.EmployeeId);
            modelBuilder.Entity<UserNotification>()
                .HasKey(n => n.NotificationId);
            modelBuilder.Entity<UserNotification>()
                .HasOne(e => e.Employee)
                .WithMany(u => u.UserNotification)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserNotification>()
                .HasIndex(a => a.ReferenceId)
                .IsUnique();

            // Configuring the relationship between UserNotification and Notification (Restrict delete)
            modelBuilder.Entity<UserNotification>()
                .HasOne(e => e.Notification)
                .WithMany(u => u.UserNotification)
                .HasForeignKey(a => a.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring the Employee entity (primary key and relationships)
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.EmployeeDepartment)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.EmployeeDepartmentId)
                .OnDelete(DeleteBehavior.SetNull); // Set null if EmployeeDepartment is deleted
            modelBuilder.Entity<Employee>().HasOne(a => a.UserAccount).WithOne(a => a.Employee).HasForeignKey<Employee>(a => a.UserId);

            // Configuring the relationship between Employee and AboutEmployee
            modelBuilder.Entity<AboutEmployee>()
                .HasKey(e => e.EmployeeInfoId);
            modelBuilder.Entity<AboutEmployee>()
                .HasOne(e => e.Employee)
                .WithOne(e => e.AboutEmployee)
                .HasForeignKey<AboutEmployee>(e => e.EmployeeInfoId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete of AboutEmployee if Employee is deleted

            // Configuring the relationship between EducationBackground and Employee (Restrict delete)
            modelBuilder.Entity<EducationBackground>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<EducationBackground>()
                .HasOne(e => e.EmployeeInfo)
                .WithMany(ei => ei.EducationBackground)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuring the relationship between UserAccount and Employee (Cascade delete)
            modelBuilder.Entity<UserAccount>()
                .HasKey(a => a.UserId);

            // Configuring the relationship between Payroll and Employee (Cascade delete)
            modelBuilder.Entity<Payroll>()
                .HasKey(p => p.PayrollId);
            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payroll)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between Shift and Employee (Cascade delete)
            modelBuilder.Entity<Shift>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Shifts)
                .HasForeignKey(s => s.EmployeeShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between LeaveApplication and Employee (Cascade delete)
            modelBuilder.Entity<LeaveApplication>()
                .HasKey(l => l.LeaveApplicationId);
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveApplication)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between OtApplication and Employee (Cascade delete)
            modelBuilder.Entity<OtApplication>()
                .HasKey(o => o.OtApplicationId);
            modelBuilder.Entity<OtApplication>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.OtApplication)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring the relationship between Attendance and Employee (Cascade delete)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendance)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
