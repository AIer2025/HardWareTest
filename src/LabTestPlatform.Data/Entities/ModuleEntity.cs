using System;

namespace LabTestPlatform.Data.Entities;

public class ModuleEntity
{
    public int ModuleId { get; set; }
    public int PlatformId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string? ModuleType { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool IsActive { get; set; }
}
