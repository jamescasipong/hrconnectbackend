using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace hrconnectbackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AboutEmployee> AboutEmployees { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<OTApplication> OTApplications { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<UserAccount> Auths { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<EducationBackground> EducationBackgrounds { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<AttendanceCertification> AttendanceCertifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AttendanceCertification>().HasKey(x => x.Id);
            modelBuilder.Entity<AttendanceCertification>().HasOne(a => a.Employee).WithMany(a => a.AttendanceCertifications).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSettings>().HasKey(u => u.EmployeeId);
            modelBuilder.Entity<UserSettings>()
                .HasOne(e => e.Employee)
                .WithOne(u => u.UserSettings)
                .HasForeignKey<UserSettings>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserNotification>().HasKey(e => e.EmployeeId);
            modelBuilder.Entity<UserNotification>().HasKey(n => n.NotificationId);
            modelBuilder.Entity<UserNotification>()
                .HasOne(e => e.Employee)
                .WithMany(u => u.UserNotification)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserNotification>()
                .HasOne(e => e.Notification)
                .WithMany(u => u.UserNotification)
                .HasForeignKey(a => a.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);


            // Employee
            modelBuilder.Entity<Employee>().HasKey(e => e.Id);
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Supervisor)
                .WithMany(s => s.Subordinates)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notifications
            modelBuilder.Entity<Notifications>()
                .HasOne(n => n.Employee)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
            .HasMany(e => e.Notifications)
            .WithOne(e => e.Employee)
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

            // Supervisor
            modelBuilder.Entity<Supervisor>().HasKey(s => s.Id);
            modelBuilder.Entity<Supervisor>()
                .HasOne(e => e.Employee)
                .WithOne()
                .HasForeignKey<Supervisor>(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmployeeInfo
            modelBuilder.Entity<AboutEmployee>().HasKey(e => e.EmployeeInfoId);
            modelBuilder.Entity<AboutEmployee>()
                .HasOne(e => e.Employee)
                .WithOne(e => e.AboutEmployee)
                .HasForeignKey<AboutEmployee>(e => e.EmployeeInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            // EducationBackground
            modelBuilder.Entity<EducationBackground>().HasKey(e => e.Id);
            modelBuilder.Entity<EducationBackground>()
                .HasOne(e => e.EmployeeInfo)
                .WithMany(ei => ei.EducationBackground)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Auth
            modelBuilder.Entity<UserAccount>().HasKey(a => a.UserId);
            modelBuilder.Entity<UserAccount>()
                .HasOne(a => a.Employee)
                .WithOne(e => e.UserAccount)
                .HasForeignKey<UserAccount>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payroll
            modelBuilder.Entity<Payroll>().HasKey(p => p.PayrollId);
            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payroll)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Department
            modelBuilder.Entity<Department>().HasKey(d => d.DepartmentId);
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Supervisor)
                .WithOne(s => s.Department)
                .HasForeignKey<Department>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Shift
            modelBuilder.Entity<Shift>().HasKey(s => s.Id);
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Shifts)
                .HasForeignKey(s => s.EmployeeShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            // Leave Application
            modelBuilder.Entity<LeaveApplication>().HasKey(l => l.LeaveApplicationId);
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveApplication)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);


            // OT Application
            modelBuilder.Entity<OTApplication>().HasKey(o => o.OTApplicationId);
            modelBuilder.Entity<OTApplication>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.OTApplication)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attendance
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendance)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
