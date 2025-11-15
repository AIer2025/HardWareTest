using System;

namespace LabTestPlatform.Data.Entities;

public class PlatformEntity
{
    public int PlatformId { get; set; }
    public int SystemId { get; set; }
    public string PlatformCode { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string? PlatformType { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool IsActive { get; set; }
}
