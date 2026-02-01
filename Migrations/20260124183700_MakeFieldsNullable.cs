using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proffessional.Migrations
{
    /// <inheritdoc />
    public partial class MakeFieldsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                });

            migrationBuilder.CreateTable(
                name: "TowingCases",
                columns: table => new
                {
                    CaseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleBrand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChassisNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerCallbackNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncidentReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncidentPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DropLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedVendorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VendorContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TowingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TollBorderCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TollFreeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedStaffId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TowingCases", x => x.CaseId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "TowingCases");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
