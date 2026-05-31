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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ItemDataType>().HasKey(x => new { x.ItemId, x.DataTypeId });
        mb.Entity<ItemTopic>().HasKey(x => new { x.ItemId, x.TopicId });

        mb.Entity<Topic>()
            .HasOne(t => t.Parent)
            .WithMany(t => t.Children)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
