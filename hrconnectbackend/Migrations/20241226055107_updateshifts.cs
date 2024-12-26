using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class updateshifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shifts_EmployeeShiftId",
                table: "Shifts");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EmployeeShiftId",
                table: "Shifts",
                column: "EmployeeShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shifts_EmployeeShiftId",
                table: "Shifts");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EmployeeShiftId",
                table: "Shifts",
                column: "EmployeeShiftId",
                unique: true);
        }
    }
}
