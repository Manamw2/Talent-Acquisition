using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class HiringTemplateTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Department",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HiringTemplateId",
                table: "Batch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "Batch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hiringParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hiringParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hiringStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutcomeType = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<int>(type: "int", nullable: true),
                    MaxValue = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hiringStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hiringStages_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hiringTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hiringTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hiringTemplates_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hiringStageOutcomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hiringStageOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hiringStageOutcomes_hiringStages_StageId",
                        column: x => x.StageId,
                        principalTable: "hiringStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hiringStageParameters",
                columns: table => new
                {
                    StageId = table.Column<int>(type: "int", nullable: false),
                    ParameterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hiringStageParameters", x => new { x.StageId, x.ParameterId });
                    table.ForeignKey(
                        name: "FK_hiringStageParameters_hiringParameters_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "hiringParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hiringStageParameters_hiringStages_StageId",
                        column: x => x.StageId,
                        principalTable: "hiringStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HiringTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HiringStageId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiringTasks_Batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batch",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HiringTasks_hiringStages_HiringStageId",
                        column: x => x.HiringStageId,
                        principalTable: "hiringStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stageDepartmentNeeds",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    EmployeesNeeded = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stageDepartmentNeeds", x => new { x.DepartmentId, x.StageId });
                    table.ForeignKey(
                        name: "FK_stageDepartmentNeeds_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stageDepartmentNeeds_hiringStages_StageId",
                        column: x => x.StageId,
                        principalTable: "hiringStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HiringTemplateStages",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Occurrence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringTemplateStages", x => new { x.TemplateId, x.StageId });
                    table.ForeignKey(
                        name: "FK_HiringTemplateStages_hiringStages_StageId",
                        column: x => x.StageId,
                        principalTable: "hiringStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HiringTemplateStages_hiringTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "hiringTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTasks",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTasks", x => new { x.TaskId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_EmployeeTasks_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTasks_HiringTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "HiringTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_TemplateId",
                table: "Jobs",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_TemplateId",
                table: "Department",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Batch_HiringTemplateId",
                table: "Batch",
                column: "HiringTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Batch_TemplateId",
                table: "Batch",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTasks_EmployeeId",
                table: "EmployeeTasks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hiringStageOutcomes_StageId",
                table: "hiringStageOutcomes",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_hiringStageParameters_ParameterId",
                table: "hiringStageParameters",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_hiringStages_AppUserId",
                table: "hiringStages",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringTasks_BatchId",
                table: "HiringTasks",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringTasks_HiringStageId",
                table: "HiringTasks",
                column: "HiringStageId");

            migrationBuilder.CreateIndex(
                name: "IX_hiringTemplates_AppUserId",
                table: "hiringTemplates",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HiringTemplateStages_StageId",
                table: "HiringTemplateStages",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_stageDepartmentNeeds_StageId",
                table: "stageDepartmentNeeds",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batch_hiringTemplates_HiringTemplateId",
                table: "Batch",
                column: "HiringTemplateId",
                principalTable: "hiringTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Batch_hiringTemplates_TemplateId",
                table: "Batch",
                column: "TemplateId",
                principalTable: "hiringTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_hiringTemplates_TemplateId",
                table: "Department",
                column: "TemplateId",
                principalTable: "hiringTemplates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_hiringTemplates_TemplateId",
                table: "Jobs",
                column: "TemplateId",
                principalTable: "hiringTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batch_hiringTemplates_HiringTemplateId",
                table: "Batch");

            migrationBuilder.DropForeignKey(
                name: "FK_Batch_hiringTemplates_TemplateId",
                table: "Batch");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_hiringTemplates_TemplateId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_hiringTemplates_TemplateId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "EmployeeTasks");

            migrationBuilder.DropTable(
                name: "hiringStageOutcomes");

            migrationBuilder.DropTable(
                name: "hiringStageParameters");

            migrationBuilder.DropTable(
                name: "HiringTemplateStages");

            migrationBuilder.DropTable(
                name: "stageDepartmentNeeds");

            migrationBuilder.DropTable(
                name: "HiringTasks");

            migrationBuilder.DropTable(
                name: "hiringParameters");

            migrationBuilder.DropTable(
                name: "hiringTemplates");

            migrationBuilder.DropTable(
                name: "hiringStages");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_TemplateId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Department_TemplateId",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Batch_HiringTemplateId",
                table: "Batch");

            migrationBuilder.DropIndex(
                name: "IX_Batch_TemplateId",
                table: "Batch");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "HiringTemplateId",
                table: "Batch");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Batch");
        }
    }
}
