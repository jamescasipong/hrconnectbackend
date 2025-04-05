using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class departmentUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptName",
                table: "Departments",
                column: "DeptName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_DeptName",
                table: "Departments");
        }
    }
}
