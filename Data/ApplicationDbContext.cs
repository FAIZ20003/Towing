using Microsoft.EntityFrameworkCore;
using Proffessional.Models;

namespace Proffessional.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TowingCase>()
                .HasKey(t => t.CaseId);
        }

        public DbSet<CaseHistory> CaseHistory { get; set; }
        public DbSet<TowingCase> TowingCases { get; set; }

        public DbSet<TowingCaseImage> TowingCaseImages { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
