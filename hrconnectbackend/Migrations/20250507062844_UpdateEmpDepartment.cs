using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmpDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDepartments_Employees_SupervisorId",
                table: "EmployeeDepartments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDepartments_SupervisorId",
                table: "EmployeeDepartments");

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId1",
                table: "EmployeeDepartments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartments_SupervisorId1",
                table: "EmployeeDepartments",
                column: "SupervisorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDepartments_Employees_SupervisorId1",
                table: "EmployeeDepartments",
                column: "SupervisorId1",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDepartments_Employees_SupervisorId1",
                table: "EmployeeDepartments");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDepartments_SupervisorId1",
                table: "EmployeeDepartments");

            migrationBuilder.DropColumn(
                name: "SupervisorId1",
                table: "EmployeeDepartments");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartments_SupervisorId",
                table: "EmployeeDepartments",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDepartments_Employees_SupervisorId",
                table: "EmployeeDepartments",
                column: "SupervisorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
