using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveRequest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixManagerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: "EMP003",
                column: "ManagerId",
                value: "EMP002");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: "EMP003",
                column: "ManagerId",
                value: null);
        }
    }
}
