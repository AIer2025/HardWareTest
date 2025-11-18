using System;

namespace LabTestPlatform.Data.Entities
{
    /// <summary>
    /// 模组实体类 - 对应数据库表 tb_module
    /// </summary>
    public class ModuleEntity
    {
        /// <summary>
        /// 模组ID (主键)
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// 平台ID (外键)
        /// </summary>
        public int PlatformId { get; set; }

        /// <summary>
        /// 模组编码 (唯一)
        /// </summary>
        public string ModuleCode { get; set; } = string.Empty;

        /// <summary>
        /// 模组名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 模组类型 (POWER/SENSOR/CONTROLLER/MOTOR/DISPLAY/COMMUNICATION/STORAGE/OTHER)
        /// </summary>
        public string? ModuleType { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string? ModelNumber { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDate { get; set; }

        /// <summary>
        /// 额定寿命(小时)
        /// </summary>
        public int? RatedLife { get; set; }

        /// <summary>
        /// 模组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 是否启用 (1:启用, 0:禁用)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
