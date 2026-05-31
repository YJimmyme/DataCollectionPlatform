namespace DataCollectionPlatform.Models;

public class ItemTopic
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;
}
