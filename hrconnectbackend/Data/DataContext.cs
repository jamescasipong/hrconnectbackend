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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Employee
            modelBuilder.Entity<Employee>().HasKey(e => e.Id);
            modelBuilder.Entity<Employee>().HasOne(e => e.Supervisor).WithMany().HasForeignKey(e => e.SupervisorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);

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
            modelBuilder.Entity<Department>().HasOne(e => e.Employee).WithOne(e => e.Department).HasForeignKey<Department>(e => e.ManagerId).OnDelete(DeleteBehavior.Cascade);

            // Shift
            modelBuilder.Entity<Shift>().HasKey(e => e.EmployeeShiftId);
            modelBuilder.Entity<Shift>().HasOne(e => e.Employee).WithOne(e => e.Shift).HasForeignKey<Shift>(e => e.EmployeeShiftId).OnDelete(DeleteBehavior.Restrict);

            // Leave Application
            modelBuilder.Entity<LeaveApplication>().HasKey(e => e.LeaveApplicationId);
            modelBuilder.Entity<LeaveApplication>().HasOne(e => e.Employee).WithMany(e => e.LeaveApplication).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);


            // Leave Approval
            modelBuilder.Entity<LeaveApproval>().HasKey(e => e.LeaveApprovalId);
            modelBuilder.Entity<LeaveApproval>().HasOne(e => e.LeaveApplication).WithOne(e => e.LeaveApproval).HasForeignKey<LeaveApproval>(e => e.LeaveApprovalId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LeaveApproval>().HasOne(e => e.Approver).WithMany(e => e.LeaveApproval).HasForeignKey(e => e.ApproverId).OnDelete(DeleteBehavior.Cascade);

            // Leaves




            // OT Application
            modelBuilder.Entity<OTApplication>().HasKey(e => e.OTApplicationId);
            modelBuilder.Entity<OTApplication>()
                .HasOne(o => o.OTApproval)
                .WithOne(o => o.OTApplication)
                .HasForeignKey<OTApplication>(o => o.OTApplicationId)  // Foreign key in OTApproval
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OTApproval>().HasKey(e => e.OTApprovalId);
            modelBuilder.Entity<OTApproval>()
                .HasOne(o => o.OTApplication)
                .WithOne(o => o.OTApproval)
                .HasForeignKey<OTApproval>(o => o.OTApprovalId)  // Foreign key in OTApplication
                .OnDelete(DeleteBehavior.Restrict);

            //Attendance
            modelBuilder.Entity<Attendance>().HasOne(e => e.Employee).WithMany(e => e.Attendance).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            



        }
    }
}
