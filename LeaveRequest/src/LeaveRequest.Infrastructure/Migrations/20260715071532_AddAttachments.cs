using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LeaveRequestId = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_Attachments_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "LeaveRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_LeaveRequestId",
                table: "Attachments",
                column: "LeaveRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");
        }
    }
}
