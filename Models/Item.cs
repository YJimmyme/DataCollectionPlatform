using System.ComponentModel.DataAnnotations;

namespace DataCollectionPlatform.Models;

public class Item
{
    public int Id { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(500)]
    public string Title { get; set; } = "";

    public string? Summary { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceRef { get; set; }
    public string? SourcePage { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<ItemDataType> ItemDataTypes { get; set; } = [];
    public ICollection<ItemTopic> ItemTopics { get; set; } = [];
}
