using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedEmployeesAndBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancelRequests",
                columns: table => new
                {
                    CancelRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CancelRequestRef = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    LeaveRequestId = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    SlaDeadline = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SlaReminderSentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SlaEscalatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelRequests", x => x.CancelRequestId);
                    table.ForeignKey(
                        name: "FK_CancelRequests_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "LeaveRequestId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportLogs",
                columns: table => new
                {
                    ImportLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImportedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorDetailsJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsRolledBack = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportLogs", x => x.ImportLogId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    NotificationLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CloudEventType = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CancelRequestId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RecipientsJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.NotificationLogId);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AbcStartDate", "AgencyCompany", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Department", "Email", "EmployeeCode", "EmployeeType", "FullNameEn", "FullNameTh", "HireDate", "IsActive", "LastSyncedAt", "ManagerId", "Position", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { "EMP002", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "Engineering", "wipa@abc.com", "E002", 1, "Wipa Rakkan", "วิภา รักงาน", new DateOnly(2019, 6, 1), 1, null, null, "Engineering Manager", null, null },
                    { "EMP003", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "HR", "nanta@abc.com", "E003", 1, "Nanta Meesuk", "นันทา มีสุข", new DateOnly(2020, 3, 1), 1, null, null, "HR Officer", null, null }
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "LeaveTypeId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsAvailableForOutsource", "MaxDaysPerYear", "RequiresMedicalCert", "TypeCode", "TypeNameEn", "TypeNameTh", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { (byte)1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, 0, 10m, 0, "ANNUAL", "Annual Leave", "ลาพักร้อน", null, null },
                    { (byte)2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, 1, 30m, 1, "SICK", "Sick Leave", "ลาป่วย", null, null },
                    { (byte)3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, 0, 3m, 0, "PERSONAL", "Personal Leave", "ลากิจ", null, null },
                    { (byte)4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, 0, 98m, 1, "MATERNITY", "Maternity Leave", "ลาคลอด", null, null }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "AbcStartDate", "AgencyCompany", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Department", "Email", "EmployeeCode", "EmployeeType", "FullNameEn", "FullNameTh", "HireDate", "IsActive", "LastSyncedAt", "ManagerId", "Position", "UpdatedAt", "UpdatedBy" },
                values: new object[] { "EMP001", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "Engineering", "somchai@abc.com", "E001", 1, "Somchai Jaidee", "สมชาย ใจดี", new DateOnly(2022, 1, 15), 1, null, "EMP002", "Software Engineer", null, null });

            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "LeaveBalanceId", "CarriedForwardDays", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "EmployeeId", "EntitledDays", "LeaveTypeId", "LeaveYear", "PendingDays", "UpdatedAt", "UpdatedBy", "UsedDays" },
                values: new object[,]
                {
                    { "10000001-0000-0000-0000-000000000001", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP001", 10m, (byte)1, 2026, 0m, null, null, 2m },
                    { "10000001-0000-0000-0000-000000000002", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP001", 30m, (byte)2, 2026, 0m, null, null, 3m },
                    { "10000001-0000-0000-0000-000000000003", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP001", 3m, (byte)3, 2026, 0m, null, null, 1m },
                    { "10000001-0000-0000-0000-000000000004", 0m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SYSTEM", null, null, "EMP001", 98m, (byte)4, 2026, 0m, null, null, 0m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancelRequests_CancelRequestRef",
                table: "CancelRequests",
                column: "CancelRequestRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CancelRequests_LeaveRequestId",
                table: "CancelRequests",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CancelRequests_Status_IsDeleted_SlaDeadline",
                table: "CancelRequests",
                columns: new[] { "Status", "IsDeleted", "SlaDeadline" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_CancelRequestId",
                table: "NotificationLogs",
                column: "CancelRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_LeaveRequestId",
                table: "NotificationLogs",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_NotificationLogId_Status",
                table: "NotificationLogs",
                columns: new[] { "NotificationLogId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancelRequests");

            migrationBuilder.DropTable(
                name: "ImportLogs");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: "EMP003");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000001-0000-0000-0000-000000000001");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000001-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000001-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "LeaveBalanceId",
                keyValue: "10000001-0000-0000-0000-000000000004");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: "EMP001");

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: (byte)1);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: (byte)2);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: (byte)3);

            migrationBuilder.DeleteData(
                table: "LeaveTypes",
                keyColumn: "LeaveTypeId",
                keyValue: (byte)4);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: "EMP002");
        }
    }
}
