using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RianFriends.Domain.Learning;

#nullable disable

namespace RianFriends.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase2And3Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avatars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hunger_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_fed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avatars", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empathy_gauge = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    conversation_mode = table.Column<string>(type: "text", nullable: false, defaultValue: "Language"),
                    gauge_control_mode = table.Column<string>(type: "text", nullable: false, defaultValue: "Auto"),
                    session_number = table.Column<int>(type: "integer", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    platform = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "friend_memories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    layer = table.Column<string>(type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_friend_memories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "friend_personas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    nationality = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    target_language = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    personality = table.Column<string>(type: "text", nullable: false),
                    interests = table.Column<string[]>(type: "text[]", nullable: false),
                    speech_style = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_friend_personas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "friends",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    persona_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_friends", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "language_level_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "text", maxLength: 10, nullable: false),
                    level = table.Column<string>(type: "text", nullable: false),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language_level_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", maxLength: 20, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    code_switch_data = table.Column<CodeSwitchSegment[]>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "snacks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avatar_id = table.Column<Guid>(type: "uuid", nullable: false),
                    snack_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_snacks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wake_up_alarms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alarm_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    repeat_days = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wake_up_alarms", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_avatars_friend_id",
                table: "avatars",
                column: "friend_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversation_sessions_friend_id_created_at",
                table: "conversation_sessions",
                columns: new[] { "friend_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_conversation_sessions_user_id_friend_id",
                table: "conversation_sessions",
                columns: new[] { "user_id", "friend_id" });

            migrationBuilder.CreateIndex(
                name: "ix_device_tokens_token",
                table: "device_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_tokens_user_id",
                table: "device_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_tokens_user_id_is_active",
                table: "device_tokens",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_friend_memories_expires_at",
                table: "friend_memories",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_friend_memories_friend_id_layer",
                table: "friend_memories",
                columns: new[] { "friend_id", "layer" });

            migrationBuilder.CreateIndex(
                name: "ix_friends_user_id",
                table: "friends",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_friends_user_id_is_active",
                table: "friends",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_language_level_records_user_id_friend_id_language",
                table: "language_level_records",
                columns: new[] { "user_id", "friend_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_session_id",
                table: "messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_snacks_avatar_id",
                table: "snacks",
                column: "avatar_id");

            migrationBuilder.CreateIndex(
                name: "ix_wake_up_alarms_user_id",
                table: "wake_up_alarms",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_wake_up_alarms_user_id_is_enabled",
                table: "wake_up_alarms",
                columns: new[] { "user_id", "is_enabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avatars");

            migrationBuilder.DropTable(
                name: "conversation_sessions");

            migrationBuilder.DropTable(
                name: "device_tokens");

            migrationBuilder.DropTable(
                name: "friend_memories");

            migrationBuilder.DropTable(
                name: "friend_personas");

            migrationBuilder.DropTable(
                name: "friends");

            migrationBuilder.DropTable(
                name: "language_level_records");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "snacks");

            migrationBuilder.DropTable(
                name: "wake_up_alarms");
        }
    }
}
