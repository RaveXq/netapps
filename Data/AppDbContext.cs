using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTTelemetryLab.Data;

public class AppDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryRecord> Records => Set<TelemetryRecord>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=iot_telemetry.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<TelemetryRecord>()
            .Property(r => r.Source)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<TelemetryRecord>()
            .HasOne(r => r.Device)
            .WithMany(d => d.Records)
            .HasForeignKey(r => r.DeviceId);
    }
}
