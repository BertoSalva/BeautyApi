using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

public class BeautyShopDbContext : DbContext
{
    public BeautyShopDbContext(DbContextOptions<BeautyShopDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    //public DbSet<Client> Clients { get; set; }
    //public DbSet<Stylist> Stylists { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Service> Services { get; set; }
    //public DbSet<Review> Reviews { get; set; }
    //public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().UseTpcMappingStrategy(); // Table-per-class inheritance

    
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Client)
            .WithMany()
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Cascade); // Keep cascade for client

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Stylist)
            .WithMany()
            .HasForeignKey(b => b.StylistId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths
    }

}

