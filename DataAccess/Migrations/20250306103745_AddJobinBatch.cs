using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class AddJobinBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: First add the column as nullable
            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "Batch",
                type: "int",
                nullable: true);  // Make it nullable initially

            // Step 2: Update the existing records to establish the relationship
            // This assumes that Jobs already have BatchIds pointing to Batches
            migrationBuilder.Sql(@"
            UPDATE b
            SET b.JobId = j.JobId
            FROM Batch b
            INNER JOIN Jobs j ON j.BatchId = b.BatchId
        ");

            // Step 3: Now drop and recreate the index on Jobs.BatchId
            migrationBuilder.DropIndex(
                name: "IX_Jobs_BatchId",
                table: "Jobs");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_BatchId",
                table: "Jobs",
                column: "BatchId");

            // Step 4: Make the column non-nullable after data is populated
            migrationBuilder.AlterColumn<int>(
                name: "JobId",
                table: "Batch",
                type: "int",
                nullable: true,
                defaultValue: 0);

            // Step 5: Create index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Batch_JobId",
                table: "Batch",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batch_Jobs_JobId",
                table: "Batch",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batch_Jobs_JobId",
                table: "Batch");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_BatchId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Batch_JobId",
                table: "Batch");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Batch");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_BatchId",
                table: "Jobs",
                column: "BatchId",
                unique: true);
        }
    }
}
