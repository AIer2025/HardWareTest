using System;

namespace LabTestPlatform.Data.Entities;

public class TestDataEntity
{
    public long TestId { get; set; }
    public int ModuleId { get; set; }
    public int? BatchId { get; set; }
    public DateTime TestTime { get; set; }
    public decimal TestValue { get; set; }
    public string? TestUnit { get; set; }
    public string TestType { get; set; } = string.Empty;
    public decimal? FailureTime { get; set; }
    public string? FailureMode { get; set; }
    public bool IsCensored { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public string? Operator { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreateTime { get; set; }
}
