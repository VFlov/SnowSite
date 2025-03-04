using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowSite.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User1Id",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User2Id",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User2Id1",
                table: "Dialogs");

            migrationBuilder.DropIndex(
                name: "IX_Dialogs_User2Id1",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "User2Id1",
                table: "Dialogs");

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_User1Id",
                table: "Dialogs",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_User2Id",
                table: "Dialogs",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User1Id",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User2Id",
                table: "Dialogs");

            migrationBuilder.AddColumn<int>(
                name: "User2Id1",
                table: "Dialogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Dialogs_User2Id1",
                table: "Dialogs",
                column: "User2Id1");

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_User1Id",
                table: "Dialogs",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_User2Id",
                table: "Dialogs",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_User2Id1",
                table: "Dialogs",
                column: "User2Id1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
