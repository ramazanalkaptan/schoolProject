using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace School.Migrations
{
    /// <inheritdoc />
    public partial class mig4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "UserForgotEmail",
                table: "UserForgotPasswords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserForgotEmail",
                table: "UserForgotPasswords");

            migrationBuilder.AddColumn<int>(
                name: "UserForgotPasswordId",
                table: "Registers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registers_UserForgotPasswordId",
                table: "Registers",
                column: "UserForgotPasswordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registers_UserForgotPasswords_UserForgotPasswordId",
                table: "Registers",
                column: "UserForgotPasswordId",
                principalTable: "UserForgotPasswords",
                principalColumn: "Id");
        }
    }
}
