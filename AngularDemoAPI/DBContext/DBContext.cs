using Microsoft.EntityFrameworkCore;
using WebApi.DBContext.DBModel;

namespace WebApi.DBContext
{
    public class db_Context : DbContext
    {
        public db_Context(DbContextOptions<db_Context> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
        }

    }
}
