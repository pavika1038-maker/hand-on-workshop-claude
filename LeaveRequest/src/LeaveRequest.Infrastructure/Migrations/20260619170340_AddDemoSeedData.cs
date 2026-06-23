using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "LeaveBalanceId", "CarriedForwardDays", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "EmployeeId", "EntitledDays", "LeaveTypeId", "LeaveYear", "PendingDays", "UpdatedAt", "UpdatedBy", "UsedDays" },
                values: new object[,]
                {
                    { "10000002-0000-0000-0000-000000000001", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP002", 10m, (byte)1, 2026, 3m, null, null, 3m },
                    { "10000002-0000-0000-0000-000000000002", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP002", 30m, (byte)2, 2026, 0m, null, null, 1m },
                    { "10000002-0000-0000-0000-000000000003", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP002", 3m, (byte)3, 2026, 0m, null, null, 0m },
                    { "10000003-0000-0000-0000-000000000001", 2m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP003", 10m, (byte)1, 2026, 0m, null, null, 5m },
                    { "10000003-0000-0000-0000-000000000002", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP003", 30m, (byte)2, 2026, 0m, null, null, 0m },
                    { "10000003-0000-0000-0000-000000000003", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP003", 3m, (byte)3, 2026, 0m, null, null, 2m }
                });

            migrationBuilder.InsertData(
                table: "LeaveRequests",
                columns: new[] { "LeaveRequestId", "ApprovedAt", "ApprovedBy", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DurationDays", "EmployeeId", "EndDate", "HalfDayPeriod", "LeaveRequestRef", "LeaveTypeId", "Reason", "RejectedAt", "RejectedBy", "RejectionReason", "StartDate", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { "a0000001-0000-0000-0000-000000000001", new DateTime(2026, 2, 3, 9, 0, 0, 0, DateTimeKind.Utc), "EMP002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "EMP001", null, null, 2m, "EMP001", new DateOnly(2026, 2, 4), null, "LR-2026-00001", (byte)1, "ท่องเที่ยวต่างจังหวัด", null, null, null, new DateOnly(2026, 2, 3), 2, null, null },
                    { "a0000001-0000-0000-0000-000000000002", null, null, new DateTime(2026, 3, 4, 8, 0, 0, 0, DateTimeKind.Utc), "EMP001", null, null, 1m, "EMP001", new DateOnly(2026, 3, 6), null, "LR-2026-00002", (byte)3, "ธุระส่วนตัว", new DateTime(2026, 3, 6, 10, 0, 0, 0, DateTimeKind.Utc), "EMP002", "ช่วงนั้นคนในทีมลาหลายคน กรุณาเลื่อนวันได้ไหม", new DateOnly(2026, 3, 6), 3, null, null },
                    { "a0000001-0000-0000-0000-000000000003", null, null, new DateTime(2026, 6, 19, 8, 30, 0, 0, DateTimeKind.Utc), "EMP001", null, null, 2m, "EMP001", new DateOnly(2026, 6, 24), null, "LR-2026-00003", (byte)2, "ไม่สบาย มีไข้", null, null, null, new DateOnly(2026, 6, 23), 1, null, null },
                    { "a0000002-0000-0000-0000-000000000001", null, null, new DateTime(2026, 6, 18, 14, 0, 0, 0, DateTimeKind.Utc), "EMP002", null, null, 3m, "EMP002", new DateOnly(2026, 7, 9), null, "LR-2026-00004", (byte)1, "ไปเที่ยวต่างประเทศ", null, null, null, new DateOnly(2026, 7, 7), 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000002-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000002-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000002-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000003-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000003-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000003-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "LeaveRequestId",
                keyValue: "a0000001-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "LeaveRequestId",
                keyValue: "a0000001-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "LeaveRequestId",
                keyValue: "a0000001-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "LeaveRequestId",
                keyValue: "a0000002-0000-0000-0000-000000000001");
        }
    }
}
