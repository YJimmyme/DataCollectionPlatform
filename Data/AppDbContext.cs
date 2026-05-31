using DataCollectionPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace DataCollectionPlatform.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<DataType> DataTypes => Set<DataType>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ItemDataType> ItemDataTypes => Set<ItemDataType>();
    public DbSet<ItemTopic> ItemTopics => Set<ItemTopic>();
    public DbSet<PushSchedule> PushSchedules => Set<PushSchedule>();
    public DbSet<PushLog> PushLogs => Set<PushLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ItemDataType>().HasKey(x => new { x.ItemId, x.DataTypeId });
        mb.Entity<ItemTopic>().HasKey(x => new { x.ItemId, x.TopicId });

        mb.Entity<Topic>()
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<AppSetting>().HasKey(s => s.Key);
    }
}
