using DataCollectionPlatform.Models;
using System.ComponentModel.DataAnnotations;

namespace DataCollectionPlatform.Models.ViewModels;

public class PushFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(200)]
    [Display(Name = "排程名稱")]
    public string Name { get; set; } = "";

    [Required]
    [Display(Name = "推送平台")]
    public string Platform { get; set; } = "email";

    [Display(Name = "執行頻率")]
    public string IntervalType { get; set; } = "manual";

    [Display(Name = "每天執行時間")]
    public string RunAtTime { get; set; } = "09:00";

    [Display(Name = "星期幾")]
    public int DayOfWeek { get; set; } = 1;

    [Display(Name = "每月幾號")]
    [Range(1, 31)]
    public int DayOfMonth { get; set; } = 1;

    [Display(Name = "只推送新資料（上次執行後新增）")]
    public bool NewItemsOnly { get; set; } = true;

    [Display(Name = "啟用")]
    public bool IsActive { get; set; } = true;

    // 篩選條件
    [Display(Name = "關鍵字")]
    public string? Query { get; set; }
    public List<int> SelectedDataTypeIds { get; set; } = [];
    public List<int> SelectedTopicIds { get; set; } = [];

    // View options
    public List<DataType> AllDataTypes { get; set; } = [];
    public List<TopicTreeNode> TopicTree { get; set; } = [];
}
