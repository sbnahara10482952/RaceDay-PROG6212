using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CATEGORY",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORY", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.UserID);
                    table.CheckConstraint("CK_USER_Role", "[Role] IN ('Organiser', 'Participant')");
                });

            migrationBuilder.CreateTable(
                name: "EVENT",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserID = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT", x => x.EventID);
                    table.CheckConstraint("CK_EVENT_Status", "[Status] IN ('Upcoming', 'Open', 'Closed', 'Completed', 'Cancelled')");
                    table.CheckConstraint("CK_EVENT_Type", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                    table.ForeignKey(
                        name: "FK_EVENT_USER_OrganiserID",
                        column: x => x.OrganiserID,
                        principalTable: "USER",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EVENT_CATEGORY",
                columns: table => new
                {
                    EventCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    AvailableSlots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT_CATEGORY", x => x.EventCategoryID);
                    table.CheckConstraint("CK_EVENT_CATEGORY_AvailableSlots", "[AvailableSlots] >= 0 AND [AvailableSlots] <= [MaxParticipants]");
                    table.CheckConstraint("CK_EVENT_CATEGORY_EntryFee", "[EntryFee] >= 0");
                    table.CheckConstraint("CK_EVENT_CATEGORY_MaxParticipants", "[MaxParticipants] > 0");
                    table.ForeignKey(
                        name: "FK_EVENT_CATEGORY_CATEGORY_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "CATEGORY",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EVENT_CATEGORY_EVENT_EventID",
                        column: x => x.EventID,
                        principalTable: "EVENT",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ENROLMENT",
                columns: table => new
                {
                    EnrolmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantID = table.Column<int>(type: "int", nullable: false),
                    EventCategoryID = table.Column<int>(type: "int", nullable: false),
                    EnrolmentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    EnrolmentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RaceNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENROLMENT", x => x.EnrolmentID);
                    table.CheckConstraint("CK_ENROLMENT_Status", "[EnrolmentStatus] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')");
                    table.ForeignKey(
                        name: "FK_ENROLMENT_EVENT_CATEGORY_EventCategoryID",
                        column: x => x.EventCategoryID,
                        principalTable: "EVENT_CATEGORY",
                        principalColumn: "EventCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ENROLMENT_USER_ParticipantID",
                        column: x => x.ParticipantID,
                        principalTable: "USER",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RESULT",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentID = table.Column<int>(type: "int", nullable: false),
                    FinishTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Position = table.Column<int>(type: "int", nullable: true),
                    ResultStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESULT", x => x.ResultID);
                    table.CheckConstraint("CK_RESULT_Position", "[Position] IS NULL OR [Position] > 0");
                    table.CheckConstraint("CK_RESULT_Status", "[ResultStatus] IN ('Pending', 'Finished', 'DNS', 'DNF')");
                    table.ForeignKey(
                        name: "FK_RESULT_ENROLMENT_EnrolmentID",
                        column: x => x.EnrolmentID,
                        principalTable: "ENROLMENT",
                        principalColumn: "EnrolmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORY_CategoryName",
                table: "CATEGORY",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENROLMENT_EventCategoryID_RaceNumber",
                table: "ENROLMENT",
                columns: new[] { "EventCategoryID", "RaceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENROLMENT_ParticipantID",
                table: "ENROLMENT",
                column: "ParticipantID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_OrganiserID",
                table: "EVENT",
                column: "OrganiserID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CATEGORY_CategoryID",
                table: "EVENT_CATEGORY",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_CATEGORY_EventID",
                table: "EVENT_CATEGORY",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_RESULT_EnrolmentID",
                table: "RESULT",
                column: "EnrolmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_Email",
                table: "USER",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RESULT");

            migrationBuilder.DropTable(
                name: "ENROLMENT");

            migrationBuilder.DropTable(
                name: "EVENT_CATEGORY");

            migrationBuilder.DropTable(
                name: "CATEGORY");

            migrationBuilder.DropTable(
                name: "EVENT");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
