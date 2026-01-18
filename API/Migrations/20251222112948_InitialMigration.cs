using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "client");

            migrationBuilder.EnsureSchema(
                name: "customer");

            migrationBuilder.EnsureSchema(
                name: "review");

            migrationBuilder.CreateTable(
                name: "channel",
                schema: "client",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    allowed_send_sms = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    survey_id = table.Column<long>(type: "bigint", nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                schema: "customer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    mobile = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "location",
                schema: "client",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    talabat_location_i_ds = table.Column<List<int>>(type: "integer[]", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "source_review_summary",
                schema: "review",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    talabat_avg_rating = table.Column<double>(type: "double precision", nullable: true),
                    talabat_total_responses = table.Column<int>(type: "integer", nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    avg_rate = table.Column<double>(type: "double precision", nullable: false),
                    total_responses = table.Column<long>(type: "bigint", nullable: false),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_review_summary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "location_channel",
                schema: "client",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    is_qr_code_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_channel_active = table.Column<bool>(type: "boolean", nullable: false),
                    qr_code_text = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_channel", x => x.id);
                    table.ForeignKey(
                        name: "FK_location_channel_channel_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "client",
                        principalTable: "channel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_location_channel_location_location_id",
                        column: x => x.location_id,
                        principalSchema: "client",
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review",
                schema: "client",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<int>(type: "integer", nullable: false),
                    rate = table.Column<int>(type: "integer", nullable: false),
                    feedback = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    reviewer_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scraped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_date = table.Column<DateOnly>(type: "Date", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: true),
                    location_name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    sentiment = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ttl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_location_location_id",
                        column: x => x.location_id,
                        principalSchema: "client",
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_location_channel_channel_id",
                schema: "client",
                table: "location_channel",
                column: "channel_id");

            migrationBuilder.CreateIndex(
                name: "IX_location_channel_location_id",
                schema: "client",
                table: "location_channel",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_location_id",
                schema: "client",
                table: "review",
                column: "location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer",
                schema: "customer");

            migrationBuilder.DropTable(
                name: "location_channel",
                schema: "client");

            migrationBuilder.DropTable(
                name: "review",
                schema: "client");

            migrationBuilder.DropTable(
                name: "source_review_summary",
                schema: "review");

            migrationBuilder.DropTable(
                name: "channel",
                schema: "client");

            migrationBuilder.DropTable(
                name: "location",
                schema: "client");
        }
    }
}
