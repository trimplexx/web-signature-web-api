﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web_signature_web_api.Migrations
{
    /// <inheritdoc />
    public partial class AddExpiresAtToUserPublicKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserPublicKeys",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserPublicKeys");
        }
    }
}
