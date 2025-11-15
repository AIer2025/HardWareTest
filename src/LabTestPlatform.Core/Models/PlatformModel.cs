namespace LabTestPlatform.Core.Models;

public class PlatformModel
{
    public int PlatformId { get; set; }
    public int SystemId { get; set; }
    public string PlatformCode { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string? PlatformType { get; set; }
}
