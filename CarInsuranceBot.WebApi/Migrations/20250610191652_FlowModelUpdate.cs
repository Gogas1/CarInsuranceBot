using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsuranceBot.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FlowModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreateInsuranceFlow_DriverLicenseCacheKey",
                table: "UserInputStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreateInsuranceFlow_IdCacheKey",
                table: "UserInputStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateInsuranceFlow_DriverLicenseCacheKey",
                table: "UserInputStates");

            migrationBuilder.DropColumn(
                name: "CreateInsuranceFlow_IdCacheKey",
                table: "UserInputStates");
        }
    }
}
