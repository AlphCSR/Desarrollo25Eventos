using Microsoft.EntityFrameworkCore;
using UsersMS.Domain.Entities;
using UsersMS.Infrastructure.Persistence.Configurations;

namespace UsersMS.Infrastructure.Persistence
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserHistory> UserHistories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }
}