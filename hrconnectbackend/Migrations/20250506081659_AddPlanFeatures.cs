using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanFeature_Plans_PlanId",
                table: "PlanFeature");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanFeature",
                table: "PlanFeature");

            migrationBuilder.RenameTable(
                name: "PlanFeature",
                newName: "PlanFeatures");

            migrationBuilder.RenameIndex(
                name: "IX_PlanFeature_PlanId",
                table: "PlanFeatures",
                newName: "IX_PlanFeatures_PlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanFeatures",
                table: "PlanFeatures",
                column: "FeatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanFeatures_Plans_PlanId",
                table: "PlanFeatures",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanFeatures_Plans_PlanId",
                table: "PlanFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlanFeatures",
                table: "PlanFeatures");

            migrationBuilder.RenameTable(
                name: "PlanFeatures",
                newName: "PlanFeature");

            migrationBuilder.RenameIndex(
                name: "IX_PlanFeatures_PlanId",
                table: "PlanFeature",
                newName: "IX_PlanFeature_PlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlanFeature",
                table: "PlanFeature",
                column: "FeatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanFeature_Plans_PlanId",
                table: "PlanFeature",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
