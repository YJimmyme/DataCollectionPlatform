using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Models.ViewModels;

public class ItemIndexViewModel
{
    public string? Query { get; set; }
    public List<int> SelectedDataTypeIds { get; set; } = [];
    public List<int> SelectedTopicIds { get; set; } = [];
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string SortBy { get; set; } = "created_desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<ItemListRow> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public List<DataType> AllDataTypes { get; set; } = [];
    public List<Topic> TopicRoots { get; set; } = [];
}

public class ItemListRow
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? SourceUrl { get; set; }
    public string? SourceRef { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> DataTypeNames { get; set; } = [];
    public List<string> TopicNames { get; set; } = [];
    public List<string> TagList { get; set; } = [];
}
