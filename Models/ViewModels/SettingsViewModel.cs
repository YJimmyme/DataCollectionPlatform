using DataCollectionPlatform.Models;

namespace DataCollectionPlatform.Models.ViewModels;

public class SettingsViewModel
{
    public List<DataType> DataTypes { get; set; } = [];
    public List<TopicFlatRow> Topics { get; set; } = [];
    public List<DataType> ActiveDataTypes { get; set; } = [];
    public string ActiveTab { get; set; } = "datatypes";
}

public class TopicFlatRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? ParentId { get; set; }
    public string ParentName { get; set; } = "";
    public bool IsActive { get; set; }
    public int Level { get; set; }
}
