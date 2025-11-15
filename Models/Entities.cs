using System;

namespace LabTestPlatform.Models
{
    /// <summary>
    /// 系统实体
    /// </summary>
    public class SystemEntity
    {
        public int SystemID { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public string SystemCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 平台实体
    /// </summary>
    public class PlatformEntity
    {
        public int PlatformID { get; set; }
        public int SystemID { get; set; }
        public string PlatformName { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Location { get; set; }
        public int? Capacity { get; set; }
        public DateTime? InstallDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string? Remarks { get; set; }
        
        // 导航属性
        public string? SystemName { get; set; }
    }

    /// <summary>
    /// 模组实体
    /// </summary>
    public class ModuleEntity
    {
        public int ModuleID { get; set; }
        public int PlatformID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Specifications { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? InstallDate { get; set; }
        public DateTime? WarrantyDate { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string? Remarks { get; set; }
        
        // 导航属性
        public string? PlatformName { get; set; }
        public string? SystemName { get; set; }
    }

    /// <summary>
    /// 测试数据实体
    /// </summary>
    public class TestDataEntity
    {
        public int TestID { get; set; }
        public int ModuleID { get; set; }
        public DateTime TestTime { get; set; }
        public string TestType { get; set; } = string.Empty;
        public double TestValue { get; set; }
        public string? Unit { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public double? Voltage { get; set; }
        public double? Current { get; set; }
        public double? Pressure { get; set; }
        public double? OperatingHours { get; set; }
        public int? CycleCount { get; set; }
        public string Status { get; set; } = "Normal";
        public string? Operator { get; set; }
        public string ImportSource { get; set; } = "Manual";
        public DateTime CreateTime { get; set; }
        public string? Remarks { get; set; }
        
        // 导航属性
        public string? ModuleName { get; set; }
        public string? ModuleCode { get; set; }
    }

    /// <summary>
    /// 威布尔分析结果实体
    /// </summary>
    public class WeibullAnalysisResultEntity
    {
        public int ResultID { get; set; }
        public int ModuleID { get; set; }
        public DateTime AnalysisTime { get; set; }
        public string AnalysisType { get; set; } = "Weibull";
        public int? DataCount { get; set; }
        public double? ShapeParameter { get; set; }  // β
        public double? ScaleParameter { get; set; }  // η
        public double? LocationParameter { get; set; } // γ
        public double? MTBF { get; set; }
        public double ConfidenceLevel { get; set; } = 0.95;
        public double? RSquared { get; set; }
        public string EstimationMethod { get; set; } = "MLE";
        public double? ReliabilityAt1000h { get; set; }
        public double? ReliabilityAt5000h { get; set; }
        public double? ReliabilityAt10000h { get; set; }
        public double? B10Life { get; set; }
        public double? B50Life { get; set; }
        public string? Analyst { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string? Remarks { get; set; }
        
        // 导航属性
        public string? ModuleName { get; set; }
        public string? ModuleCode { get; set; }
    }

    /// <summary>
    /// 分析报告实体
    /// </summary>
    public class AnalysisReportEntity
    {
        public int ReportID { get; set; }
        public int ResultID { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Weibull";
        public string ReportFormat { get; set; } = "PDF";
        public string? FilePath { get; set; }
        public long? FileSize { get; set; }
        public DateTime GenerateTime { get; set; }
        public string? Generator { get; set; }
        public string Status { get; set; } = "Generated";
        public DateTime CreateTime { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 用户实体
    /// </summary>
    public class UserEntity
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Operator";
        public string? Department { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
