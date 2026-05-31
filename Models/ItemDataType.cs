namespace DataCollectionPlatform.Models;

public class ItemDataType
{
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int DataTypeId { get; set; }
    public DataType DataType { get; set; } = null!;
}
