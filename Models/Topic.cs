using System.ComponentModel.DataAnnotations;

namespace DataCollectionPlatform.Models;

public class Topic
{
    public int Id { get; set; }

    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(200)]
    public string Name { get; set; } = "";

    public int? ParentId { get; set; }
    public Topic? Parent { get; set; }
    public ICollection<Topic> Children { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public ICollection<ItemTopic> ItemTopics { get; set; } = [];
}
