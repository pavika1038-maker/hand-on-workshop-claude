using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalHistories",
                columns: table => new
                {
                    ApprovalHistoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeaveRequestId = table.Column<string>(type: "TEXT", nullable: true),
                    CancelRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApproverId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ActionAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistories", x => x.ApprovalHistoryId);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_CancelRequests_CancelRequestId",
                        column: x => x.CancelRequestId,
                        principalTable: "CancelRequests",
                        principalColumn: "CancelRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "LeaveRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_CancelRequestId",
                table: "ApprovalHistories",
                column: "CancelRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_LeaveRequestId",
                table: "ApprovalHistories",
                column: "LeaveRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalHistories");
        }
    }
}
