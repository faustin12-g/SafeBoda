using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeBoda.Infrastructure.Entities;

namespace SafeBoda.Infrastructure.Data;

public class SafeBodaDbContext : IdentityDbContext<ApplicationUser>
{
    public SafeBodaDbContext(DbContextOptions<SafeBodaDbContext> options) : base(options)
    {
    }

    public DbSet<RiderEntity> Riders { get; set; }
    public DbSet<DriverEntity> Drivers { get; set; }
    public DbSet<TripEntity> Trips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RiderEntity>(entity =>
        {
            entity.ToTable("Riders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<DriverEntity>(entity =>
        {
            entity.ToTable("Drivers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MotoPlateNumber).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<TripEntity>(entity =>
        {
            entity.ToTable("Trips");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RiderId).IsRequired();
            entity.Property(e => e.DriverId).IsRequired();
            entity.Property(e => e.StartLatitude).IsRequired();
            entity.Property(e => e.StartLongitude).IsRequired();
            entity.Property(e => e.EndLatitude).IsRequired();
            entity.Property(e => e.EndLongitude).IsRequired();
            entity.Property(e => e.Fare).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.RequestTime).IsRequired();
        });
    }
}

