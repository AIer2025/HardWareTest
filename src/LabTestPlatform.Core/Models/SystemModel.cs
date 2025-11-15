namespace LabTestPlatform.Core.Models;

public class SystemModel
{
    public int SystemId { get; set; }
    public string SystemCode { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
}
