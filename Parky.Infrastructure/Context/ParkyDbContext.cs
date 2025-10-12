using Microsoft.EntityFrameworkCore;
using Parky.Domain.Entities;

namespace Parky.Infrastructure.Context
{
    public class ParkyDbContext : DbContext
    {
        public ParkyDbContext(DbContextOptions<ParkyDbContext> options)
           : base(options)
        {
        }
        public DbSet<ParkingLot> ParkingLots { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ParkingLot>(entity =>
            {
                entity.HasIndex(t => t.Name);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_ParkingLot_Capacity_Positive", "capacity >= 0");
                    t.HasCheckConstraint("CK_ParkingLot_Name_NotEmpty", "char_length(name) > 0");
                });
            });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

    }
}
