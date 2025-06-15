using CarInsuranceBot.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.WebApi.DbContexts
{
    public class CarInsuranceDbContext : DbContext
    {
        public DbSet<MyUser> Users { get; set; }
        public DbSet<UserInputState> UserInputStates { get; set; }

        public CarInsuranceDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyUser>()
                .HasOne(u => u.InputState)
                .WithOne(st => st.User)
                .HasForeignKey<UserInputState>(st => st.UserId);

            modelBuilder.Entity<UserInputState>()
                .OwnsOne(us => us.CreateInsuranceFlow);

            base.OnModelCreating(modelBuilder);
        }
    }
}
