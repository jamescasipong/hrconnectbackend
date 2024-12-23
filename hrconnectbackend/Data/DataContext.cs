using hrconnectbackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace hrconnectbackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Employee
            modelBuilder.Entity<Employee>().HasKey(e => e.Id);
            modelBuilder.Entity<Employee>().HasOne(e => e.Supervisor).WithMany(e => e.Subordinates).HasForeignKey(e => e.SupervisorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Department).WithMany(e => e.Employees).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supervisor>().HasKey(e => e.Id);
            modelBuilder.Entity<Supervisor>()
                .HasOne(s => s.Employee)
                .WithOne()
                .HasForeignKey<Supervisor>(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);


            // EmployeeInfo
            modelBuilder.Entity<EmployeeInfo>().HasKey(e => e.EmployeeInfoId);
            modelBuilder.Entity<EmployeeInfo>().HasOne(e => e.Employee).WithOne(e => e.EmployeeInfo).HasForeignKey<EmployeeInfo>(e => e.EmployeeInfoId).OnDelete(DeleteBehavior.Cascade);

            // Auth
            modelBuilder.Entity<Auth>().HasKey(e => e.AuthEmpId);
            modelBuilder.Entity<Auth>().HasOne(e => e.Employee).WithOne(e => e.Auth).HasForeignKey<Auth>(e => e.AuthEmpId).OnDelete(DeleteBehavior.Cascade);

            // Payroll
            modelBuilder.Entity<Payroll>().HasKey(e => e.PayrollId);
            modelBuilder.Entity<Payroll>().HasOne(e => e.Employee).WithMany(e => e.Payroll).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);

            // Dept
            modelBuilder.Entity<Department>().HasKey(e => e.DepartmentId);
            modelBuilder.Entity<Department>().HasOne(e => e.Supervisor).WithOne(e => e.Department).HasForeignKey<Department>(e => e.ManagerId).OnDelete(DeleteBehavior.Restrict);

            // Shift
            modelBuilder.Entity<Shift>().HasKey(e => e.EmployeeShiftId);
            modelBuilder.Entity<Shift>().HasOne(e => e.Employee).WithOne(e => e.Shift).HasForeignKey<Shift>(e => e.EmployeeShiftId).OnDelete(DeleteBehavior.Restrict);

            // Leave Application
            modelBuilder.Entity<LeaveApplication>().HasKey(e => e.LeaveApplicationId);
            modelBuilder.Entity<LeaveApplication>().HasOne(e => e.Employee).WithMany(e => e.LeaveApplication).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);


            // Leave Approval
            modelBuilder.Entity<LeaveApproval>().HasKey(e => e.LeaveApprovalId);
            modelBuilder.Entity<LeaveApproval>()
                .HasOne(e => e.LeaveApplication)
                .WithOne(e => e.LeaveApproval)
                .HasForeignKey<LeaveApproval>(e => e.LeaveApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LeaveApproval>()
                .HasOne(e => e.Supervisor)
                .WithMany(e => e.LeaveApprovals)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.Cascade);


            // OT Application
            modelBuilder.Entity<OTApplication>().HasKey(e => e.OTApplicationId);
            modelBuilder.Entity<OTApplication>()
                .HasOne(o => o.Employee)
                .WithMany(o => o.OTApplication)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OTApproval>().HasKey(e => e.OTApprovalId);
            modelBuilder.Entity<OTApproval>()
                .HasOne(o => o.OTApplication)
                .WithOne(o => o.OTApproval)
                .HasForeignKey<OTApproval>(o => o.OTApplicationId)  // Foreign key in OTApplication
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OTApproval>()
                .HasOne(o => o.Supervisor)
                .WithMany(o => o.OTApprovals)
                .HasForeignKey(o => o.SupervisorId)
                .OnDelete(DeleteBehavior.Cascade);

            //Attendance
            modelBuilder.Entity<Attendance>().
                HasOne(e => e.Employee)
                .WithMany(e => e.Attendance)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);




        }
    }
}
