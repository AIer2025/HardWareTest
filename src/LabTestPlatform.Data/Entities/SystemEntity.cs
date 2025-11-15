using System;

namespace LabTestPlatform.Data.Entities;

public class SystemEntity
{
    public int SystemId { get; set; }
    public string SystemCode { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? InstallDate { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool IsActive { get; set; }
}
