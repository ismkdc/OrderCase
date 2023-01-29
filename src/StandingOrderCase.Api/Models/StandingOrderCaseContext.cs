using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Enums;

namespace StandingOrderCase.Api.Models;

public class StandingOrderCaseContext : DbContext
{
    public StandingOrderCaseContext(DbContextOptions<StandingOrderCaseContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<StandingOrder> StandingOrders { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.Parse("a823ff83-b2b3-41bb-a472-5e96717ad6fe"),
                Name = "ismail",
                Email = "ism.kundakci@hotmail.com",
                PhoneNumber = "+905315718380",
                PushToken = "fooBar"
            });
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        var date = DateTime.Now;
        var addedEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        addedEntities.ForEach(e => { e.Entity.CreatedAt = date; });

        var modifiedEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified)
            .ToList();

        modifiedEntities.ForEach(e => { e.Entity.UpdatedAt = date; });

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}