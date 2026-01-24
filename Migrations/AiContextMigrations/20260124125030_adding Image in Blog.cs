using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace blog_app_ai_dotnet.Migrations.AiContextMigrations
{
    /// <inheritdoc />
    public partial class addingImageinBlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Author",
                table: "Blogs",
                newName: "Image");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Blogs",
                newName: "Author");
        }
    }
}
