using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EmployeeCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FullNameTh = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FullNameEn = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    HireDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ManagerId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EmployeeType = table.Column<int>(type: "INTEGER", nullable: false),
                    AgencyCompany = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    AbcStartDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    LastSyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    LeaveTypeId = table.Column<byte>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TypeCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TypeNameTh = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TypeNameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaxDaysPerYear = table.Column<decimal>(type: "REAL", nullable: true),
                    IsAvailableForOutsource = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiresMedicalCert = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.LeaveTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    LeaveBalanceId = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeaveTypeId = table.Column<byte>(type: "INTEGER", nullable: false),
                    LeaveYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EntitledDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    UsedDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    PendingDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    CarriedForwardDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.LeaveBalanceId);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    LeaveRequestId = table.Column<string>(type: "TEXT", nullable: false),
                    LeaveRequestRef = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LeaveTypeId = table.Column<byte>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DurationDays = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsHalfDay = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    HalfDayPeriod = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.LeaveRequestId);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "UQ_Employees_Code",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_LeaveTypeId",
                table: "LeaveBalances",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_LeaveBalances_Employee_Type_Year",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeaveTypeId", "LeaveYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Employee_Status",
                table: "LeaveRequests",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_LeaveRequests_Ref",
                table: "LeaveRequests",
                column: "LeaveRequestRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_LeaveTypes_TypeCode",
                table: "LeaveTypes",
                column: "TypeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "LeaveTypes");
        }
    }
}
