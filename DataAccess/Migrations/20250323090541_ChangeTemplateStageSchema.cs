using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTemplateStageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HiringTemplateStages",
                table: "HiringTemplateStages");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "HiringTemplateStages",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HiringTemplateStages",
                table: "HiringTemplateStages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_HiringTemplateStages_TemplateId",
                table: "HiringTemplateStages",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_HiringTemplateStages",
                table: "HiringTemplateStages");

            migrationBuilder.DropIndex(
                name: "IX_HiringTemplateStages_TemplateId",
                table: "HiringTemplateStages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HiringTemplateStages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HiringTemplateStages",
                table: "HiringTemplateStages",
                columns: new[] { "TemplateId", "StageId" });
        }
    }
}
