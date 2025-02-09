﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using hrconnectbackend.Data;

#nullable disable

namespace hrconnectbackend.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("UserSettings", b =>
                {
                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DateFormat")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<bool>("IsTwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("NotificationsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("PrivacyLevel")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Theme")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("TimeFormat")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("TwoFactorMethod")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TwoFactorSecret")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("EmployeeId");

                    b.ToTable("UserSettings");
                });

            modelBuilder.Entity("hrconnectbackend.Models.AboutEmployee", b =>
                {
                    b.Property<int>("EmployeeInfoId")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Age")
                        .HasColumnType("int");

                    b.Property<DateOnly?>("BirthDate")
                        .HasColumnType("date");

                    b.Property<string>("ProfilePicture")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EmployeeInfoId");

                    b.ToTable("AboutEmployees");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Attendance", b =>
                {
                    b.Property<int>("AttendanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttendanceId"));

                    b.Property<TimeOnly>("ClockIn")
                        .HasColumnType("time");

                    b.Property<TimeOnly?>("ClockOut")
                        .HasColumnType("time");

                    b.Property<DateOnly>("DateToday")
                        .HasColumnType("date");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.HasKey("AttendanceId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Attendances");
                });

            modelBuilder.Entity("hrconnectbackend.Models.AttendanceCertification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<TimeSpan>("ClockIn")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("ClockOut")
                        .HasColumnType("time");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SupervisorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("SupervisorId");

                    b.ToTable("AttendanceCertifications");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Department", b =>
                {
                    b.Property<int>("DepartmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DepartmentId"));

                    b.Property<string>("DeptName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ManagerId")
                        .HasColumnType("int");

                    b.HasKey("DepartmentId");

                    b.HasIndex("ManagerId")
                        .IsUnique();

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("hrconnectbackend.Models.EducationBackground", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Degree")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FieldOfStudy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("GPA")
                        .HasColumnType("float");

                    b.Property<string>("InstitutionName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("EducationBackgrounds");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BankAccountNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BankName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("BaseSalary")
                        .HasColumnType("int");

                    b.Property<DateOnly>("CreatedAt")
                        .HasColumnType("date");

                    b.Property<int?>("DepartmentId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmergencyContactName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmergencyContactPhone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Position")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SupervisorId")
                        .HasColumnType("int");

                    b.Property<string>("TaxId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly>("UpdatedAt")
                        .HasColumnType("date");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("SupervisorId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("hrconnectbackend.Models.LeaveApplication", b =>
                {
                    b.Property<int>("LeaveApplicationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LeaveApplicationId"));

                    b.Property<DateOnly>("AppliedDate")
                        .HasColumnType("date");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("date");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SupervisorId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("LeaveApplicationId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("LeaveApplications");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Notifications", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("hrconnectbackend.Models.OTApplication", b =>
                {
                    b.Property<int>("OTApplicationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OTApplicationId"));

                    b.Property<DateOnly>("AppliedDate")
                        .HasColumnType("date");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time");

                    b.Property<string>("Reasons")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("date");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SupervisorId")
                        .HasColumnType("int");

                    b.HasKey("OTApplicationId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("OTApplications");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Payroll", b =>
                {
                    b.Property<int>("PayrollId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PayrollId"));

                    b.Property<double>("Bonus")
                        .HasColumnType("float");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<DateOnly>("PayDate")
                        .HasColumnType("date");

                    b.Property<double>("Salary")
                        .HasColumnType("float");

                    b.HasKey("PayrollId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Payrolls");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Shift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DaysOfWorked")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EmployeeShiftId")
                        .HasColumnType("int");

                    b.Property<TimeOnly>("TimeIn")
                        .HasColumnType("time");

                    b.Property<TimeOnly>("TimeOut")
                        .HasColumnType("time");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeShiftId");

                    b.ToTable("Shifts");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Supervisor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId")
                        .IsUnique();

                    b.ToTable("Supervisors");
                });

            modelBuilder.Entity("hrconnectbackend.Models.UserAccount", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailVerified")
                        .HasColumnType("bit");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("SMSVerified")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("VerificationCode")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("UserAccount");
                });

            modelBuilder.Entity("hrconnectbackend.Models.UserNotification", b =>
                {
                    b.Property<int>("NotificationId")
                        .HasColumnType("int");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.HasKey("NotificationId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("UserNotifications");
                });

            modelBuilder.Entity("UserSettings", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithOne("UserSettings")
                        .HasForeignKey("UserSettings", "EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.AboutEmployee", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithOne("AboutEmployee")
                        .HasForeignKey("hrconnectbackend.Models.AboutEmployee", "EmployeeInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Attendance", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("Attendance")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.AttendanceCertification", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("AttendanceCertifications")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("hrconnectbackend.Models.Supervisor", "Supervisor")
                        .WithMany("AttendanceCertifications")
                        .HasForeignKey("SupervisorId");

                    b.Navigation("Employee");

                    b.Navigation("Supervisor");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Department", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Supervisor", "Supervisor")
                        .WithOne("Department")
                        .HasForeignKey("hrconnectbackend.Models.Department", "ManagerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Supervisor");
                });

            modelBuilder.Entity("hrconnectbackend.Models.EducationBackground", b =>
                {
                    b.HasOne("hrconnectbackend.Models.AboutEmployee", "EmployeeInfo")
                        .WithMany("EducationBackground")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("EmployeeInfo");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Employee", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Department", "Department")
                        .WithMany("Employees")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("hrconnectbackend.Models.Supervisor", "Supervisor")
                        .WithMany("Subordinates")
                        .HasForeignKey("SupervisorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Department");

                    b.Navigation("Supervisor");
                });

            modelBuilder.Entity("hrconnectbackend.Models.LeaveApplication", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("LeaveApplication")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Notifications", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("Notifications")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.OTApplication", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("OTApplication")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Payroll", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("Payroll")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Shift", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("Shifts")
                        .HasForeignKey("EmployeeShiftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Supervisor", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithOne()
                        .HasForeignKey("hrconnectbackend.Models.Supervisor", "EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.UserAccount", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithOne("UserAccount")
                        .HasForeignKey("hrconnectbackend.Models.UserAccount", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("hrconnectbackend.Models.UserNotification", b =>
                {
                    b.HasOne("hrconnectbackend.Models.Employee", "Employee")
                        .WithMany("UserNotification")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("hrconnectbackend.Models.Notifications", "Notification")
                        .WithMany("UserNotification")
                        .HasForeignKey("NotificationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Notification");
                });

            modelBuilder.Entity("hrconnectbackend.Models.AboutEmployee", b =>
                {
                    b.Navigation("EducationBackground");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Department", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Employee", b =>
                {
                    b.Navigation("AboutEmployee");

                    b.Navigation("Attendance");

                    b.Navigation("AttendanceCertifications");

                    b.Navigation("LeaveApplication");

                    b.Navigation("Notifications");

                    b.Navigation("OTApplication");

                    b.Navigation("Payroll");

                    b.Navigation("Shifts");

                    b.Navigation("UserAccount");

                    b.Navigation("UserNotification");

                    b.Navigation("UserSettings");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Notifications", b =>
                {
                    b.Navigation("UserNotification");
                });

            modelBuilder.Entity("hrconnectbackend.Models.Supervisor", b =>
                {
                    b.Navigation("AttendanceCertifications");

                    b.Navigation("Department");

                    b.Navigation("Subordinates");
                });
#pragma warning restore 612, 618
        }
    }
}
