using DataCollectionPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace DataCollectionPlatform.Models.ViewModels;

public class ItemFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "標題為必填")]
    [StringLength(500)]
    [Display(Name = "標題")]
    public string Title { get; set; } = "";

    [Display(Name = "摘要")]
    public string? Summary { get; set; }

    [Display(Name = "來源網址")]
    public string? SourceUrl { get; set; }

    [Display(Name = "出處說明")]
    public string? SourceRef { get; set; }

    [Display(Name = "頁碼 / 章節")]
    public string? SourcePage { get; set; }

    [Display(Name = "作者 / 單位")]
    public string? Author { get; set; }

    [Display(Name = "發布日期")]
    public DateTime? PublishedAt { get; set; }

    [Display(Name = "標籤（逗號分隔）")]
    public string? Tags { get; set; }

    [Display(Name = "備註")]
    public string? Notes { get; set; }

    public List<int> SelectedDataTypeIds { get; set; } = [];
    public List<int> SelectedTopicIds { get; set; } = [];

    public List<DataType> AllDataTypes { get; set; } = [];
    public List<TopicTreeNode> TopicTree { get; set; } = [];
}

public class TopicTreeNode
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
}
