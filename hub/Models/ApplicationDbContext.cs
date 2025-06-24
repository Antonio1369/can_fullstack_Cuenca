using Microsoft.EntityFrameworkCore;

namespace hub.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserQuery> UserQueries { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserQueries)
                .WithOne(uq => uq.User)
                .HasForeignKey(uq => uq.UserId);  

            base.OnModelCreating(modelBuilder);
        }
    }
}
