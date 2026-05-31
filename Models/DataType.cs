using System.ComponentModel.DataAnnotations;

namespace DataCollectionPlatform.Models;

public class DataType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(100)]
    public string Name { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public ICollection<ItemDataType> ItemDataTypes { get; set; } = [];
}
