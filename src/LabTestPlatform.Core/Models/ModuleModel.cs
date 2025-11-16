namespace LabTestPlatform.Core.Models;

public class ModuleModel
{
    public int ModuleId { get; set; }
    public int PlatformId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleType { get; set; }
    public string? Manufacturer { get; set; }
    
    // 额外的显示属性
    public string PlatformName { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public int TestCount { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
}
