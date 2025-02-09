using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTypeTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "AttendanceCertifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications");

            migrationBuilder.AlterColumn<int>(
                name: "SupervisorId",
                table: "AttendanceCertifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceCertifications_Supervisors_SupervisorId",
                table: "AttendanceCertifications",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
