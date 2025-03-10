using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class nullableBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Batch_BatchId",
                table: "Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "Jobs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Batch_BatchId",
                table: "Jobs",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Batch_BatchId",
                table: "Jobs");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Batch_BatchId",
                table: "Jobs",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
