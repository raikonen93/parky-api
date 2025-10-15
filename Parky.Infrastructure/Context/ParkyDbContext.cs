using Microsoft.EntityFrameworkCore;
using Parky.Domain.Entities;
using Parky.Domain.Enums;

namespace Parky.Infrastructure.Context
{
    public class ParkyDbContext : DbContext
    {
        public ParkyDbContext(DbContextOptions<ParkyDbContext> options)
           : base(options)
        {
        }
        public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<User> Users => Set<User>();
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

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasIndex(t => new { t.LotId, t.From, t.To }).IsUnique();
                entity.HasOne(b => b.Lot)
                       .WithMany(l => l.Bookings)
                       .HasForeignKey(b => b.LotId)
                       .OnDelete(DeleteBehavior.Cascade);
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Booking_ValidRange", "\"from\" < \"to\"");
                });
                entity.Property<uint>("xmin")
                   .IsRowVersion()
                   .IsConcurrencyToken();

                entity.HasAnnotation("Npgsql:IndexMethod", "gist");
                entity.HasAnnotation("Npgsql:ExclusionConstraint",
                    "EXCLUDE USING gist (lot_id WITH =, tstzrange(\"from\", \"to\") WITH &&)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Role)
                    .HasConversion<string>();
                entity.HasData(new User { Id = 1, Username = "driver", Password = "driverpassword", Role = UserRole.Driver },
                    new User { Id = 2, Username = "owner", Password = "ownerpassword", Role = UserRole.Owner });
            });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

    }
}
