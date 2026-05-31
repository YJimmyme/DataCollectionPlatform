namespace DataCollectionPlatform.Models.ViewModels;

public class ItemDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string? SourceUrl { get; set; }
    public string? SourceRef { get; set; }
    public string? SourcePage { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> DataTypeNames { get; set; } = [];
    public List<string> TopicNames { get; set; } = [];
}
