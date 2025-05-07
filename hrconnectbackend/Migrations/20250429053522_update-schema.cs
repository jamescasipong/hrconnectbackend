using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hrconnectbackend.Migrations
{
    /// <inheritdoc />
    public partial class updateschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_UserAccount_UserId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_UserAccount_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccount_Organizations_OrganizationId",
                table: "UserAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_UserAccount_UserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_UserAccount_UserId",
                table: "UserSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccount",
                table: "UserAccount");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UserNotifications");

            migrationBuilder.RenameTable(
                name: "UserAccount",
                newName: "UserAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccount_OrganizationId",
                table: "UserAccounts",
                newName: "IX_UserAccounts_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_UserAccounts_UserId",
                table: "Employees",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_UserAccounts_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccounts_Organizations_OrganizationId",
                table: "UserAccounts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_UserAccounts_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_UserAccounts_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "UserAccounts",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_UserAccounts_UserId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_UserAccounts_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAccounts_Organizations_OrganizationId",
                table: "UserAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_UserAccounts_UserId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_UserAccounts_UserId",
                table: "UserSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAccounts",
                table: "UserAccounts");

            migrationBuilder.RenameTable(
                name: "UserAccounts",
                newName: "UserAccount");

            migrationBuilder.RenameIndex(
                name: "IX_UserAccounts_OrganizationId",
                table: "UserAccount",
                newName: "IX_UserAccount_OrganizationId");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UserNotifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAccount",
                table: "UserAccount",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_UserAccount_UserId",
                table: "Employees",
                column: "UserId",
                principalTable: "UserAccount",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_UserAccount_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "UserAccount",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAccount_Organizations_OrganizationId",
                table: "UserAccount",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_UserAccount_UserId",
                table: "UserPermissions",
                column: "UserId",
                principalTable: "UserAccount",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_UserAccount_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "UserAccount",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
