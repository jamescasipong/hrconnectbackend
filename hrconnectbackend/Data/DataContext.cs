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
        public DbSet<EmployeeInfo> EmployeesInfo { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<LeaveApproval> LeaveApprovals { get; set; }
        public DbSet<OTApplication> OTApplications { get; set; }
        public DbSet<OTApproval> OTApprovals { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Auth> Auths { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<EducationBackground> EducationBackgrounds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Supervisor
            modelBuilder.Entity<Supervisor>().HasKey(s => s.Id);
            modelBuilder.Entity<Supervisor>()
                .HasOne(s => s.Employee)
                .WithOne()
                .HasForeignKey<Supervisor>(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // EmployeeInfo
            modelBuilder.Entity<EmployeeInfo>().HasKey(e => e.EmployeeInfoId);
            modelBuilder.Entity<EmployeeInfo>()
                .HasOne(e => e.Employee)
                .WithOne(e => e.EmployeeInfo)
                .HasForeignKey<EmployeeInfo>(e => e.EmployeeInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            // EducationBackground
            modelBuilder.Entity<EducationBackground>().HasKey(e => e.Id);
            modelBuilder.Entity<EducationBackground>()
                .HasOne(e => e.EmployeeInfo)
                .WithMany(ei => ei.EducationBackground)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Auth
            modelBuilder.Entity<Auth>().HasKey(a => a.AuthEmpId);
            modelBuilder.Entity<Auth>()
                .HasOne(a => a.Employee)
                .WithOne(e => e.Auth)
                .HasForeignKey<Auth>(a => a.AuthEmpId)
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

            // Leave Approval
            modelBuilder.Entity<LeaveApproval>().HasKey(l => l.LeaveApprovalId);
            modelBuilder.Entity<LeaveApproval>()
                .HasOne(l => l.LeaveApplication)
                .WithOne(la => la.LeaveApproval)
                .HasForeignKey<LeaveApproval>(l => l.LeaveApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LeaveApproval>()
                .HasOne(l => l.Supervisor)
                .WithMany(s => s.LeaveApprovals)
                .HasForeignKey(l => l.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // OT Application
            modelBuilder.Entity<OTApplication>().HasKey(o => o.OTApplicationId);
            modelBuilder.Entity<OTApplication>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.OTApplication)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // OT Approval
            modelBuilder.Entity<OTApproval>().HasKey(o => o.OTApprovalId);
            modelBuilder.Entity<OTApproval>()
                .HasOne(o => o.OTApplication)
                .WithOne(oa => oa.OTApproval)
                .HasForeignKey<OTApproval>(o => o.OTApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OTApproval>()
                .HasOne(o => o.Supervisor)
                .WithMany(s => s.OTApprovals)
                .HasForeignKey(o => o.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendance)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
