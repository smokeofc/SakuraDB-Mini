using Microsoft.EntityFrameworkCore;
using SakuraDB_Mini.Models;

namespace SakuraDB_Mini.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ProcessedFileInfo> FileInfos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add any additional configurations for the model here
            modelBuilder.Entity<ProcessedFileInfo>()
                .HasIndex(f => f.MD5);

            modelBuilder.Entity<ProcessedFileInfo>()
                .HasIndex(f => f.SHA1);
        }
    }
}