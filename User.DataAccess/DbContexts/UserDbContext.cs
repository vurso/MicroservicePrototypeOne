using Microsoft.EntityFrameworkCore;
using User.DataAccess.Entities;

namespace User.DataAccess.DbContexts
{
    public class UserDbContext : DbContext
    {
        public DbSet<Preference> Preferences { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Preference>()
                .HasKey(k => new { k.Id });
        }
    }
}