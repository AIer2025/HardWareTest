namespace LabTestPlatform.Core.Models;

public class ModuleModel
{
    public int ModuleId { get; set; }
    public int PlatformId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleType { get; set; }
    public string? Manufacturer { get; set; }
}
