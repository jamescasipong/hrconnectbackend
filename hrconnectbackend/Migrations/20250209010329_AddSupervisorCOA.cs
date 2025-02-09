using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorCOA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "AttendanceCertifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCertifications_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceCertifications_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "AttendanceCertifications");
        }
    }
}
