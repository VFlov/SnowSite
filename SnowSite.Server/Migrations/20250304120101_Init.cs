using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowSite.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_UserId",
                table: "Dialogs");

            migrationBuilder.DropIndex(
                name: "IX_Dialogs_UserId",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "Sender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AvatarColor",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Dialogs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Dialogs",
                newName: "User2UnreadCount");

            migrationBuilder.RenameColumn(
                name: "UnreadCount",
                table: "Dialogs",
                newName: "User2Id1");

            migrationBuilder.AddColumn<int>(
                name: "SenderId",
                table: "Messages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "User1Id",
                table: "Dialogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "User1UnreadCount",
                table: "Dialogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "User2Id",
                table: "Dialogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DialogId",
                table: "Messages",
                column: "DialogId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogs_User1Id",
                table: "Dialogs",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogs_User2Id",
                table: "Dialogs",
                column: "User2Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Dialogs_DialogId",
                table: "Messages",
                column: "DialogId",
                principalTable: "Dialogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Users_User2Id1",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Dialogs_DialogId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DialogId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Dialogs_User1Id",
                table: "Dialogs");

            migrationBuilder.DropIndex(
                name: "IX_Dialogs_User2Id",
                table: "Dialogs");

            migrationBuilder.DropIndex(
                name: "IX_Dialogs_User2Id1",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "User1Id",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "User1UnreadCount",
                table: "Dialogs");

            migrationBuilder.DropColumn(
                name: "User2Id",
                table: "Dialogs");

            migrationBuilder.RenameColumn(
                name: "User2UnreadCount",
                table: "Dialogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "User2Id1",
                table: "Dialogs",
                newName: "UnreadCount");

            migrationBuilder.AddColumn<string>(
                name: "Sender",
                table: "Messages",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarColor",
                table: "Dialogs",
                type: "TEXT",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Dialogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Dialogs_UserId",
                table: "Dialogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Users_UserId",
                table: "Dialogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
