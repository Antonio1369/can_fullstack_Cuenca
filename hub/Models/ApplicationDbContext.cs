using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace hub.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserQuery> UserQueries { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
        public DbSet<Noticias> Noticias { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user", "public");
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Username).HasColumnName("username");
                entity.Property(u => u.Password).HasColumnName("password");
                entity.Property(u => u.Email).HasColumnName("email");
            });

            // Configure UserQuery entity
            modelBuilder.Entity<UserQuery>(entity =>
            {
                entity.ToTable("userquery", "public");
                entity.Property(uq => uq.Id).HasColumnName("id");
                entity.Property(uq => uq.QueryText).HasColumnName("query_text");
                entity.Property(uq => uq.QueryEmbedding).HasColumnName("query_embedding");
                entity.Property(uq => uq.CreatedAt).HasColumnName("created_at");
                entity.Property(uq => uq.UserId).HasColumnName("user_id");
            });

            // Configure UserFile entity
            modelBuilder.Entity<UserFile>(entity =>
            {
                entity.ToTable("user_files", "public");
                entity.Property(uf => uf.Id).HasColumnName("id");                
                entity.Property(uf => uf.Filename).HasColumnName("filename");
                entity.Property(uf => uf.Processed).HasColumnName("processed");
                entity.Property(uf => uf.UploadedAt).HasColumnName("uploaded_at");
                entity.Property(uf => uf.UserId).HasColumnName("user_id");
            });

            // Configure Noticias entity
            modelBuilder.Entity<Noticias>(entity =>
            {
                entity.ToTable("noticias", "public");
                entity.Property(n => n.Id).HasColumnName("id");
                entity.Property(n => n.Url).HasColumnName("url");
                entity.Property(n => n.Title).HasColumnName("title");
                entity.Property(n => n.Content).HasColumnName("content");
                entity.Property(n => n.Embedding).HasColumnName("embedding");
            });

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserQueries)
                .WithOne(uq => uq.User)
                .HasForeignKey(uq => uq.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserFiles)
                .WithOne(uf => uf.User)
                .HasForeignKey(uf => uf.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}