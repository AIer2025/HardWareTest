using System;

namespace LabTestPlatform.Data.Entities
{
    /// <summary>
    /// 系统实体类 - 对应数据库表 tb_system
    /// </summary>
    public class SystemEntity
    {
        /// <summary>
        /// 系统ID (主键)
        /// </summary>
        public int SystemId { get; set; }

        /// <summary>
        /// 系统编码 (唯一)
        /// </summary>
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 系统描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 安装位置
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public DateTime? InstallDate { get; set; }

        /// <summary>
        /// 质保截止日期
        /// </summary>
        public DateTime? WarrantyEndDate { get; set; }

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
