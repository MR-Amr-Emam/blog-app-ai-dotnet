using Microsoft.EntityFrameworkCore;

namespace AiContextModels
{
    public class AiContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AiContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public AiContext(DbContextOptions<AiContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(
                    _configuration.GetConnectionString("AiDbConnection"),
                    ServerVersion.AutoDetect(_configuration.GetConnectionString("AiDbConnection"))
                );
            }
        }
    }
}