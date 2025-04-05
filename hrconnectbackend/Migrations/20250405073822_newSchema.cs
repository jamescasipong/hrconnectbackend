using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class newSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Supervisors_ManagerId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EducationBackgrounds_AboutEmployees_UserId",
                table: "EducationBackgrounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Supervisors_SupervisorId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_OTApplications_Employees_EmployeeId",
                table: "OTApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_Employees_EmployeeId",
                table: "UserSettings");

            migrationBuilder.DropTable(
                name: "Supervisors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OTApplications",
                table: "OTApplications");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceCertifications_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Departments");

            migrationBuilder.RenameTable(
                name: "OTApplications",
                newName: "OtApplications");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "UserSettings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "SMSVerified",
                table: "UserAccount",
                newName: "SmsVerified");

            migrationBuilder.RenameColumn(
                name: "OTApplicationId",
                table: "OtApplications",
                newName: "OtApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_OTApplications_EmployeeId",
                table: "OtApplications",
                newName: "IX_OtApplications_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "Employees",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Employees",
                newName: "EmployeeDepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_SupervisorId",
                table: "Employees",
                newName: "IX_Employees_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_EmployeeDepartmentId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EducationBackgrounds",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationBackgrounds_UserId",
                table: "EducationBackgrounds",
                newName: "IX_EducationBackgrounds_EmployeeId");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "UserAccount",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserAccount",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "UserAccount",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserRole",
                table: "UserAccount",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "OtApplications",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Departments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "AttendanceCertifications",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AboutEmployees",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AboutEmployees",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtApplications",
                table: "OtApplications",
                column: "OtApplicationId");

            migrationBuilder.CreateTable(
                name: "EmployeeDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    SupervisorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartments_Employees_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePosition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Position = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePosition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ResourceId = table.Column<int>(type: "integer", nullable: false),
                    Read = table.Column<bool>(type: "boolean", nullable: false),
                    Write = table.Column<bool>(type: "boolean", nullable: false),
                    Delete = table.Column<bool>(type: "boolean", nullable: false),
                    Update = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPermissions_UserAccount_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccount",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartments_DepartmentId",
                table: "EmployeeDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartments_SupervisorId",
                table: "EmployeeDepartments",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationBackgrounds_AboutEmployees_EmployeeId",
                table: "EducationBackgrounds",
                column: "EmployeeId",
                principalTable: "AboutEmployees",
                principalColumn: "EmployeeInfoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeDepartments_EmployeeDepartmentId",
                table: "Employees",
                column: "EmployeeDepartmentId",
                principalTable: "EmployeeDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeePosition_PositionId",
                table: "Employees",
                column: "PositionId",
                principalTable: "EmployeePosition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtApplications_Employees_EmployeeId",
                table: "OtApplications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_UserAccount_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "UserAccount",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationBackgrounds_AboutEmployees_EmployeeId",
                table: "EducationBackgrounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeDepartments_EmployeeDepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeePosition_PositionId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_OtApplications_Employees_EmployeeId",
                table: "OtApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_UserAccount_UserId",
                table: "UserSettings");

            migrationBuilder.DropTable(
                name: "EmployeeDepartments");

            migrationBuilder.DropTable(
                name: "EmployeePosition");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtApplications",
                table: "OtApplications");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "UserRole",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AboutEmployees");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AboutEmployees");

            migrationBuilder.RenameTable(
                name: "OtApplications",
                newName: "OTApplications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserSettings",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "SmsVerified",
                table: "UserAccount",
                newName: "SMSVerified");

            migrationBuilder.RenameColumn(
                name: "OtApplicationId",
                table: "OTApplications",
                newName: "OTApplicationId");

            migrationBuilder.RenameIndex(
                name: "IX_OtApplications_EmployeeId",
                table: "OTApplications",
                newName: "IX_OTApplications_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "Employees",
                newName: "SupervisorId");

            migrationBuilder.RenameColumn(
                name: "EmployeeDepartmentId",
                table: "Employees",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_PositionId",
                table: "Employees",
                newName: "IX_Employees_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_EmployeeDepartmentId",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "EducationBackgrounds",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EducationBackgrounds_EmployeeId",
                table: "EducationBackgrounds",
                newName: "IX_EducationBackgrounds_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "UserAccount",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserAccount",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "UserAccount",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "OTApplications",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Employees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "AttendanceCertifications",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OTApplications",
                table: "OTApplications",
                column: "OTApplicationId");

            migrationBuilder.CreateTable(
                name: "Supervisors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supervisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supervisors_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCertifications_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_EmployeeId",
                table: "Supervisors",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Supervisors_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationBackgrounds_AboutEmployees_UserId",
                table: "EducationBackgrounds",
                column: "UserId",
                principalTable: "AboutEmployees",
                principalColumn: "EmployeeInfoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Supervisors_SupervisorId",
                table: "Employees",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OTApplications_Employees_EmployeeId",
                table: "OTApplications",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_Employees_EmployeeId",
                table: "UserSettings",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
