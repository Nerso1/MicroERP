namespace MicroERP.Domain.Entities;

public class Warehouse : IAuditable
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}
